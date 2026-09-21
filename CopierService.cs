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
                Name = request.Master.Name
            },
            Slave = new tradecopy.Grpc.Account
            {
                Type = request.Slave.Type,
                User = request.Slave.User,
                Password = request.Slave.Password,
                Server = request.Slave.Server,
                Name = request.Slave.Name
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
            var reply = await _grpcClient.StartAsync(grpcReq, headers, cancellationToken: ct);
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
            var reply = await _grpcClient.ListAsync(new tradecopy.Grpc.ListRequest { UserKey = _account.UserKey }, headers, cancellationToken: ct);
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
            var reply = await _grpcClient.PauseAsync(new tradecopy.Grpc.PauseRequest { UserKey = _account.UserKey, CopierId = copierId, Paused = paused }, headers, cancellationToken: ct);
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
            var reply = await _grpcClient.RemoveAsync(new tradecopy.Grpc.RemoveRequest { UserKey = _account.UserKey, CopierId = copierId }, headers, cancellationToken: ct);
            return new SimpleReply { Ok = reply.Ok, Error = reply.Error };
        }
        catch (Exception ex)
        {
            return new SimpleReply { Ok = false, Error = ex.Message };
        }
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
