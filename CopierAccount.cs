using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Grpc.Core;
using Grpc.Net.Client;

namespace MetaRPC.Copier;

/// <summary>
/// Low-level gRPC protocol layer for MetaRPC Trade Copier.
/// Configures HTTP/2 channels, TLS, authentication headers, and connection keep-alive.
/// </summary>
public sealed class CopierAccount : IDisposable
{
    public GrpcChannel Channel { get; }
    public string UserKey { get; }
    public string ManagerKey { get; }

    public CopierAccount(string endpoint, string userKey, string managerKey = "")
    {
        UserKey = userKey;
        ManagerKey = string.IsNullOrEmpty(managerKey) ? userKey : managerKey;

        var handler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            KeepAlivePingDelay = TimeSpan.FromSeconds(30),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
            EnableMultipleHttp2Connections = true
        };

        var options = new GrpcChannelOptions
        {
            HttpHandler = handler,
            MaxReceiveMessageSize = 16 * 1024 * 1024,
            MaxSendMessageSize = 16 * 1024 * 1024
        };

        Channel = GrpcChannel.ForAddress(endpoint.StartsWith("http") ? endpoint : $"https://{endpoint}", options);
    }

    public Metadata CreateAuthMetadata()
    {
        var headers = new Metadata
        {
            { "authorization", $"Bearer {UserKey}" },
            { "x-metarpc-manager", ManagerKey },
            { "x-metarpc-client-sdk", "CSharpCopier/1.0.0" }
        };
        return headers;
    }

    public void Dispose() => Channel.Dispose();
}
