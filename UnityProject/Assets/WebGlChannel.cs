using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

internal class WebGlChannel : ChannelBase
{
    public readonly GrpcWebConnector Connector;
    public readonly int ChannelKey;

    public WebGlChannel(GrpcWebConnector connector, string target, int channelKey) : base(target)
    {
        Connector = connector;
        ChannelKey = channelKey;
    }

    internal readonly Dictionary<int, WebGLCallInvoker> Calls = new Dictionary<int, WebGLCallInvoker>();

    public override CallInvoker CreateCallInvoker() => new WebGLCallInvoker(this);

    public class WebGLCallInvoker : CallInvoker
    {
        public readonly WebGlChannel Channel;
        public GrpcWebConnector Connector => Channel.Connector;
        public Status Status = new Status(StatusCode.Unknown, "");
        public WebGLCallInvoker(WebGlChannel webGlChannel)
        {
            Channel = webGlChannel;
        }

        private TaskCompletionSource<object> UnaryResponse;
        private AsyncStreamReader streamReader;
        private Func<byte[], object> Deserializer;

        private TaskCompletionSource<Metadata> Headers = new TaskCompletionSource<Metadata>();
        
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var requestBytes = method.RequestMarshaller.Serializer(request);
            Deserializer = method.RequestMarshaller.Deserializer;
            var base64Request = Convert.ToBase64String(requestBytes);
            var headers = options.Headers?.EncodeMetadata();
            var deadline = options.Deadline.ToUnixTimeSeconds();
            var callKey = GrpcWebApi.UnaryRequest(Connector.InstanceKey, Channel.ChannelKey, method.ServiceName, method.Name, headers, base64Request, deadline);

            Channel.Calls.Add(callKey, this);
            Debug.Log("Registered Unary Call: " + callKey);

            UnaryResponse = new TaskCompletionSource<object>();
            var call = new AsyncUnaryCall<TResponse>(
                UnaryResponse.Task.ContinueWith(it => (TResponse)it.Result),
                Headers.Task,
                () => Status,
                () => null,
                () => GrpcWebApi.CancelCall(Connector.InstanceKey, Channel.ChannelKey, callKey)
            );

            options.CancellationToken.Register(call.Dispose);
            return call;
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string host,
            CallOptions options,
            TRequest request)
        {
            var requestBytes = method.RequestMarshaller.Serializer(request);
            var base64Request = Convert.ToBase64String(requestBytes);
            var headers = options.Headers?.EncodeMetadata();
            var deadline = options.Deadline.ToUnixTimeSeconds();
            var callKey = GrpcWebApi.ServerStreamingRequest(Connector.InstanceKey, Channel.ChannelKey, method.ServiceName, method.Name, headers, base64Request, deadline);
            
            Debug.Log("Registered Streaming Call: " + callKey);


            var reader = new AsyncStreamReader<TResponse>();
            streamReader = reader;

            var call = new AsyncServerStreamingCall<TResponse>(
                reader,
                Headers.Task,
                () => Status,
                () => null,
                () =>
                {
                    GrpcWebApi.CancelCall(Connector.InstanceKey, Channel.ChannelKey, callKey);
                    streamReader.SignalEnd();
                }
            );
            options.CancellationToken.Register(call.Dispose);
            return call;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request) =>
            throw new NotImplementedException("UnityWebGL does not allow for blocking calls. Callback happens on main thread.");

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options) =>
            throw new NotImplementedException("GrpcWeb does not allow for client streaming.");

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options) =>
            throw new NotImplementedException("GrpcWeb does not allow for client streaming.");

        public void ReportUnaryResponse(byte[] messageEncoded)
        {
            var obj = Deserializer(messageEncoded);
            UnaryResponse.SetResult(obj);
        }

        public void ReportHeaders(Metadata headers)
        {
            Headers.SetResult(headers);
        }

        public void ReportServerStreamingResponse(byte[] messageEncoded)
        {
            var obj = Deserializer(messageEncoded);
            streamReader.AddItem(obj);
        }

        public void ReportCompleted()
        {
            streamReader.SignalEnd();
        }

        public void ReportError(string errorMessage)
        {
            var exception = new Exception(errorMessage);
            streamReader?.SignalError(exception);
            UnaryResponse?.SetException(exception);
            Headers.SetException(exception);
        }

        public void ReportStatus(Status status)
        {
            this.Status = status;
        }
    }
}
