# CSharpCopier — Cloud Trade Copier .NET / C# SDK for MetaTrader 4 & 5

[![NuGet](https://img.shields.io/badge/nuget-MetaRPC.Copier-blue.svg)](https://www.nuget.org/packages)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Docs](https://img.shields.io/badge/docs-CSharpCopier-0083ff.svg)](https://github.com/MetaRPC/CSharpCopier/tree/main/docs)
[![Latency](https://img.shields.io/badge/execution-<20ms-success.svg)](https://mrpc.pro)
[![Platform](https://img.shields.io/badge/.NET-8.0%20|%209.0-purple.svg)](https://dotnet.microsoft.com/)

> **Official C# / .NET SDK for MetaRPC Cloud Trade Copier (`copy.mrpc.pro:443`).**  
> Replicate trades in real time across MetaTrader 4 and MetaTrader 5 accounts with sub-20ms execution latency — **without running desktop terminals or Windows VPS.**

---

## ⚡ Why MetaRPC Trade Copier?

- **Cross-Platform & Cross-Broker**: Copy effortlessly from **MT4 to MT5**, **MT5 to MT4**, MT4 to MT4, or MT5 to MT5.
- **Zero Terminal / VPS Requirement**: Entire trade synchronization runs in high-speed cloud memory co-located with London (LD4) and New York (NY4) brokers.
- **Built for Prop Firms & Fund Managers**: Replicate master signals to 100+ slave accounts simultaneously with custom lot sizing, risk rules, and slippage guards.
- **Advanced Risk Controls**:
  - Fixed Lot or Proportional Lot Multiplier
  - Reverse Trading / Inverted Orders
  - Copy Stop Loss (SL) & Take Profit (TP)
  - Max Drawdown and equity stop protections

---

## 📦 Installation

```bash
dotnet add package MetaRPC.Copier
```

---

## 🚀 30-Second Quick Start

```csharp
using System;
using System.Threading.Tasks;
using MetaRPC.CSharpCopier;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Connect to MetaRPC Trade Copier Cloud Service
        // Get your free API key at https://mrpc.pro/signup
        var copier = new CopierService("https://copy.mrpc.pro:443", "your_mrpc_api_key");

        // 2. Define Master and Slave accounts
        var master = new Account {
            Type = "MT5",
            User = 10001,
            Password = "master_password",
            Server = "MetaQuotes-Demo"
        };

        var slave = new Account {
            Type = "MT5",
            User = 20002,
            Password = "slave_password",
            Server = "ICMarkets-Demo"
        };

        // 3. Configure trade replication settings
        var request = new StartRequest {
            UserKey = "your_mrpc_api_key",
            Master = master,
            Slave = slave,
            RiskType = "LotMultiplier",
            RiskValue = "1.0",           // 1:1 lot ratio
            CopySlTp = true              // Mirror Stop Loss & Take Profit
        };

        // 4. Start trade replication
        Console.WriteLine("Starting Cloud Trade Copier...");
        var response = await copier.StartAsync(request);
        Console.WriteLine($"Copier Active! Instance ID: {response.InstanceId}");
    }
}
```

---

## 🔑 Getting Your API Key & Free Trial

1. **Sign Up**: Create your free account at [https://mrpc.pro/signup](https://mrpc.pro/signup).
2. **Copy API Key**: Open your portal dashboard at [https://mrpc.pro/my](https://mrpc.pro/my) to copy your API token.
3. **Web GUI Management**: You can also monitor and manage copiers via the visual web UI at [https://mrpc.pro/my](https://mrpc.pro/my).

---

## 🌐 Production Endpoints

| Environment | Host / URL | Port | Protocol | Purpose |
| :--- | :--- | :--- | :--- | :--- |
| **Copier Production gRPC** | `copy.mrpc.pro` | `443` | TLS / gRPC | Real-time low-latency copier engine |
| **Web Portal / Copier GUI** | [https://mrpc.pro/my](https://mrpc.pro/my) | `443` | HTTPS | Monitor trade logs, cycles & latency |
| **Account Registration** | [https://mrpc.pro/signup](https://mrpc.pro/signup) | `443` | HTTPS | Instant free trial registration |

---

## 🏢 Compatible Brokers & Prop Firms

Works seamlessly across 500+ MetaTrader server environments:
- **Prop Firms**: FTMO, FundedNext, The Funded Trader, E8 Funding, Alpha Capital, SurgeTrader.
- **Brokers**: IC Markets, Pepperstone, Exness, Tickmill, XM, FXCM, FP Markets, Eightcap, AvaTrade.

---

## 📚 Documentation & Guides

- 📖 [Trade Copier Documentation](https://github.com/MetaRPC/CSharpCopier/tree/main/docs)
- 🚀 [Quick Start Walkthrough](https://github.com/MetaRPC/CSharpCopier/blob/main/docs/All_Guides/Your_First_Project.md)
- ⚙️ [Copier Parameters Reference](https://github.com/MetaRPC/CSharpCopier/blob/main/docs/API_Reference/Copier_Parameters.md)

---

## 📄 License

This SDK is open-sourced under the [MIT License](LICENSE).  
Cloud infrastructure and copier execution engines are operated by [MetaRPC](https://mrpc.pro).
