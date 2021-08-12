using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
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

        private TaskCompletionSource<IMessage> _unaryResponse;
        private AsyncStreamReader _streamReader;
        private Func<byte[], IMessage> _deserializer;

        private TaskCompletionSource<Metadata> _headers = new TaskCompletionSource<Metadata>();
        
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            Debug.Log("EARARAR");
            if (!(request is IMessage reqMessage))
            {
                throw new Exception("Only IMessage Supported");
            }

            var requestBytes = reqMessage.ToByteArray();

            _deserializer = bytes => {
                var responseInstance = (IMessage)Activator.CreateInstance<TResponse>();
                responseInstance.MergeFrom(bytes);
                return responseInstance;
            };

            var base64Request = Convert.ToBase64String(requestBytes);
            var headers = options.Headers?.EncodeMetadata();
            var deadline = options.Deadline.ToUnixTimeSeconds();
            var callKey = GrpcWebApi.UnaryRequest(Connector.InstanceKey, Channel.ChannelKey, method.ServiceName, method.Name, headers, base64Request, deadline);

            Debug.Log("rararara");
            Channel.Calls.Add(callKey, this);
            Debug.Log("Registered Unary Call: " + callKey);

            _unaryResponse = new TaskCompletionSource<IMessage>();
            var call = new AsyncUnaryCall<TResponse>(
                _unaryResponse.Task.ContinueWith(it => (TResponse)it.Result),
                _headers.Task,
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
            _streamReader = reader;

            var call = new AsyncServerStreamingCall<TResponse>(
                reader,
                _headers.Task,
                () => Status,
                () => null,
                () =>
                {
                    GrpcWebApi.CancelCall(Connector.InstanceKey, Channel.ChannelKey, callKey);
                    _streamReader.SignalEnd();
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
            var obj = _deserializer(messageEncoded);
            _unaryResponse.SetResult(obj);
        }

        public void ReportHeaders(Metadata headers)
        {
            _headers.SetResult(headers);
        }

        public void ReportServerStreamingResponse(byte[] messageEncoded)
        {
            var obj = _deserializer(messageEncoded);
            _streamReader.AddItem(obj);
        }

        public void ReportCompleted()
        {
            _streamReader.SignalEnd();
        }

        public void ReportError(string errorMessage)
        {
            var exception = new Exception(errorMessage);
            _streamReader?.SignalError(exception);
            _unaryResponse?.SetException(exception);
            _headers.SetException(exception);
        }

        public void ReportStatus(Status status)
        {
            this.Status = status;
        }
    }
}
