# MetaRPC C# Trade Copier SDK

Authenticate, configure a master–follower relationship, inspect its state, pause and remove it.

**[Documentation](https://metarpc.github.io/CSharpCopier/) · [Quickstart](https://metarpc.github.io/CSharpCopier/quickstart/) · [Authentication](https://metarpc.github.io/CSharpCopier/authentication/) · [API reference](https://metarpc.github.io/CSharpCopier/reference/)**

## Install from source

Requires .NET SDK 8+.

```bash
git clone https://github.com/MetaRPC/CSharpCopier.git
cd CSharpCopier
dotnet build
dotnet build examples/List/List.csproj
```

## First request

Set `MRPC_API_KEY` to your real owner API key in your environment, then run:

```bash
dotnet run --project examples/List
```

This List example is read-only: no account creation or trades. 

## Authentication

Every SDK call sends `ApiKey` (`apikey` in gRPC metadata). The gRPC body also carries `user_key`; the header does not replace it. `TRIAL` is not an owner key. Read the [verified transport contract](https://metarpc.github.io/CSharpCopier/authentication/).

## Scope and verification

Core operations: StartAsync · ListAsync · PauseAsync · RemoveAsync. Advanced server features are distinguished from the supported wrapper surface. Review legacy replication examples before using them with disposable demos. Standard tests need no broker credentials.

See [testing and contributing](https://metarpc.github.io/CSharpCopier/testing/) for the regression suite, demo checklist and Pages build. Edit `docs/`; generated `site/` output is not source.
