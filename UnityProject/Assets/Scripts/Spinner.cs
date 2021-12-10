using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Ai.Transforms.Grpcwebunity;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using JsInterop;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    private int i = 0;
    TestService.TestServiceClient client;

    
    async void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Runtime.Initialize();
        var innerHandler = new UnityWebGlHttpHandler();
        innerHandler.BeforeSend += message =>
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/grpc-web-text"));
        var handler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, innerHandler);
#else
        var innerHandler = new HttpClientHandler();
        var handler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, innerHandler);
#endif

        var options = new GrpcChannelOptions
        {
            HttpHandler = handler,
            //LoggerFactory = new UnityLoggerFactory(),
        };
        var channel = GrpcChannel.ForAddress("http://localhost:8001", options);

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

        if (i++ % 200 == 0)
        {
            PrintUnary();
        }
        transform.Rotate(Vector3.up, 1f);
    }

    private async void PrintUnary()
    {
        using var call = client.UnaryAsync(new Request { Data = "Unary: " + i });
        var response = await call.ResponseAsync.ConfigureAwait(continueOnCapturedContext: true);
        Debug.Log(response.Data);

    }
}
