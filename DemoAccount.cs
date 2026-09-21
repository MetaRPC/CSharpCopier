using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MetaRPC.Copier.Models;

namespace MetaRPC.Copier;

/// <summary>
/// Client helper for creating demo accounts, connecting MT5 terminals via ConnectEx, and cleanly disconnecting.
/// </summary>
public sealed class DemoAccountClient
{
    private readonly string _endpoint;
    private static readonly HttpClient _http = new HttpClient();

    public DemoAccountClient(string endpoint = "https://mt5.mrpc.pro")
    {
        var clean = endpoint.Replace("http://", "").Replace("https://", "").Replace(":443", "").TrimEnd('/');
        _endpoint = $"https://{clean}";
    }

    public async Task<GuiDemoOpenAccountReply> OpenDemoAccountAsync(GuiDemoOpenAccountRequest request, string apiKey = "TRIAL")
    {
        var url = $"{_endpoint}/DemoAccount/Open?server={Uri.EscapeDataString(request.Server)}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("APIKey", apiKey);
        req.Headers.Add("User-Agent", "CSharpCopier/1.0.0");

        using var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new GuiDemoOpenAccountReply
        {
            ResultCode = root.TryGetProperty("resultCode", out var rc) ? rc.GetInt32() : 0,
            Login = root.TryGetProperty("login", out var lg) ? ulong.Parse(lg.GetString() ?? "0") : 0,
            Password = root.TryGetProperty("password", out var pw) ? pw.GetString() ?? "" : "",
            Investor = root.TryGetProperty("investor", out var inv) ? inv.GetString() ?? "" : "",
            Server = root.TryGetProperty("server", out var srv) ? srv.GetString() ?? request.Server : request.Server,
            DebugLog = root.TryGetProperty("debugLog", out var dbg) ? dbg.GetString() ?? "" : ""
        };
    }

    public async Task<ConnectExReply> ConnectExAsync(ulong user, string password, string server = "MetaQuotes-Demo", string apiKey = "TRIAL")
    {
        var url = $"{_endpoint}/ConnectEx?user={user}&password={Uri.EscapeDataString(password)}&mtClusterName={Uri.EscapeDataString(server)}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("APIKey", apiKey);
        req.Headers.Add("User-Agent", "CSharpCopier/1.0.0");

        using var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var guid = doc.RootElement.GetProperty("data").GetProperty("terminalInstanceGuid").GetString() ?? "";

        return new ConnectExReply
        {
            TerminalInstanceGuid = guid,
            TerminalType = "MT5"
        };
    }

    public async Task<DisconnectReply> DisconnectAsync(string terminalId, string apiKey = "TRIAL")
    {
        var url = $"{_endpoint}/Disconnect";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("APIKey", apiKey);
        req.Headers.Add("id", terminalId);
        req.Headers.Add("User-Agent", "CSharpCopier/1.0.0");

        using var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");

        return new DisconnectReply
        {
            UniqueIdentifier = data.GetProperty("uniqueIdentifier").GetString() ?? "",
            FullLifeTimeSeconds = data.GetProperty("fullLifeTimeSeconds").GetInt32()
        };
    }
}
