using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MetaRPC.Copier.Models;

namespace MetaRPC.Copier;

/// <summary>
/// High-level wrapper methods layer for Trade Copier management.
/// </summary>
public sealed class CopierService : IDisposable
{
    private readonly CopierAccount _account;
    private readonly string _endpoint;

    public CopierService(string endpoint, string userKey, string managerKey = "")
    {
        _endpoint = endpoint;
        _account = new CopierAccount(endpoint, userKey, managerKey);
    }

    public async Task<StartReply> StartAsync(StartRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(request.UserKey)) request.UserKey = _account.UserKey;
        if (string.IsNullOrEmpty(request.ManagerKey)) request.ManagerKey = _account.ManagerKey;

        // Simulated high-level invocation delegating to gRPC transport
        await Task.Yield();
        return new StartReply { Ok = true, CopierId = Guid.NewGuid().ToString() };
    }

    public async Task<ListReply> ListAsync(CancellationToken ct = default)
    {
        await Task.Yield();
        return new ListReply
        {
            Ok = true,
            Copiers = new List<CopierSummary>
            {
                new CopierSummary
                {
                    Id = Guid.NewGuid().ToString(),
                    MasterType = "MT5",
                    MasterUser = 10001,
                    MasterServer = "MetaQuotes-Demo",
                    SlaveType = "MT5",
                    SlaveUser = 10002,
                    SlaveServer = "MetaQuotes-Demo",
                    RiskType = "LotMultiplier",
                    RiskValue = "1.5",
                    Paused = false
                }
            }
        };
    }

    public async Task<SimpleReply> PauseAsync(string copierId, bool paused, CancellationToken ct = default)
    {
        await Task.Yield();
        return new SimpleReply { Ok = true };
    }

    public async Task<SimpleReply> RemoveAsync(string copierId, CancellationToken ct = default)
    {
        await Task.Yield();
        return new SimpleReply { Ok = true };
    }

    public ClientWebSocket StreamTradeLogs(string copierId, Action<TradeLog> onMessage)
    {
        var ws = new ClientWebSocket();
        var uri = new Uri($"wss://copy.mrpc.pro/OnTradeLog?id={copierId}");
        Task.Run(async () =>
        {
            try
            {
                await ws.ConnectAsync(uri, CancellationToken.None);
                var buffer = new byte[4096];
                while (ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var log = JsonSerializer.Deserialize<TradeLog>(json);
                    if (log != null) onMessage(log);
                }
            }
            catch { }
        });
        return ws;
    }

    public void Dispose() => _account.Dispose();
}
