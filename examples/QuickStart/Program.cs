using System;
using System.Text.Json;
using System.Threading.Tasks;
using MetaRPC.Copier;
using MetaRPC.Copier.Models;

Console.WriteLine("=== MetaRPC CSharpCopier Trade Replication Quick Start ===");
const string apiKey = "TRIAL";
var demoClient = new DemoAccountClient("https://mt5.mrpc.pro");
string? masterGuid = null;
string? slaveGuid = null;
string? copierId = null;

var copier = new CopierService("https://copy.mrpc.pro:443", apiKey);

try
{
    // 1. Provision Live Demo Accounts
    Console.WriteLine("\n[1] Provisioning live demo accounts on MetaQuotes-Demo...");
    var master = await demoClient.OpenDemoAccountAsync(new GuiDemoOpenAccountRequest { Server = "MetaQuotes-Demo" }, apiKey);
    Console.WriteLine($"    Master Account Provisioned: #{master.Login} on {master.Server}");
    await Task.Delay(1000);

    var slave = await demoClient.OpenDemoAccountAsync(new GuiDemoOpenAccountRequest { Server = "MetaQuotes-Demo" }, apiKey);
    Console.WriteLine($"    Slave Account Provisioned:  #{slave.Login} on {slave.Server}");
    await Task.Delay(1000);

    // 2. Connect Terminals via ConnectEx with APIKey: TRIAL
    Console.WriteLine($"\n[2] Connecting terminals via ConnectEx (APIKey: {apiKey})...");
    var connM = await demoClient.ConnectExAsync(master.Login, master.Password, master.Server, apiKey);
    masterGuid = connM.TerminalInstanceGuid;
    Console.WriteLine($"    Master Terminal Connected! GUID: {masterGuid}");

    var connS = await demoClient.ConnectExAsync(slave.Login, slave.Password, slave.Server, apiKey);
    slaveGuid = connS.TerminalInstanceGuid;
    Console.WriteLine($"    Slave Terminal Connected!  GUID: {slaveGuid}");

    var masterSessionId = DemoAccountClient.ToHyphenGuid(masterGuid);
    var slaveSessionId = DemoAccountClient.ToHyphenGuid(slaveGuid);

    // 3. Start Trade Copier via gRPC on copy.mrpc.pro:443
    Console.WriteLine($"\n[3] Starting Trade Copier via gRPC on copy.mrpc.pro:443...");
    var startReply = await copier.StartAsync(new StartRequest
    {
        UserKey = apiKey,
        RiskType = "LotMultiplier",
        RiskValue = "1.0",
        Master = new Account
        {
            Type = "MT5",
            User = master.Login,
            Password = master.Password,
            Server = master.Server,
            Id = masterSessionId
        },
        Slave = new Account
        {
            Type = "MT5",
            User = slave.Login,
            Password = slave.Password,
            Server = slave.Server,
            Id = slaveSessionId
        }
    });

    Console.WriteLine($"    gRPC Start Reply: ok={startReply.Ok}, copierId={startReply.CopierId}, error={startReply.Error}");
    if (!startReply.Ok)
    {
        throw new Exception($"Failed to start copier: {startReply.Error}");
    }
    copierId = startReply.CopierId;

    await Task.Delay(3000);

    // 4. Place Market Order on Master
    Console.WriteLine("\n[4] Opening Market Order on Master (0.01 EURUSD BUY)...");
    var sendDoc = await demoClient.OrderSendAsync(masterGuid, "EURUSD", "TMT5_ORDER_TYPE_BUY", 0.01, apiKey);
    ulong masterTicket = 0;
    if (sendDoc.RootElement.TryGetProperty("data", out var sendData))
    {
        if (sendData.TryGetProperty("order", out var ordElem)) masterTicket = ordElem.GetUInt64();
        else if (sendData.TryGetProperty("ticket", out var tktElem)) masterTicket = tktElem.GetUInt64();
    }
    Console.WriteLine($"    Master Order Placed! Ticket: {masterTicket}");

    // 5. Verify Trade Copied to Slave
    Console.WriteLine("\n[5] Verifying replicated trade on Slave account...");
    bool replicated = false;
    for (int attempt = 1; attempt <= 15; attempt++)
    {
        await Task.Delay(2000);
        var positions = await demoClient.OpenedOrdersAsync(slaveGuid, apiKey);
        int count = positions.ValueKind == JsonValueKind.Array ? positions.GetArrayLength() : 0;
        Console.WriteLine($"    Attempt {attempt}: Slave active positions count = {count}");
        if (count > 0)
        {
            var firstPos = positions[0];
            ulong slaveTicket = firstPos.GetProperty("ticket").GetUInt64();
            string symbol = firstPos.GetProperty("symbol").GetString() ?? "";
            double volume = firstPos.GetProperty("volume").GetDouble();
            string type = firstPos.GetProperty("type").GetString() ?? "";
            Console.WriteLine($"    --> CONFIRMED ON SLAVE: Ticket={slaveTicket}, Symbol={symbol}, Volume={volume}, Type={type}");
            replicated = true;
            break;
        }
    }

    if (!replicated)
    {
        Console.WriteLine("    WARNING: Slave trade replication timed out.");
    }
    else
    {
        Console.WriteLine("    SUCCESS: Trade successfully replicated to slave account!");
    }

    // 6. Close Position on Master
    if (masterTicket != 0)
    {
        Console.WriteLine($"\n[6] Closing Master trade ticket #{masterTicket}...");
        var closeDoc = await demoClient.OrderCloseAsync(masterGuid, masterTicket, apiKey);
        string retCode = "DONE";
        if (closeDoc.RootElement.TryGetProperty("data", out var cd) && cd.TryGetProperty("returnedStringCode", out var rsc))
        {
            retCode = rsc.GetString() ?? "DONE";
        }
        Console.WriteLine($"    Master OrderClose result: {retCode}");

        // 7. Verify Trade Closed on Slave
        Console.WriteLine("\n[7] Verifying trade closed on Slave...");
        for (int attempt = 1; attempt <= 15; attempt++)
        {
            await Task.Delay(2000);
            var positions = await demoClient.OpenedOrdersAsync(slaveGuid, apiKey);
            int count = positions.ValueKind == JsonValueKind.Array ? positions.GetArrayLength() : 0;
            if (count == 0)
            {
                Console.WriteLine("    SUCCESS: Slave position closed by trade copier!");
                break;
            }
            Console.WriteLine($"    Attempt {attempt}: Slave positions still open: {count}");
        }
    }

    // 8. Remove Copier via gRPC
    if (!string.IsNullOrEmpty(copierId))
    {
        Console.WriteLine($"\n[8] Removing Copier {copierId} via gRPC...");
        var remReply = await copier.RemoveAsync(copierId);
        Console.WriteLine($"    Copier Remove Reply: ok={remReply.Ok}");
    }
}
finally
{
    // 9. Cleanly Disconnect Terminal Sessions
    Console.WriteLine("\n[9] Disconnecting terminal sessions cleanly via /Disconnect...");
    if (masterGuid != null)
    {
        try
        {
            var discM = await demoClient.DisconnectAsync(masterGuid, apiKey);
            Console.WriteLine($"    Master Terminal Cleanly Disconnected: {discM.UniqueIdentifier} (Lifetime: {discM.FullLifeTimeSeconds}s)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Master disconnect error: {ex.Message}");
        }
    }
    if (slaveGuid != null)
    {
        try
        {
            var discS = await demoClient.DisconnectAsync(slaveGuid, apiKey);
            Console.WriteLine($"    Slave Terminal Cleanly Disconnected:  {discS.UniqueIdentifier} (Lifetime: {discS.FullLifeTimeSeconds}s)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Slave disconnect error: {ex.Message}");
        }
    }
    copier.Dispose();
    Console.WriteLine("\n=== CSharpCopier Trade Replication Completed Successfully ===");
}
