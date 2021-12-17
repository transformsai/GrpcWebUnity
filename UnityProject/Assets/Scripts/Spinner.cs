using System;
using System.Threading;
using Ai.Transforms.Grpcwebunity;
using TransformsAI.Unity.Grpc.Web;
using TransformsAI.Unity.Protobuf;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    public int Counter;
    private TestService.TestServiceClient _client;
    public Proto<Request> Request;
    private CancellationTokenSource _cts = new CancellationTokenSource();

    async void Awake()
    {
        var channel = UnityGrpcWeb.MakeChannel("http://localhost:8001");
        _client = new TestService.TestServiceClient(channel);
        var reqCopy = Request.Value.Clone();
        reqCopy.Data += " Stream Input";
        var call = _client.ServerStream(reqCopy);
        var stream = call.ResponseStream;

        while (await stream.MoveNext(_cts.Token))
        {
            var item = stream.Current;
            Debug.Log("Stream: " + item.Data);
        }
    }

    private void Update()
    {
        if (Counter++ % 200 == 0) PrintUnary(Counter);

        transform.Rotate(Vector3.up, 1f);
    }

    private async void PrintUnary(int counter)
    {
        var reqCopy = Request.Value.Clone();
        reqCopy.Data += $" {counter}";
        using var call = _client.UnaryAsync(reqCopy, cancellationToken: _cts.Token);
        var response = await call.ResponseAsync.ConfigureAwait(false);
        Debug.Log($"{counter}:\n{response.Data}");
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts = null;
    }
}
