# First request with C#

List your own copiers to check connectivity and identity without creating a terminal or placing a trade.

## 1. Install

Requires **.NET SDK 8+**. These instructions build from source; they do not assume a registry release exists.

```bash
git clone https://github.com/MetaRPC/CSharpCopier.git
cd CSharpCopier
dotnet build
dotnet build examples/List/List.csproj
```



## 2. Set your owner key

=== "Bash"
    ```bash
    export MRPC_API_KEY='your-owner-key'
    ```
=== "PowerShell"
    ```powershell
    $env:MRPC_API_KEY = 'your-owner-key'
    ```

Use the real owner API key, not a broker password, terminal ID or `TRIAL`. Read [authentication](authentication.md) if access is refused.

## 3. Run

```bash
dotnet run --project examples/List
```

Source: [`examples/List/Program.cs`](https://github.com/MetaRPC/CSharpCopier/blob/main/examples/List/Program.cs).

```csharp
using MetaRPC.Copier;
using var service = new CopierService("https://copy.mrpc.pro", Environment.GetEnvironmentVariable("MRPC_API_KEY") ?? "");
var result = await service.ListAsync();
if (!result.Ok) throw new Exception(result.Error);
foreach (var copier in result.Copiers) Console.WriteLine($"{copier.Id}: paused={copier.Paused}");
```

A successful empty list means this owner has no copiers. Treat failed requests as errors, not empty accounts.

## 4. Prepare a demo workflow

Read [the lifecycle guide](lifecycle.md), then [choose sizing](sizing.md). Existing replication examples under `examples/` are advanced integration examples: they may provision accounts and trade. Inspect them and their cleanup before running with disposable demos. List is the default starting point.
