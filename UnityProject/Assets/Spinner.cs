using System.Threading;
using System.Threading.Tasks;
using Ai.Transforms.Grpcwebunity;
using Grpc.Core;
using UnityEngine;

public class Spinner : MonoBehaviour
{

    // Update is called once per frame
    async void Awake()
    {
        var instance = GrpcWebConnector.Instance;
        var channel = await instance.MakeChannelAsync("http://localhost:8001");

        var client = new TestService.TestServiceClient(channel);
        var call = client.ServerStream(new Request { Data = "earaara" });
        var stream = call.ResponseStream;
        while (await stream.MoveNext())
        {
            var item = stream.Current;
            Debug.Log("Success! " + item.Data);
        }
    }
    private void Update()
    {

        transform.Rotate(Vector3.up, 1f);
    }
}

