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
    private readonly tradecopy.Grpc.CopierService.CopierServiceClient _grpcClient;

    public CopierService(string endpoint, string userKey, string managerKey = "")
    {
        _endpoint = endpoint;
        _account = new CopierAccount(endpoint, userKey, string.IsNullOrEmpty(managerKey) ? userKey : managerKey);
        _grpcClient = new tradecopy.Grpc.CopierService.CopierServiceClient(_account.Channel);
    }

    internal CopierService(CopierAccount account, Grpc.Core.CallInvoker invoker)
    {
        _account = account;
        _endpoint = "https://copy.mrpc.pro";
        _grpcClient = new tradecopy.Grpc.CopierService.CopierServiceClient(invoker);
    }

    public async Task<StartReply> StartAsync(StartRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(request.UserKey)) request.UserKey = _account.UserKey;
        if (string.IsNullOrEmpty(request.ManagerKey)) request.ManagerKey = _account.ManagerKey;

        var grpcReq = new tradecopy.Grpc.StartRequest
        {
            UserKey = request.UserKey,
            ManagerKey = request.ManagerKey,
            Master = new tradecopy.Grpc.Account
            {
                Type = request.Master.Type,
                User = request.Master.User,
                Password = request.Master.Password,
                Server = request.Master.Server,
                Name = request.Master.Name,
                Id = request.Master.Id
            },
            Slave = new tradecopy.Grpc.Account
            {
                Type = request.Slave.Type,
                User = request.Slave.User,
                Password = request.Slave.Password,
                Server = request.Slave.Server,
                Name = request.Slave.Name,
                Id = request.Slave.Id
            },
            RiskType = request.RiskType,
            RiskValue = request.RiskValue,
            FixedMasterBalance = request.FixedMasterBalance,
            CopySl = request.CopySl,
            CopyTp = request.CopyTp,
            CopyPendingOrders = request.CopyPendingOrders,
            ReverseCopy = request.ReverseCopy
        };

        try
        {
            var headers = _account.CreateAuthMetadata();
            var reply = await _grpcClient.StartAsync(grpcReq, headers, deadline: DateTime.UtcNow.AddSeconds(180), cancellationToken: ct);
            return new StartReply { Ok = reply.Ok, CopierId = reply.CopierId, Error = reply.Error };
        }
        catch (Exception ex)
        {
            return new StartReply { Ok = false, Error = ex.Message };
        }
    }

    public async Task<ListReply> ListAsync(CancellationToken ct = default)
    {
        try
        {
            var headers = _account.CreateAuthMetadata();
            var reply = await _grpcClient.ListAsync(new tradecopy.Grpc.ListRequest { UserKey = _account.UserKey }, headers, deadline: DateTime.UtcNow.AddSeconds(180), cancellationToken: ct);
            var list = new List<CopierSummary>();
            foreach (var c in reply.Copiers)
            {
                list.Add(new CopierSummary
                {
                    Id = c.Id,
                    MasterType = c.MasterType,
                    MasterUser = c.MasterUser,
                    MasterServer = c.MasterServer,
                    SlaveType = c.SlaveType,
                    SlaveUser = c.SlaveUser,
                    SlaveServer = c.SlaveServer,
                    RiskType = c.RiskType,
                    RiskValue = c.RiskValue,
                    Paused = c.Paused,
                    PauseReason = c.PauseReason
                });
            }
            return new ListReply { Ok = reply.Ok, Copiers = list, Error = reply.Error };
        }
        catch (Exception ex)
        {
            return new ListReply { Ok = false, Copiers = new List<CopierSummary>(), Error = ex.Message };
        }
    }

    public async Task<SimpleReply> PauseAsync(string copierId, bool paused, CancellationToken ct = default)
    {
        try
        {
            var headers = _account.CreateAuthMetadata();
            var reply = await _grpcClient.PauseAsync(new tradecopy.Grpc.PauseRequest { UserKey = _account.UserKey, CopierId = copierId, Paused = paused }, headers, deadline: DateTime.UtcNow.AddSeconds(180), cancellationToken: ct);
            return new SimpleReply { Ok = reply.Ok, Error = reply.Error };
        }
        catch (Exception ex)
        {
            return new SimpleReply { Ok = false, Error = ex.Message };
        }
    }

    public async Task<SimpleReply> RemoveAsync(string copierId, CancellationToken ct = default)
    {
        try
        {
            var headers = _account.CreateAuthMetadata();
            var reply = await _grpcClient.RemoveAsync(new tradecopy.Grpc.RemoveRequest { UserKey = _account.UserKey, CopierId = copierId }, headers, deadline: DateTime.UtcNow.AddSeconds(180), cancellationToken: ct);
            return new SimpleReply { Ok = reply.Ok, Error = reply.Error };
        }
        catch (Exception ex)
        {
            return new SimpleReply { Ok = false, Error = ex.Message };
        }
    }

    /// <summary>Reads complete real log messages. Await this task to observe connection and parsing failures.</summary>
    public async Task StreamTradeLogsAsync(string copierId, Action<JsonElement> onMessage, CancellationToken ct = default)
    {
        using var ws = new ClientWebSocket();
        await ReadTradeLogsAsync(ws, copierId, onMessage, ct);
    }

    private async Task ReadTradeLogsAsync(ClientWebSocket ws, string copierId, Action<JsonElement> onMessage, CancellationToken ct)
    {
        var endpoint = new Uri(_endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? _endpoint : $"https://{_endpoint}");
        var uri = new UriBuilder(endpoint) { Scheme = endpoint.Scheme == "http" ? "ws" : "wss", Path = "/OnTradeLog", Query = "id=" + Uri.EscapeDataString(copierId) }.Uri;
        ws.Options.SetRequestHeader("ApiKey", _account.UserKey);
        using var handshake = CancellationTokenSource.CreateLinkedTokenSource(ct);
        handshake.CancelAfter(TimeSpan.FromSeconds(10));
        await ws.ConnectAsync(uri, handshake.Token);
        var buffer = new byte[8192];
        while (ws.State == WebSocketState.Open)
        {
            using var message = new System.IO.MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == WebSocketMessageType.Close) return;
                if (result.MessageType != WebSocketMessageType.Text) throw new InvalidOperationException("Expected a JSON text message.");
                if (message.Length + result.Count > 4 * 1024 * 1024) throw new InvalidOperationException("Trade-log message exceeds 4 MiB.");
                message.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);
            using var document = JsonDocument.Parse(message.ToArray());
            onMessage(document.RootElement.Clone());
        }
    }

    [Obsolete("Use and await StreamTradeLogsAsync to observe errors and preserve the full log payload.")]
    public ClientWebSocket StreamTradeLogs(string copierId, Action<TradeLog> onMessage, Action<Exception>? onError = null)
    {
        var ws = new ClientWebSocket();
        _ = Task.Run(async () => {
            try { await ReadTradeLogsAsync(ws, copierId, json => onMessage(TradeLog.FromJson(json)), CancellationToken.None); }
            catch (Exception ex) { if (onError != null) onError(ex); else System.Diagnostics.Trace.TraceError("Trade-log stream failed: {0}", ex.GetType().Name); }
            finally { ws.Dispose(); }
        });
        return ws;
    }

    public void Dispose() => _account.Dispose();
}
