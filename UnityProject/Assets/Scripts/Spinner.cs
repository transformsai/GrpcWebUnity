using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Ai.Transforms.Grpcwebunity;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using JsInterop;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

public class Spinner : MonoBehaviour
{
    private int i = 0;
    TestService.TestServiceClient client;

    // Update is called once per frame
    async void Awake()
    {

#if UNITY_WEBGL && !UNITY_EDITOR
        Runtime.Initialize();
        var innerHandler = new UnityWebGlHttpHandler();
        innerHandler.BeforeSend += message =>
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/grpc-web-text"));
        var handler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, innerHandler);
#else
        var innerHandler = new HttpClientHandler();
        var handler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, innerHandler);
#endif

        var channel = GrpcChannel.ForAddress("http://localhost:8001", new GrpcChannelOptions { HttpHandler = handler, LoggerFactory = new UnityLoggerFactory(), });

        client = new TestService.TestServiceClient(channel);
        PrintUnary();
        return;
        var call = client.ServerStream(new Request { Data = "cheem" });
        var headers = await call.ResponseHeadersAsync;
        Debug.Log($"HeadersStream: <{string.Join(", ", headers)}>");

        var stream = call.ResponseStream;
        while (await stream.MoveNext(CancellationToken.None))
        {
            var item = stream.Current;
            Debug.Log("Success! " + item.Data);
        }
    }
    private void Update()
    {
        if (i++ % 200 == 0)
        {
            ;
        }
        transform.Rotate(Vector3.up, 1f);
    }

    private async void PrintUnary()
    {
        using var call = client.UnaryAsync(new Request { Data = "earaara" + i });
        var headers = await call.ResponseHeadersAsync;
        Debug.Log($"HeadersUnary: <{string.Join(", ", headers)}>");
        var response = await call.ResponseAsync.ConfigureAwait(continueOnCapturedContext: true);
        Debug.Log(response.Data);
    }

    class UnityLoggerFactory : ILoggerFactory
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            return new UnityLogger(categoryName);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotSupportedException();
        }

        private class UnityLogger : ILogger
        {
            private readonly string _categoryName;
            private List<object> scopeObjects = new List<object>();

            public UnityLogger(string categoryName)
            {
                _categoryName = categoryName;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (formatter == null) formatter = (s, e) => $"{eventId} {s} : {e}";

                switch (logLevel)
                {
                    case LogLevel.None:
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                        Debug.Log(formatter(state, exception));
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(formatter(state, exception));
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        Debug.LogError(formatter(state, exception));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                }
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state)
            {
                return new LogScope<TState>(this, state);
            }

            private readonly struct LogScope<T> : IDisposable
            {
                private readonly UnityLogger _unityLogger;
                private readonly T _state;

                public LogScope(UnityLogger unityLogger, T state)
                {
                    _unityLogger = unityLogger;
                    _state = state;
                    _unityLogger.scopeObjects.Add(state);
                }

                public void Dispose()
                {
                    var state = _state;
                    var objects = _unityLogger.scopeObjects;
                    var index = objects.FindLastIndex(it => it.Equals(state));
                    if (index >= 0) objects.RemoveAt(index);

                }
            }

        }
    }
}
