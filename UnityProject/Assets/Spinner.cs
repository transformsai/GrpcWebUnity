using System.Collections;
using System.Collections.Generic;
using Ai.Transforms.Grpcwebunity;
using UnityEngine;

public class Spinner : MonoBehaviour
{

    // Update is called once per frame
    async void Awake()
    {
        transform.Rotate(Vector3.up, 1f);
        var instance = GrpcWebConnector.Instance;
        var channel = await instance.MakeChannelAsync("http://localhost:8001");

        var client = new TestService.TestServiceClient(channel);
        var ret = await client.UnaryAsync(new Request { Data = "earaara" });
        Debug.Log(ret.Data);


    }
}
