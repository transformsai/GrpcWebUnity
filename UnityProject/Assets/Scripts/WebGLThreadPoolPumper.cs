using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

public static class WebGlThreadPoolPumper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void StartPumping()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer || Application.isEditor) return;

        var dispatchMethod = Type
            .GetType("System.Threading.ThreadPoolWorkQueue")?
            .GetMethod("Dispatch", BindingFlags.NonPublic | BindingFlags.Static)?
            .CreateDelegate(typeof(Func<bool>)) as Func<bool>;

        Pump(dispatchMethod);
    }

    public static void Pump(object dispatchMethod)
    {
        var method = (Func<bool>)dispatchMethod;
        var didFinishWork = method();
        SynchronizationContext.Current.Post(Pump, dispatchMethod);
    }
}

