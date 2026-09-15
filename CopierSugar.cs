using System;
using System.Threading.Tasks;
using MetaRPC.Copier.Models;

namespace MetaRPC.Copier;

/// <summary>
/// High-level convenience API and fluent builder for Trade Copier.
/// </summary>
public sealed class CopierSugar
{
    private readonly StartRequest _req = new();
    private string _endpoint = "https://copy.mrpc.pro:443";

    public static CopierSugar Create() => new();

    public CopierSugar WithEndpoint(string endpoint) { _endpoint = endpoint; return this; }
    public CopierSugar WithCredentials(string userKey, string managerKey = "") { _req.UserKey = userKey; _req.ManagerKey = string.IsNullOrEmpty(managerKey) ? userKey : managerKey; return this; }

    public CopierSugar FromMaster(Action<AccountBuilder> configure)
    {
        var b = new AccountBuilder();
        configure(b);
        _req.Master = b.Build();
        return this;
    }

    public CopierSugar ToSlave(Action<AccountBuilder> configure)
    {
        var b = new AccountBuilder();
        configure(b);
        _req.Slave = b.Build();
        return this;
    }

    public CopierSugar WithLotMultiplier(double multiplier) { _req.RiskType = "LotMultiplier"; _req.RiskValue = multiplier.ToString("0.00"); return this; }
    public CopierSugar WithFixedLot(double lot) { _req.RiskType = "FixedLot"; _req.RiskValue = lot.ToString("0.00"); return this; }
    public CopierSugar CopyStopLoss(bool copy = true) { _req.CopySl = copy; return this; }
    public CopierSugar CopyTakeProfit(bool copy = true) { _req.CopyTp = copy; return this; }
    public CopierSugar CopyPendingOrders(bool copy = true) { _req.CopyPendingOrders = copy; return this; }
    public CopierSugar InvertDirection(bool reverse = true) { _req.ReverseCopy = reverse; return this; }

    public async Task<StartReply> StartAsync()
    {
        using var svc = new CopierService(_endpoint, _req.UserKey, _req.ManagerKey);
        return await svc.StartAsync(_req);
    }
}

public sealed class AccountBuilder
{
    private readonly Account _acc = new();
    public AccountBuilder Type(string type) { _acc.Type = type; return this; }
    public AccountBuilder User(ulong user) { _acc.User = user; return this; }
    public AccountBuilder Password(string password) { _acc.Password = password; return this; }
    public AccountBuilder Server(string server) { _acc.Server = server; return this; }
    public AccountBuilder Name(string name) { _acc.Name = name; return this; }
    public Account Build() => _acc;
}
