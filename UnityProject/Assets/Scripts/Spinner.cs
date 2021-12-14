using System.Threading;
using Ai.Transforms.Grpcwebunity;
using TransformsAI.UnityGrpcWeb;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    private int i = 0;
    TestService.TestServiceClient client;

    async void Awake()
    {
        var channel = UnityGrpcWeb.MakeChannel("http://localhost:8001");
        client = new TestService.TestServiceClient(channel);
        var call = client.ServerStream(new Request { Data = "cheem" });
        var stream = call.ResponseStream;
        while (await stream.MoveNext(CancellationToken.None))
        {
            var item = stream.Current;
            Debug.Log("Stream: " + item.Data);
        }
    }

    private void Update()
    {
        if (i++ % 200 == 0) PrintUnary();

        transform.Rotate(Vector3.up, 1f);
    }

    private async void PrintUnary()
    {
        using var call = client.UnaryAsync(new Request { Data = "Unary: " + i });
        var response = await call.ResponseAsync.ConfigureAwait(continueOnCapturedContext: true);
        Debug.Log(response.Data);

    }
}
