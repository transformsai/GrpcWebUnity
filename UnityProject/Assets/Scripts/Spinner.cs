using System.Threading;
using Ai.Transforms.Grpcwebunity;
using GrpcWebUnity;
using UnityEngine;
public class Spinner : MonoBehaviour
{
    private int i = 0;
    TestService.TestServiceClient client;

    // Update is called once per frame
    async void Awake()
    {
        var channel = await GrpcWeb.GetChannelAsync("localhost:8001");

        client = new TestService.TestServiceClient(channel);
        var call = client.ServerStream(new Request { Data = "cheem" });
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
            PrintUnary();
        }
        transform.Rotate(Vector3.up, 1f);
    }

    private async void PrintUnary()
    {
         using var call = client.UnaryAsync(new Request {Data = "earaara" + i});
         var response = await call;
         Debug.Log(response.Data);
    }
}

