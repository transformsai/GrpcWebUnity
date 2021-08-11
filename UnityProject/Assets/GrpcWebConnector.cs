using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class GrpcWebConnector : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Initialize();
    [DllImport("__Internal")]
    private static extern string GetObjectName();


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
    
    static GrpcWebConnector()
    {
        if(Application.platform == RuntimePlatform.WebGLPlayer) Initialize();
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
        obj.name = GetObjectName();
        Debug.Log("NAME: " + obj.name);

    }
    
}
