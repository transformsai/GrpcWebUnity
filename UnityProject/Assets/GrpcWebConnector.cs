using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

public class GrpcWebConnector : MonoBehaviour
{

    internal int InstanceKey { get; private set; } = -1;
    private readonly TaskCompletionSource<int> _initializationTask = new TaskCompletionSource<int>();
    private bool IsInitialized => InstanceKey < 0;
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


    // Receives a pipe-delimited Unary response in the format "channelKey|callKey|base64EncodedMessage"
    internal void OnUnaryResponse(string channelCallMessage)
    {
        ParseParams(channelCallMessage, out _, out var call, out var payload);
        var messageEncoded = Convert.FromBase64String(payload);
        call.ReportUnaryResponse(messageEncoded);
    }

    // Receives a ServerStreamingResponse Unary response in the format "channelKey|callKey|base64EncodedMessage"
    internal void OnServerStreamingResponse(string channelCallMessage)
    {
        ParseParams(channelCallMessage, out _, out var call, out var payload);
        var messageEncoded = Convert.FromBase64String(payload);
        call.ReportServerStreamingResponse(messageEncoded);
    }

    // Signals call completion in the format "channelKey|callKey"
    internal void OnServerStreamingComplete(string channelCall)
    {
        ParseParams(channelCall, out _, out var call, out _);
        call.ReportCompleted();
    }

    // Signals an error occurred in a call in the format "channelKey|callKey|ErrorString"
    // The error string is base64 encoded
    internal void OnCallError(string channelCallError)
    {
        ParseParams(channelCallError, out _, out var call, out var error);
        var errorDecoded = Utils.FromBase64Utf8(error);
        call.ReportError(errorDecoded);
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

    // Reports the status of a call. Multi-line. First line is "channelKey|callKey|StatusCode"
    // Message follows in rest of line.
    internal void OnStatus(string channelCallStatus)
    {
        using var reader = new StringReader(channelCallStatus);
        var first = reader.ReadLine();
        if (first == null) throw new Exception("Invalid Status");
        ParseParams(first, out _, out var call, out var statusCode);
        var intCode = int.Parse(statusCode);

        var message = reader.ReadToEnd();
        var status = new Status((StatusCode) intCode, message);
        call.ReportStatus(status);
    }

    // Signals that the connector has been initialized
    internal void OnInstanceRegistered(int instanceKey)
    {
        InstanceKey = instanceKey;
        _initializationTask.SetResult(instanceKey);
        Debug.Log("Registered Instance: " + InstanceKey);
    }


    private void ParseParams(string channelKeyMessage, out WebGlChannel channel, out WebGlChannel.WebGLCallInvoker call, out string payload)
    {
        var parameters = channelKeyMessage.Split('|');
        var channelKey = int.Parse(parameters[0]);
        channel = _channelMap[channelKey];
        call = parameters.Length > 1 ? channel.Calls[int.Parse(parameters[1])] : null;
        payload = parameters.Length > 2 ? parameters[2] : null;

    }
}
