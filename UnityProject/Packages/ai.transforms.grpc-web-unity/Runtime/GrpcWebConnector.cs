using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

namespace GrpcWebUnity
{
    public class GrpcWebConnector : MonoBehaviour
    {

        internal int InstanceKey { get; private set; } = -1;
        private readonly TaskCompletionSource<int> _initializationTask = new TaskCompletionSource<int>();
        private bool IsInitialized => InstanceKey >= 0;
        public Task WaitForInitialization => _initializationTask.Task;
        private readonly Dictionary<int, WebGlChannel> _channelMap = new Dictionary<int, WebGlChannel>();
        private static GrpcWebConnector _instance;

        public static GrpcWebConnector Instance
        {
            get
            {
                if (_instance) return _instance;
                var newInstance = new GameObject().AddComponent<GrpcWebConnector>();
                Debug.Assert(newInstance == _instance, "should be set in Awake");
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance)
            {
                Debug.LogError("Duplicate connectors");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            GameObject obj = gameObject;

            obj.name = GrpcWebApi.RegisterInstance();
            DontDestroyOnLoad(obj);
            Debug.Log("Created GrpcWebConnector: " + obj.name);


        }

        public async Task<ChannelBase> MakeChannelAsync(string target)
        {
            await WaitForInitialization;
            return MakeChannel(target);
        }

        public ChannelBase MakeChannel(string target)
        {
            if (!IsInitialized)
                throw new Exception($"Connector not initialized yet. Wait for {nameof(WaitForInitialization)}");

            var channelKey = GrpcWebApi.RegisterChannel(InstanceKey, target);
            Debug.Log("Registered Channel: " + channelKey);
            var channel = new WebGlChannel(this, target, channelKey);
            _channelMap[channelKey] = channel;
            return channel;
        }


        // Receives a ServerStreamingResponse Unary response in the format "channelKey|callKey|base64EncodedMessage"
        internal void OnServerStreamingResponse(string channelCallMessage)
        {
            ParseParams(channelCallMessage, out _, out var call, out var parameters);
            var messageEncoded = Convert.FromBase64String(parameters[2]);
            call.ReportServerStreamingResponse(messageEncoded);
        }

        // Receives a pipe-delimited Unary response in the format "channelKey|callKey|statusCode|statusDetail|trailingMetadata|base64EncodedMessage"
        // The final parameter may be ommited if the call is not expected to return a final message.
        internal void OnCallCompletion(string channelCallStatusDetailMessage)
        {

            ParseParams(channelCallStatusDetailMessage, out _, out var call, out var parameters);
            var code = (StatusCode) int.Parse(parameters[2]);
            var detail = Utils.FromBase64Utf8(parameters[3]);
            var trailersString = Utils.FromBase64Utf8(parameters[4]);
            var messageEncoded = parameters.Length < 6 ? null : Convert.FromBase64String(parameters[5]);
            var status = new Status(code, detail);
            var trailers = Utils.DecodeMetadata(new StringReader(trailersString));
            call.ReportCompleted(status, trailers, messageEncoded);

        }

        // Takes multi-line Metadata. The first line is expected to have the format `channelKey|callKey`
        // followed by 0 or more lines with the format `base64EncodedKey|base64EncodedValue`. Each key-value pair is of the format.
        // If the key ends in `-bin`, it is expected to be a binary value. 
        internal void OnHeaders(string channelCallMetadata)
        {
            using var reader = new StringReader(channelCallMetadata);
            var first = reader.ReadLine();
            if (first == null) throw new Exception("Invalid Headers");
            ParseParams(first, out _, out var call, out _);
            var headers = Utils.DecodeMetadata(reader);
            call.ReportHeaders(headers);
        }


        // Signals that the connector has been initialized
        internal void OnInstanceRegistered(int instanceKey)
        {
            InstanceKey = instanceKey;
            _initializationTask.SetResult(instanceKey);
        }


        private void ParseParams(string channelKeyMessage, out WebGlChannel channel, out WebGLCall call,
            out string[] parameters)
        {
            parameters = channelKeyMessage.Split('|');
            var channelKey = int.Parse(parameters[0]);
            channel = _channelMap[channelKey];
            call = parameters.Length > 1 ? channel.Calls[int.Parse(parameters[1])] : null;

        }
    }

}
