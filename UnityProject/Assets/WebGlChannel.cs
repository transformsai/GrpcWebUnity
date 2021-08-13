using System.Collections.Generic;
using Grpc.Core;

internal class WebGlChannel : ChannelBase
{
    public readonly GrpcWebConnector Connector;
    public readonly int ChannelKey;

    public WebGlChannel(GrpcWebConnector connector, string target, int channelKey) : base(target)
    {
        Connector = connector;
        ChannelKey = channelKey;
    }

    internal readonly Dictionary<int, WebGLCall> Calls = new Dictionary<int, WebGLCall>();

    public override CallInvoker CreateCallInvoker() => new WebGLCall(this);
}
