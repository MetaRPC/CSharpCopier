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

## Plan an MT4 or MT5 copier integration

Use these free resources before connecting disposable demo accounts:

- [Choose your setup and review service costs](https://mrpc.pro/start/copier?lang=en&utm_source=github&utm_medium=referral&utm_campaign=copier_sdk_setup_nov17&utm_content=csharpcopier_readme_setup) — compare the control-panel workflow with SDK integration. Hosted terminals and running copiers are billed separately, including on broker demo accounts.
- [Check follower lot sizing](https://mrpc.pro/tools/copier-sizing?lang=en&utm_source=github&utm_medium=referral&utm_campaign=copier_sdk_setup_nov17&utm_content=csharpcopier_readme_sizing) — preview requested volume and broker minimum/maximum/step conflicts. This is a worksheet, not a broker quote or a guarantee of equal financial risk.
- [Review symbol mapping](https://mrpc.pro/tools/symbol-mapping?lang=en&utm_source=github&utm_medium=referral&utm_campaign=copier_sdk_setup_nov17&utm_content=csharpcopier_readme_symbols) — compare symbol names, then verify the actual contract specifications with each broker before copying.
- [Work through the migration checklist](https://mrpc.pro/guides/copier-migration?lang=en&utm_source=github&utm_medium=referral&utm_campaign=copier_sdk_setup_nov17&utm_content=csharpcopier_readme_migration) — validate sizing, one demo opening and closing, pause/resume and rejection logs before moving an integration.

These tools do not place trades or connect your accounts. MetaRPC provides developer APIs and hosted software; we do not manage or invest customer funds. Use this SDK's quickstart and recipes for the actual API calls.

## Demo workflow and troubleshooting

Start with the read-only example above, then use disposable demo accounts for a copying test. The [lifecycle recipes](https://metarpc.github.io/CSharpCopier/recipes/) show the SDK workflow; [position sizing](https://metarpc.github.io/CSharpCopier/sizing/) explains the settings, and [troubleshooting](https://metarpc.github.io/CSharpCopier/troubleshooting/) covers rejected trades and uncertain outcomes.

- [Watch the recorded copier troubleshooting walkthrough](https://mrpc.pro/tutorials/console/copier-troubleshooting?utm_source=github&utm_medium=referral&utm_campaign=sdk_guides_nov12&utm_content=csharpcopier) — a real demo rejection, corrected volume, and verified opening/closing. This video uses the control panel; use this SDK's recipes for code.
- [Open the control panel](https://mrpc.pro/my) to inspect your account pair and trade results.
- [Pricing](https://mrpc.pro/#pricing) — hosted terminals and copying can incur service charges, including on broker demo accounts. Demo funds are not MetaRPC service credit.

## Scope and verification

cTrader Spotware demo ↔ MT5 and TradeLocker HEROFX demo ↔ MT5 acceptance passed in both directions on November 10, 2026: five sizing modes, pause/resume, live/persisted logs and API-key errors. See the [provider guide](https://metarpc.github.io/CSharpCopier/providers/) and [dated results](https://metarpc.github.io/CSharpCopier/testing/).

MT4 FreshForex-Demo and MT5 MetaQuotes-Demo acceptance passed on November 10, 2026: opens/closes, five sizing modes, pause/resume, live/persisted logs and API-key failures. See the [separate broker test evidence](https://metarpc.github.io/CSharpCopier/testing/).

Core operations: StartAsync · ListAsync · PauseAsync · RemoveAsync. Advanced server features are distinguished from the supported wrapper surface. Review legacy replication examples before using them with disposable demos. Standard tests need no broker credentials.

See [testing and contributing](https://metarpc.github.io/CSharpCopier/testing/) for the regression suite, demo checklist and Pages build. Edit `docs/`; generated `site/` output is not source.
