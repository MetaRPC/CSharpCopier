using System;
using System.Text.Json.Serialization;

namespace MetaRPC.Copier.Models;

public class Account
{
    [JsonPropertyName("type")] public string Type { get; set; } = "MT5";
    [JsonPropertyName("user")] public ulong User { get; set; }
    [JsonPropertyName("password")] public string Password { get; set; } = "";
    [JsonPropertyName("server")] public string Server { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public class StartRequest
{
    [JsonPropertyName("user_key")] public string UserKey { get; set; } = "";
    [JsonPropertyName("manager_key")] public string ManagerKey { get; set; } = "";
    [JsonPropertyName("master")] public Account Master { get; set; } = new();
    [JsonPropertyName("slave")] public Account Slave { get; set; } = new();
    [JsonPropertyName("risk_type")] public string RiskType { get; set; } = "LotMultiplier";
    [JsonPropertyName("risk_value")] public string RiskValue { get; set; } = "1.0";
    [JsonPropertyName("fixed_master_balance")] public string FixedMasterBalance { get; set; } = "";
    [JsonPropertyName("copy_sl")] public bool CopySl { get; set; } = true;
    [JsonPropertyName("copy_tp")] public bool CopyTp { get; set; } = true;
    [JsonPropertyName("copy_pending_orders")] public bool CopyPendingOrders { get; set; } = false;
    [JsonPropertyName("reverse_copy")] public bool ReverseCopy { get; set; } = false;
}

public class StartReply
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }
    [JsonPropertyName("copier_id")] public string CopierId { get; set; } = "";
    [JsonPropertyName("error")] public string Error { get; set; } = "";
}

public class CopierSummary
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("master_type")] public string MasterType { get; set; } = "";
    [JsonPropertyName("master_user")] public ulong MasterUser { get; set; }
    [JsonPropertyName("master_server")] public string MasterServer { get; set; } = "";
    [JsonPropertyName("slave_type")] public string SlaveType { get; set; } = "";
    [JsonPropertyName("slave_user")] public ulong SlaveUser { get; set; }
    [JsonPropertyName("slave_server")] public string SlaveServer { get; set; } = "";
    [JsonPropertyName("risk_type")] public string RiskType { get; set; } = "";
    [JsonPropertyName("risk_value")] public string RiskValue { get; set; } = "";
    [JsonPropertyName("paused")] public bool Paused { get; set; }
    [JsonPropertyName("pause_reason")] public string PauseReason { get; set; } = "";
}

public class ListReply
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }
    [JsonPropertyName("copiers")] public List<CopierSummary> Copiers { get; set; } = new();
    [JsonPropertyName("error")] public string Error { get; set; } = "";
}

public class SimpleReply
{
    [JsonPropertyName("ok")] public bool Ok { get; set; }
    [JsonPropertyName("error")] public string Error { get; set; } = "";
}

public class TradeLog
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("copierId")] public string CopierId { get; set; } = "";
    [JsonPropertyName("symbol")] public string Symbol { get; set; } = "";
    [JsonPropertyName("updateType")] public string UpdateType { get; set; } = "";
    [JsonPropertyName("masterTicket")] public ulong MasterTicket { get; set; }
    [JsonPropertyName("slaveTicket")] public ulong SlaveTicket { get; set; }
    [JsonPropertyName("profit")] public double Profit { get; set; }
    [JsonPropertyName("success")] public bool Success { get; set; }
}

public class GuiDemoOpenAccountRequest
{
    public string Company { get; set; } = "MetaQuotes-Demo";
    public string FirstName { get; set; } = "Demo";
    public string LastName { get; set; } = "User";
    public string Email { get; set; } = "demo@metarpc.pro";
    public string Phone { get; set; } = "+123456789";
    public string Server { get; set; } = "MetaQuotes-Demo";
    public string AccountType { get; set; } = "forex";
    public int TimeoutSeconds { get; set; } = 30;
}

public class GuiDemoOpenAccountReply
{
    public int ResultCode { get; set; }
    public ulong Login { get; set; }
    public string Password { get; set; } = "";
    public string Investor { get; set; } = "";
    public string Server { get; set; } = "";
    public string DebugLog { get; set; } = "";
}
