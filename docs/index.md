---
hide:
  - toc
---
<div class="hero"><span class="eyebrow">METARPC / DEVELOPER GUIDES</span><h1>Trade copying.<br>Built with C#.</h1><p>From your first authenticated request to a monitored master–follower workflow. Clear examples, explicit sizing and practical recovery.</p><a class="md-button md-button--primary" href="quickstart/">Start building →</a> <a class="md-button" href="reference/">Explore the API</a><div class="hero-meta">TLS transport · Owner authentication · Demo-first validation</div></div>

<div class="grid cards" markdown>

- **01 · Connect with confidence**

    Understand keys, account IDs and transport before sending a request.

    [Authentication →](authentication.md)

- **02 · Control the lifecycle**

    Start once, inspect state, pause and clean up your test.

    [Copier lifecycle →](lifecycle.md)

- **03 · Make sizing explicit**

    Choose lot, balance or equity sizing and account for broker constraints.

    [Sizing and settings →](sizing.md)

- **04 · Diagnose the result**

    Separate network success from management success and broker execution.

    [Troubleshooting →](troubleshooting.md)

</div>

## Your first successful request

Requires .NET SDK 8+. Install dependencies and run the read-only List example. Keep your key in `MRPC_API_KEY`.

```csharp
using MetaRPC.Copier;
using var service = new CopierService("https://copy.mrpc.pro", Environment.GetEnvironmentVariable("MRPC_API_KEY") ?? "");
var result = await service.ListAsync();
if (!result.Ok) throw new Exception(result.Error);
foreach (var copier in result.Copiers) Console.WriteLine($"{copier.Id}: paused={copier.Paused}");
```

[Full setup](quickstart.md) · [Testing guide](testing.md) · [All SDKs](sdks.md)
