using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using UnityEngine;

namespace GrpcWebUnity.Internal
{
    internal class WebGLCall
    {
        public readonly WebGlChannel Channel;
        private GrpcWebConnector Connector => Channel.Connector;

        public MethodType? CallType { get; private set; }
        public int CallKey { get; private set; } = -1;

        private TaskCompletionSource<Metadata> _headers = new TaskCompletionSource<Metadata>();
        private TaskCompletionSource<IMessage> _unaryResponse;
        private AsyncStreamReader _streamReader;

        private Status _status = new Status(StatusCode.Unknown, "");
        private Metadata _trailers;

        private Type _requestType;
        private Type _responseType;



        public WebGLCall(WebGlChannel webGlChannel)
        {
            Channel = webGlChannel;
        }

        private void SubmitCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host,
            CallOptions options, TRequest request)
        {
            CallType = method.Type;

            var iMessageType = typeof(IMessage);
            _requestType = typeof(TRequest);
            _responseType = typeof(TResponse);

            var areValidTypes = iMessageType.IsAssignableFrom(_requestType) &&
                                iMessageType.IsAssignableFrom(_responseType);

            if (!areValidTypes) throw new Exception("Only IMessage Supported");

            string base64Request = null;

            if (request is IMessage reqMessage)
            {
                var requestBytes = reqMessage.ToByteArray();
                base64Request = Convert.ToBase64String(requestBytes);
            }

            var headers = options.Headers?.EncodeMetadata();
            var deadline = options.Deadline.ToUnixTimeSeconds();

            CallKey = CallType switch
            {
                MethodType.Unary => GrpcWebApi.UnaryRequest(Connector.InstanceKey,
                    Channel.ChannelKey, method.ServiceName, method.Name, headers, base64Request, deadline),
                MethodType.ServerStreaming => GrpcWebApi.ServerStreamingRequest(Connector.InstanceKey,
                    Channel.ChannelKey, method.ServiceName, method.Name, headers, base64Request, deadline),
                _ => throw new ArgumentOutOfRangeException(nameof(CallType), CallType,
                    "GrpcWeb does not allow for client streaming.")
            };
            Channel.Calls.Add(CallKey, this);
        }

        public AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {

            SubmitCall(method, host, options, request);

            _unaryResponse = new TaskCompletionSource<IMessage>();

            var call = new AsyncUnaryCall<TResponse>(
                _unaryResponse.Task.ContinueWithSync(it => (TResponse)it.Result),
                _headers.Task,
                () => _status,
                () => _trailers,
                Cancel
            );
            options.CancellationToken.Register(call.Dispose);
            return call;
        }


        public AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string host,
            CallOptions options,
            TRequest request)
        {
            SubmitCall(method, host, options, request);

            _streamReader = new AsyncStreamReader<TResponse>(options.CancellationToken);

            var call = new AsyncServerStreamingCall<TResponse>(
                _streamReader as IAsyncStreamReader<TResponse>,
                _headers.Task,
                () => _status,
                () => _trailers,
                Cancel
            );

            options.CancellationToken.Register(call.Dispose);
            return call;
        }


        internal void ReportHeaders(Metadata headers)
        {
            _headers.TrySetResult(headers);
        }

        internal void ReportServerStreamingResponse(byte[] messageEncoded)
        {
            var obj = Deserializer(messageEncoded);
            _streamReader.AddItem(obj);
        }

        internal void ReportCompleted(Status status, Metadata trailers, byte[] messageEncoded = null)
        {
            if (status.StatusCode != StatusCode.OK)
            {
                _trailers = trailers;
                IssueError(new RpcException(status));
                return;
            }

            _status = status;
            _trailers = trailers;

            switch (CallType)
            {
                case MethodType.Unary:
                case MethodType.ClientStreaming:
                    if (messageEncoded == null)
                    {
                        var exception = new RpcException(status, $"{nameof(WebGlChannel)}: Call returned no response");
                        IssueError(exception);
                        throw exception;
                    }

                    _unaryResponse.SetResult(Deserializer(messageEncoded));
                    break;
                case MethodType.ServerStreaming:
                case MethodType.DuplexStreaming:
                    _streamReader.SignalEnd();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CallType), CallType, "Unsupported CallType?");
            }
        }


        private void Cancel()
        {
            GrpcWebApi.CancelCall(Connector.InstanceKey, Channel.ChannelKey, CallKey);
            IssueError(new RpcException(Status.DefaultCancelled, $"{nameof(WebGlChannel)}:Client cancelled the call."));
        }

        private IMessage Deserializer(byte[] messageEncoded)
        {
            var responseInstance = (IMessage)Activator.CreateInstance(_responseType);
            responseInstance.MergeFrom(messageEncoded);
            return responseInstance;
        }


        private void IssueError(RpcException exception)
        {
            _status = exception.Status;
            _streamReader?.SignalError(exception);
            _unaryResponse?.TrySetException(exception);
            _headers.TrySetException(exception);
        }
    }
}
