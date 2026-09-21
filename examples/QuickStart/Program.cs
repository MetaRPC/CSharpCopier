using System;
using System.Threading.Tasks;
using MetaRPC.Copier;
using MetaRPC.Copier.Models;

Console.WriteLine("=== MetaRPC CSharpCopier Quick Start Demo ===");
const string apiKey = "TRIAL";

// 1. Provision Live Demo Account
Console.WriteLine("\n[1] Provisioning live demo account on MetaQuotes-Demo...");
var demoClient = new DemoAccountClient("https://mt5.mrpc.pro");
var master = await demoClient.OpenDemoAccountAsync(new GuiDemoOpenAccountRequest { Server = "MetaQuotes-Demo" }, apiKey);
Console.WriteLine($"    Master Account Provisioned: #{master.Login} on {master.Server}");

// 2. Connect Terminal via ConnectEx with APIKey: TRIAL
Console.WriteLine($"\n[2] Connecting terminal via ConnectEx (APIKey: {apiKey})...");
var conn = await demoClient.ConnectExAsync(master.Login, master.Password, master.Server, apiKey);
Console.WriteLine($"    Terminal Connected! Instance GUID: {conn.TerminalInstanceGuid}");

// 3. Check Trade Copier Service
Console.WriteLine($"\n[3] Interacting with Copier Service (userKey: {apiKey})...");
var copier = new CopierService("https://copy.mrpc.pro:443", apiKey);
var list = await copier.ListAsync();
Console.WriteLine($"    Active Copiers for {apiKey}: {list.Copiers.Count}");

// 4. Cleanly Disconnect Terminal Session
Console.WriteLine($"\n[4] Disconnecting terminal session {conn.TerminalInstanceGuid}...");
var disc = await demoClient.DisconnectAsync(conn.TerminalInstanceGuid, apiKey);
Console.WriteLine($"    Terminal Cleanly Disconnected: {disc.UniqueIdentifier} (Lifetime: {disc.FullLifeTimeSeconds}s)");

Console.WriteLine("\n=== CSharpCopier Quick Start Completed Successfully ===");
