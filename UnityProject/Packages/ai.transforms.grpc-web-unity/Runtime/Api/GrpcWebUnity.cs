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

        /// <summary>
        /// Gets a GRPC channel depending on the current implementation.
        /// On WebGL, it will return a webgl channel. On standalone, it will return a default GRPC channel.
        /// If no scheme is given, it will check the credentials or default to insecure http.
        /// If no port is given, will default to 80 for insecure and 443 for secure.
        /// Call is async because waiting for a channel on WebGL requires operations in javascript to occur
        /// outside of the current unity frame.
        /// </summary>
        /// <param name="targetAddress">
        /// The target to connect to.
        /// </param>
        /// <param name="credentials">Optional. if not set, will check <paramref name="targetAddress"/> scheme</param>
        /// <param name="cancellationToken">used to cancel waiting for the connection. </param>
        /// <returns></returns>
        public static async Task<ChannelBase> GetChannelAsync(string targetAddress,
            ChannelCredentials credentials = null, CancellationToken cancellationToken = default)
        {
            if (credentials == null)
            {
                credentials = targetAddress.StartsWith(Uri.UriSchemeHttps) ?
                    ChannelCredentials.SecureSsl :
                    ChannelCredentials.Insecure;
            }

            bool isValid = Uri.TryCreate(targetAddress, UriKind.Absolute, out var uri);

            if (!isValid || !ValidSchemes.Contains(uri.Scheme))
            {
                var scheme = credentials == ChannelCredentials.SecureSsl ? "https://" : "http://";
                isValid = Uri.TryCreate(scheme + targetAddress, UriKind.Absolute, out uri);
            }

            if (!isValid) throw new UriFormatException($"{nameof(targetAddress)} is not a valid URI: {targetAddress}");

            // gRPC-web requires a different uri format than C# gRPC (needs to include scheme)
            string webTargetAddress = uri.ToString().TrimEnd('/');
            // strips away scheme
            string standaloneTargetAddress = uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Scheme, UriFormat.UriEscaped).TrimEnd('/');

#if UNITY_WEBGL && !UNITY_EDITOR
            var instance = Internal.GrpcWebConnector.Instance;
            await instance.WaitForInitialization.WithCancellation(cancellationToken);
            return instance.MakeChannel(webTargetAddress);
#else
            // Delay ensures consistent behaviour across platforms 
            await Task.Delay(1, cancellationToken);
            return new Channel(standaloneTargetAddress, credentials);
#endif
        }

    }
}
