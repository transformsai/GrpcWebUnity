using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

public class GrpcWebConnector : MonoBehaviour
{

    /// <summary>
    /// 
    /// Gets the gameobject name that JS is expecting to use to communicate with this script
    /// </summary>
    [DllImport("__Internal")]
    private static extern string GetObjectName();

    /// <summary>
    /// Registers this unity instance to allow the code to connect
    /// This will async return in OnInstanceRegistered
    /// </summary>
    [DllImport("__Internal")]
    private static extern void RegisterInstance();

    /// <summary>
    /// Registers a channel to allow repeated calls to the same target
    /// </summary>
    /// <returns>A channel key. This channel key need to be passed for calls </returns>
    [DllImport("__Internal")]
    private static extern int RegisterChannel(int instanceKey, string target);


    /// <summary>
    /// Starts a unary request on the channel.
    /// </summary>
    /// <returns>A call key. This key be used to map responses and allow cancellations.</returns>
    [DllImport("__Internal")]
    private static extern int ChannelUnaryRequest(int instanceKey, int channelKey, string serviceName, string methodName, string base64Message);


    /// <summary>
    /// Starts a server streaming request on the channel.
    /// </summary>
    /// <returns>A call key. This key be used to map responses and allow cancellations.</returns>
    [DllImport("__Internal")]
    private static extern int ChannelServerStreamingRequest(int instanceKey, int channelKey, string serviceName, string methodName, string base64Message);


    /// <summary>
    /// Starts a server streaming request on the channel.
    /// </summary>
    /// <returns>A call key. This key be used to map responses and allow cancellations.</returns>
    [DllImport("__Internal")]
    private static extern int CancelCall(int instanceKey, int channelKey, int callKey);



    // Receives a pipe-delimited Unary response in the format "channelKey|callKey|base64EncodedMessage"
    private void OnChannelUnaryResponse(string channelCallMessage)
    {

    }

    // Receives a ServerStreamingResponse Unary response in the format "channelKey|callKey|base64EncodedMessage"
    private void OnChannelServerStreamingResponse(string channelCallMessage)
    {

    }

    // Signals call completion in the format "channelKey|callKey"
    private void OnChannelServerStreamingComplete(string channelCall)
    {

    }

    // Signals an error occurred in a call in the format "channelKey|callKey|ErrorString"
    // The error string will have all pipes `|` replaced with Underscores `_`
    private void OnChannelCallError(string channelCallError)
    {

    }


    // Signals that the connector has been initialized
    private void OnInstanceRegistered(int instanceKey)
    {
        InstanceKey = instanceKey;
        _initializationTask.SetResult(instanceKey);
    }


    // Signals that the connector has been initialized
    private void OnInstanceRegistrationFailure(string errorString)
    {
        Debug.LogError(errorString);
        _initializationTask.SetException(new Exception($"Initialization failed: {errorString}"));
    }


    private static GrpcWebConnector _instance;
    public static GrpcWebConnector Instance
    {
        get
        {
            if (_instance) return _instance;
            var go = new GameObject();
            go.AddComponent<GrpcWebConnector>();
            if (_instance) return _instance;
            Debug.LogError("Failed to register connector.");
            return null;
        }
    }

    private int InstanceKey { get; set; } = -1;
    private readonly TaskCompletionSource<int> _initializationTask = new TaskCompletionSource<int>();
    private bool IsInitialized => InstanceKey < 0;
    public Task WaitForInitialization => _initializationTask.Task;

    private Dictionary<int, WebGlChannel> _channelMap  = new Dictionary<int, WebGlChannel>();

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
        obj.name = GetObjectName();
        DontDestroyOnLoad(obj);
        Debug.Log("Created GrpcWebConnector: " + obj.name);

        RegisterInstance();
        Debug.Log("Registered Instance: " + InstanceKey);

    }

    public ChannelBase MakeChannel(string target)
    {
        if (!IsInitialized)
            throw new Exception($"Connector not initialized yet. Wait for {nameof(WaitForInitialization)}");

        var channelKey = RegisterChannel(InstanceKey, target);
        Debug.Log("Registered Channel: " + channelKey);
        var channel = new WebGlChannel(this, target, channelKey);
        _channelMap[channelKey] = channel;
        return channel;
    }


    private class WebGlChannel : ChannelBase
    {
        public readonly GrpcWebConnector Connector;
        public readonly int ChannelKey;

        public WebGlChannel(GrpcWebConnector connector, string target, int channelKey) : base(target)
        {
            Connector = connector;
            ChannelKey = channelKey;
        }

        public override CallInvoker CreateCallInvoker() => new WebGLCallInvoker(this);
    }

    class WebGLCallInvoker : CallInvoker
    {
        public readonly WebGlChannel Channel;
        public GrpcWebConnector Connector => Channel.Connector;

        public WebGLCallInvoker(WebGlChannel webGlChannel)
        {
            Channel = webGlChannel;
        }

        private TaskCompletionSource<object> UnaryResponse;

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            var requestBytes = method.RequestMarshaller.Serializer(request);
            var base64Request = Convert.ToBase64String(requestBytes);
            var callKey = ChannelUnaryRequest(Connector.InstanceKey, Channel.ChannelKey, method.ServiceName, method.Name, base64Request);
            Debug.Log("Registered Call: " + callKey);
            var x = new { };

            var call = new AsyncUnaryCall<TResponse>(
                UnaryResponse.Task.ContinueWith(it => (TResponse)it.Result),
                Task.FromResult(new Metadata()),
                () => new Status(StatusCode.OK, "We did it!"),
                () => new Metadata(),
                () => CancelCall(Connector.InstanceKey, Channel.ChannelKey, callKey)
            );

            return call;
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options,
            TRequest request)
        {
            var requestBytes = method.RequestMarshaller.Serializer(request);
            var base64Request = Convert.ToBase64String(requestBytes);
            var callKey = ChannelUnaryRequest(Connector.InstanceKey, Channel.ChannelKey, method.ServiceName, method.Name, base64Request);
            Debug.Log("Registered Call: " + callKey);
            var x = new { };
            var streamReader = new WebGLStreamReader<TResponse>();
            var call = new AsyncServerStreamingCall<TResponse>(
                streamReader,
                Task.FromResult(new Metadata()),
                () => new Status(StatusCode.OK, "We did it again!"),
                () => new Metadata(),
                () => {
                    CancelCall(Connector.InstanceKey, Channel.ChannelKey, callKey);
                    streamReader.SignalEnd();
                }
            );

            return call;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request) =>
            throw new NotImplementedException("WebGL does not allow for blocking calls. Callback happens on main thread.");

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options) =>
            throw new NotImplementedException("WebGL does not allow for client streaming.");

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options) =>
            throw new NotImplementedException("WebGL does not allow for client streaming.");

        class WebGLStreamReader<T> : IAsyncStreamReader<T>
        {
            public T Current { get; set; }

            private Queue<T> _queue = new Queue<T>();

            private TaskCompletionSource<object> _updated = new TaskCompletionSource<object>();

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                cancellationToken.Register(() => _updated.SetCanceled());
                while (true) {
                    if (_queue.Count > 0)
                    {
                        T item = _queue.Dequeue();
                        Current = item;
                        return true;
                    }
                    else
                    {
                        try
                        {
                            // TODO: RESPECT THE AUTHORITY OF THE CANCELLATION TOKEN
                            await _updated.Task;                            
                        }
                        catch (TaskCanceledException)
                        {
                            return false;
                        }
                        finally {
                            _updated = new TaskCompletionSource<object>();
                        }
                    }
                }                               
            }

            public void SignalEnd() {
                _updated.SetCanceled();
            }

            public void SignalError(Exception e) {
                _updated.SetException(e);
            }

            public void AddItem(T item) {
                _queue.Enqueue(item);
                _updated.SetResult(null);
            }
        }
    }    
}
