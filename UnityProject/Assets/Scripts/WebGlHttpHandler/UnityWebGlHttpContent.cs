using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JsInterop;
using JsInterop.Internal;
using JsInterop.Types;
using UnityEngine;

internal sealed class UnityWebGlHttpContent : HttpContent
{
    private byte[] _data;
    private readonly UnityWebGlFetchResponse _status;

    public UnityWebGlHttpContent(UnityWebGlFetchResponse status)
    {
        Debug.Log(2000);
        _status = status ?? throw new ArgumentNullException(nameof(status));
    }

    private async Task<byte[]> GetResponseData(CancellationToken cancellationToken)
    {
        Debug.Log(100);
        if (_data != null)
        {
        Debug.Log(102);
            return _data;
        }
        try
        {
        Debug.Log(103);
            var dataBufferVal = await _status.ArrayBuffer().Task.ConfigureAwait(continueOnCapturedContext: true);

        Debug.Log(104);
            using (var dataBuffer = dataBufferVal.As<JsObject>())
            {
        Debug.Log(105);
                using (var dataBinView = Runtime.CreateHostObject("Uint8Array", dataBuffer).As<JsTypedArray>())
                {
        Debug.Log(106);
                    _data = dataBinView.GetDataCopy<byte>();
        Debug.Log(107);
                    _status.Dispose();
                }
            }
        }
        catch (JsException jse)
        {
        Debug.Log(108);
            throw UnityWebGlHttpHandler.TranslateJSException(jse, cancellationToken);
        }

        return _data;
    }

    protected override async Task<Stream> CreateContentReadStreamAsync()
    {
        Debug.Log(109);
        byte[] data = await GetResponseData(CancellationToken.None).ConfigureAwait(continueOnCapturedContext: true);
        Debug.Log(110);
        return new MemoryStream(data, writable: false);
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) =>
        SerializeToStreamAsync(stream, context, CancellationToken.None);

    async Task SerializeToStreamAsync(Stream stream, TransportContext context, CancellationToken cancellationToken)
    {
        Debug.Log(111);
        byte[] data = await GetResponseData(cancellationToken).ConfigureAwait(continueOnCapturedContext: true);
        Debug.Log(112);
        await stream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(continueOnCapturedContext: true);
    }
    protected override bool TryComputeLength(out long length)
    {
        Debug.Log(113);
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
        Debug.Log(114);
        _status?.Dispose();
        base.Dispose(disposing);
    }
}
