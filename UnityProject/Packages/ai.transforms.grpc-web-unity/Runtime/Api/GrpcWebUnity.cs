using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace GrpcWebUnity
{
    public static class GrpcWeb
    {
        public static async Task<ChannelBase> GetChannelAsync(string targetAddress,
            ChannelCredentials credentials = null, CancellationToken cancellationToken = default)
        {
            credentials ??= ChannelCredentials.Insecure;

#if UNITY_WEBGL && !UNITY_EDITOR
            var instance = GrpcWebUnity.Internal.GrpcWebConnector.Instance;
            await instance.WaitForInitialization.WithCancellation(cancellationToken);
            return instance.MakeChannel(targetAddress);
#else
            return new Channel(targetAddress, credentials);
#endif
        }

    }
}
