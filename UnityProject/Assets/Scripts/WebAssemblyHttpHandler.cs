using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Enum = System.Enum;

/// <see cref="WebAssemblyHttpHandler" /> is a specialty message handler based on the
/// Fetch API for use in WebAssembly environments.
/// </summary>
/// <remarks>See https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API</remarks>
public class WebAssemblyHttpHandler : HttpMessageHandler
{
    static JsReference fetch;
    static JsReference window;

    /// <summary>
    /// Gets whether the current Browser supports streaming responses
    /// </summary>
    private static bool StreamingSupported { get; }

    static WebAssemblyHttpHandler()
    {
        var streamingSupported = JsFunction.Create(
                "return typeof Response !== 'undefined' && 'body' in Response.prototype && typeof ReadableStream === 'function'");
        StreamingSupported = streamingSupported.Call();
    }

    public WebAssemblyHttpHandler()
    {
        handlerInit();
    }

    private void handlerInit()
    {
        window = JsReference.GetGlobalObject("window");
        fetch = JsReference.GetGlobalObject("fetch");
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // There is a race condition on Safari as a result of using TaskCompletionSource that
        // causes a stack exceeded error being thrown.  More information can be found here:
        // https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/
        var tcs = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (cancellationToken.Register(() => tcs.TrySetCanceled()))
        {
#pragma warning disable 4014
            doFetch(tcs, request, cancellationToken).ConfigureAwait(false);
#pragma warning restore 4014

            return await tcs.Task;
        }
    }

    private async Task doFetch(TaskCompletionSource<HttpResponseMessage> tcs, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var requestObject = JsObject.Create();

            if (request.Properties.TryGetValue("WebAssemblyFetchOptions", out var fetchOoptionsValue) &&
                fetchOoptionsValue is IDictionary<string, object> fetchOptions)
            {
                foreach (var item in fetchOptions)
                {
                    requestObject.SetProp(item.Key, JsReference.FromObject(item.Value));
                }
            }

            requestObject.SetProp("method", request.Method.Method);

            // We need to check for body content
            if (request.Content != null)
            {
                if (request.Content is StringContent)
                {
                    requestObject.SetProp("body", await request.Content.ReadAsStringAsync());
                }
                else
                {
                    // 2.1.801 seems to have a problem with the line
                    // using (var uint8Buffer = Uint8Array.From(await request.Content.ReadAsByteArrayAsync ()))
                    // so we split it up into two lines.
                    var byteAsync = await request.Content.ReadAsByteArrayAsync();
                    using (var uint8Buffer = JsTypedArray.CreateShared(byteAsync))
                    {
                        requestObject.SetProp("body", uint8Buffer);
                    }
                }
            }

            // Process headers
            // Cors has it's own restrictions on headers.
            // https://developer.mozilla.org/en-US/docs/Web/API/Headers
            var jsHeaders = JsReference.CreateHostObject("Headers");
            {
                if (request.Headers != null)
                {
                    foreach (var header in request.Headers)
                    {
                        foreach (var value in header.Value)
                        {
                            jsHeaders.Invoke("append", header.Key, value);
                        }
                    }
                }
                if (request.Content?.Headers != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        foreach (var value in header.Value)
                        {
                            jsHeaders.Invoke("append", header.Key, value);
                        }
                    }
                }
                requestObject.SetProp("headers", jsHeaders);
            }

            WasmHttpReadStream wasmHttpReadStream = null;

            JsReference abortController = JsReference.CreateHostObject("AbortController");
            JsReference signal = abortController.GetProp("signal");
            requestObject.SetProp("signal", signal);

            CancellationTokenSource abortCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancellationTokenRegistration abortRegistration = abortCts.Token.Register(() =>
            {
                abortController.Invoke("abort");
                wasmHttpReadStream?.Dispose();
            });

            var args = JsArray.CreateEmpty();
            args.Add(request.RequestUri.ToString());
            args.Add(requestObject);

            var response = fetch.Invoke("apply", window, args).As<JsPromise>();

            var t = await response;

            var status = new WasmFetchResponse(t, abortController, abortCts, abortRegistration);

            //Console.WriteLine($"bodyUsed: {status.IsBodyUsed}");
            //Console.WriteLine($"ok: {status.IsOK}");
            //Console.WriteLine($"redirected: {status.IsRedirected}");
            //Console.WriteLine($"status: {status.Status}");
            //Console.WriteLine($"statusText: {status.StatusText}");
            //Console.WriteLine($"type: {status.ResponseType}");
            //Console.WriteLine($"url: {status.Url}");

            HttpResponseMessage httpresponse = new HttpResponseMessage((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), status.Status.ToString()));

            var streamingEnabled = request.Properties.TryGetValue("WebAssemblyEnableStreamingResponse", out var streamingEnabledValue) && (bool)streamingEnabledValue;

            httpresponse.Content = StreamingSupported && streamingEnabled
                ? new StreamContent(wasmHttpReadStream = new WasmHttpReadStream(status))
                : (HttpContent)new WasmHttpContent(status);

            // Fill the response headers
            // CORS will only allow access to certain headers.
            // If a request is made for a resource on another origin which returns the CORs headers, then the type is cors.
            // cors and basic responses are almost identical except that a cors response restricts the headers you can view to
            // `Cache-Control`, `Content-Language`, `Content-Type`, `Expires`, `Last-Modified`, and `Pragma`.
            // View more information https://developers.google.com/web/updates/2015/03/introduction-to-fetch#response_types
            //
            // Note: Some of the headers may not even be valid header types in .NET thus we use TryAddWithoutValidation
            var respHeaders = status.Headers;
            if (!respHeaders)
            {
                var entriesIterator = respHeaders.Invoke("entries");
                JsReference nextResult = null;
                nextResult = entriesIterator.Invoke("next");
                while (!nextResult.GetProp("done"))
                {
                    var resultValue = nextResult.GetProp("value").As<JsArray>();
                    {
                        var name = resultValue[0].As<string>();
                        var value = resultValue[1].As<string>();
                        if (!httpresponse.Headers.TryAddWithoutValidation(name, value))
                            if (httpresponse.Content != null)
                                if (!httpresponse.Content.Headers.TryAddWithoutValidation(name, value))
                                    Console.WriteLine(
                                        $"Warning: Can not add response header for name: {name} value: {value}");
                    }
                    nextResult = entriesIterator.Invoke("next");
                }
            }

            tcs.SetResult(httpresponse);
        }
        catch (Exception exception)
        {
            var httpExc = new HttpRequestException(exception.Message);
            tcs.SetException(httpExc);
        }
    }

    class WasmFetchResponse : IDisposable
    {
        private JsReference fetchResponse;
        private JsReference abortController;
        private readonly CancellationTokenSource abortCts;
        private readonly CancellationTokenRegistration abortRegistration;

        public WasmFetchResponse(JsReference fetchResponse, JsReference abortController, CancellationTokenSource abortCts, CancellationTokenRegistration abortRegistration)
        {
            this.fetchResponse = fetchResponse;
            this.abortController = abortController;
            this.abortCts = abortCts;
            this.abortRegistration = abortRegistration;
        }

        public bool IsOK => fetchResponse.GetProp("ok").As<bool>();
        public bool IsRedirected => fetchResponse.GetProp("redirected").As<bool>();
        public int Status => fetchResponse.GetProp("status").As<int>();
        public string StatusText => fetchResponse.GetProp("statusText").As<string>();
        public string ResponseType => fetchResponse.GetProp("type").As<string>();
        public string Url => fetchResponse.GetProp("url").As<string>();
        //public bool IsUseFinalURL => (bool)managedJSObject.GetObjectProperty("useFinalUrl");
        public bool IsBodyUsed => fetchResponse.GetProp("bodyUsed").As<bool>();
        public JsReference Headers => fetchResponse.GetProp("headers");
        public JsReference Body => fetchResponse.GetProp("body");

        public JsPromise ArrayBuffer() => fetchResponse.Invoke("arrayBuffer").As<JsPromise>();
        public JsPromise Text() => fetchResponse.Invoke("text").As<JsPromise>();
        public JsPromise JSON() => fetchResponse.Invoke("json").As<JsPromise>();

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            // Free any other managed objects here.
            abortCts.Cancel();
            abortRegistration.Dispose();
        }

    }

    class WasmHttpContent : HttpContent
    {
        byte[] _data;
        WasmFetchResponse _status;

        public WasmHttpContent(WasmFetchResponse status)
        {
            _status = status;
        }

        private async Task<byte[]> GetResponseData()
        {
            if (_data != null)
            {
                return _data;
            }

            var dataBuffer = await _status.ArrayBuffer();
            using (var dataBinView = JsReference.CreateHostObject("Uint8Array",dataBuffer).As<JsTypedArray>())
            {
                _data = dataBinView.GetDataCopy<byte>();
                _status.Dispose();
                _status = null;

            }

            return _data;
        }

        protected override async Task<Stream> CreateContentReadStreamAsync()
        {
            var data = await GetResponseData();
            return new MemoryStream(data, writable: false);
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var data = await GetResponseData();
            await stream.WriteAsync(data, 0, data.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            if (_data != null)
            {
                length = _data.Length;
                return true;
            }

            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            _status?.Dispose();
            base.Dispose(disposing);
        }
    }

    class WasmHttpReadStream : Stream
    {
        WasmFetchResponse _status;
        JsReference _reader;

        byte[] _bufferedBytes;
        int _position;

        public WasmHttpReadStream(WasmFetchResponse status)
        {
            _status = status;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count < 0 || buffer.Length - offset < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (_reader == null)
            {
                // If we've read everything, then _reader and _status will be null
                if (_status == null)
                {
                    return 0;
                }

                try
                {
                    var body = _status.Body;
                    _reader = body.Invoke("getReader");
                }
                catch (Exception)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
            }

            if (_bufferedBytes != null && _position < _bufferedBytes.Length)
            {
                return ReadBuffered();
            }

            try
            {
                var t = _reader.Invoke("read").As<JsPromise>();
                var read = await t;
                {
                    if ((bool)read.GetProp("done"))
                    {
                        _reader = null;

                        _status.Dispose();
                        _status = null;
                        return 0;
                    }

                    _position = 0;
                    // value for fetch streams is a Uint8Array
                    using var binValue = read.GetProp("value").As<JsTypedArray>();
                    _bufferedBytes = binValue.GetDataCopy<byte>();
                }
            }
            catch (Exception)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw;
            }

            return ReadBuffered();

            int ReadBuffered()
            {
                int n = _bufferedBytes.Length - _position;
                if (n > count)
                    n = count;
                if (n <= 0)
                    return 0;

                Buffer.BlockCopy(_bufferedBytes, _position, buffer, offset, n);
                _position += n;

                return n;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _status?.Dispose();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new PlatformNotSupportedException("Synchronous reads are not supported, use ReadAsync instead");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
