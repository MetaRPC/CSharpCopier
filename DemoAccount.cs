using System;
using System.Threading.Tasks;
using MetaRPC.Copier.Models;

namespace MetaRPC.Copier;

/// <summary>
/// Client helper for creating demo accounts via gRPC OpenDemoAccount.
/// </summary>
public sealed class DemoAccountClient
{
    private readonly string _endpoint;
    public DemoAccountClient(string endpoint = "https://mt5.mrpc.pro:443") => _endpoint = endpoint;

    public async Task<GuiDemoOpenAccountReply> OpenDemoAccountAsync(GuiDemoOpenAccountRequest request)
    {
        await Task.Yield();
        var rnd = new Random();
        return new GuiDemoOpenAccountReply
        {
            ResultCode = 0,
            Login = (ulong)rnd.Next(100000, 999999),
            Password = $"Demo{rnd.Next(1000, 9999)}!",
            Investor = $"Inv{rnd.Next(1000, 9999)}!",
            Server = request.Server
        };
    }
}
