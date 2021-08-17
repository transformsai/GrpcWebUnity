using System;
using System.Collections.Generic;
using Grpc.Core;
using UnityEngine;

namespace GrpcWebUnity.Internal
{
    internal class WebGlChannel : ChannelBase
    {
        public readonly GrpcWebConnector Connector;
        public readonly int ChannelKey;

        public WebGlChannel(GrpcWebConnector connector, string target, int channelKey) : base(target)
        {
            Connector = connector;
            ChannelKey = channelKey;
        }

        internal readonly Dictionary<int, WebGLCall> Calls = new Dictionary<int, WebGLCall>();

        public override CallInvoker CreateCallInvoker()
        {
            return new WebGLCallInvoker(this);
        }
    }

    internal class WebGLCallInvoker : CallInvoker
    {
        private WebGlChannel Channel { get; }

        public WebGLCallInvoker(WebGlChannel webGlChannel)
        {
            Channel = webGlChannel;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var call = new WebGLCall(Channel);
            return call.AsyncUnaryCall(method, host, options, request);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options,
            TRequest request)
        {
            var call = new WebGLCall(Channel);
            return call.AsyncServerStreamingCall(method, host, options, request);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method,
            string host, CallOptions options, TRequest request) =>
            throw new NotImplementedException(
                "UnityWebGL does not allow for blocking calls. Callback happens on main thread.");

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method, string host, CallOptions options) =>
            throw new NotImplementedException("GrpcWeb does not allow for client streaming.");

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method, string host, CallOptions options) =>
            throw new NotImplementedException();



    }
}

