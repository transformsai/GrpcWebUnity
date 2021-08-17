using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

namespace GrpcWebUnity
{
    public static class GrpcWeb
    {
        private static string[] ValidSchemes { get; } = { Uri.UriSchemeHttps, Uri.UriSchemeHttp };

        public static async Task<ChannelBase> GetChannelAsync(string targetAddress,
            ChannelCredentials credentials = null, CancellationToken cancellationToken = default)
        {
            credentials ??= ChannelCredentials.Insecure;

            // Added in if rather than compile defines to avoid accidental refactoring from breaking it.
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                bool valid = Uri.TryCreate(targetAddress, UriKind.Absolute, out var uri);
                if (!valid || !ValidSchemes.Contains(uri.Scheme))
                    valid = Uri.TryCreate("http://" + targetAddress, UriKind.Absolute, out uri);

                if (!valid) throw new UriFormatException($"{nameof(targetAddress)} is not a valid URI: {targetAddress}");

                targetAddress = uri.ToString();

            }

#if UNITY_WEBGL && !UNITY_EDITOR
            var instance = Internal.GrpcWebConnector.Instance;
            await instance.WaitForInitialization.WithCancellation(cancellationToken);
            return instance.MakeChannel(targetAddress);
#else
            // Ensures consistent behaviour across platforms 
            await Task.Delay(1, cancellationToken);
            return new Channel(targetAddress, credentials);
#endif
        }

    }
}
