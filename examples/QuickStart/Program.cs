using System;
using System.Threading.Tasks;
using MetaRPC.Copier;
using MetaRPC.Copier.Models;

Console.WriteLine("=== MetaRPC CSharpCopier Quick Start Demo ===");

// 1. Provision Demo Accounts
var demoClient = new DemoAccountClient("https://mt5.mrpc.pro:443");
var master = await demoClient.OpenDemoAccountAsync(new GuiDemoOpenAccountRequest { Server = "MetaQuotes-Demo" });
var slave = await demoClient.OpenDemoAccountAsync(new GuiDemoOpenAccountRequest { Server = "MetaQuotes-Demo" });
Console.WriteLine($"[1] Created Master Account: #{master.Login} on {master.Server}");
Console.WriteLine($"    Created Slave Account:  #{slave.Login} on {slave.Server}");

// 2. Initialize Copier Service
var copier = new CopierService("https://copy.mrpc.pro:443", "YOUR_USER_KEY");

// 3. Start Copier
var startRes = await copier.StartAsync(new StartRequest
{
    Master = new Account { Type = "MT5", User = master.Login, Password = master.Password, Server = master.Server },
    Slave = new Account { Type = "MT5", User = slave.Login, Password = slave.Password, Server = slave.Server },
    RiskType = "LotMultiplier",
    RiskValue = "1.5",
    CopySl = true,
    CopyTp = true
});
Console.WriteLine($"[2] Started Copier! ID: {startRes.CopierId}");

// 4. List Active Copiers
var list = await copier.ListAsync();
Console.WriteLine($"[3] Active Copiers count: {list.Copiers.Count}");

// 5. Pause and Cleanup
await copier.PauseAsync(startRes.CopierId, true);
await copier.RemoveAsync(startRes.CopierId);
Console.WriteLine("[4] Copier cleanly paused and removed.");
