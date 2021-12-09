using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JsInterop.Internal;
using JsInterop.Types;
using UnityEngine;

internal sealed class UnityWebGlHttpReadStream : Stream
{
    private UnityWebGlFetchResponse _status;
    private JsObject _reader;

    private byte[] _bufferedBytes;
    private int _position;

    public UnityWebGlHttpReadStream(UnityWebGlFetchResponse status)
    {
        Debug.Log(1000);
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
        Debug.Log(1);
        if (_reader == null)
        {
            // If we've read everything, then _reader and _status will be null
            if (_status == null)
            {
                Debug.Log(2);
                return 0;
            }

            try
            {
                Debug.Log(3);
                using (JsObject body = _status.Body)
                {
                    Debug.Log(4);
                    _reader = body.Invoke("getReader").As<JsObject>();
                }
            }
            catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested)
            {
                Debug.Log(5);
                throw new OperationCanceledException("Request cancelled:", oce, cancellationToken);
            }
            catch (JsException jse)
            {
                Debug.Log(6);
                throw UnityWebGlHttpHandler.TranslateJSException(jse, cancellationToken);
            }
        }

        if (_bufferedBytes != null && _position < _bufferedBytes.Length)
        {
            Debug.Log(7);
            return ReadBuffered();
        }

        try
        {
            Debug.Log(8);
            var t = _reader.Invoke("read").As<JsPromise>();
            Debug.Log(9);
            var readVal = await t.Task.ConfigureAwait(continueOnCapturedContext: true);
            Debug.Log(10);
            using (var read = readVal.As<JsObject>())
            {
                Debug.Log(11);
                if ((bool)read.GetProp("done"))
                {
                    Debug.Log(12);
                    _reader.Dispose();
                    _reader = null;

                    _status?.Dispose();
                    _status = null;
                    Debug.Log(13);
                    return 0;
                }

                _position = 0;
                // value for fetch streams is a Uint8Array
                Debug.Log(14);
                using (var binValue = read.GetProp("value").As<JsTypedArray>())
                    _bufferedBytes = binValue.GetDataCopy<byte>();
            }
        }
        catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested)
        {
            Debug.Log(15);
            throw new OperationCanceledException("Request cancelled:", oce, cancellationToken);
        }
        catch (JsException jse)
        {
            Debug.Log(16);
            throw UnityWebGlHttpHandler.TranslateJSException(jse, cancellationToken);
        }

        Debug.Log(17);
        return ReadBuffered();

        int ReadBuffered()
        {
            Debug.Log(18);
            int n = Math.Min(_bufferedBytes.Length - _position, count);
            Debug.Log(19);
            if (n <= 0)
            {
                Debug.Log(20);
                return 0;
            }

            Debug.Log(21);
            Array.Copy(_bufferedBytes, _position, buffer, offset, n);
            Debug.Log(22);
            _position += n;

            Debug.Log(23);
            return n;
        }
    }

    protected override void Dispose(bool disposing)
    {
        _reader?.Dispose();
        _status?.Dispose();
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("SR.net_http_synchronous_reads_not_supported");
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
