# SynkVault SDK — Monorepo

Official SDKs for the [SynkVault](https://synkvault.com) Partners API.

## Packages

| Package | Language | Path | Status |
|---------|----------|------|--------|
| [`@synkvault/sdk`](packages/js/README.md) | TypeScript / Node.js | `packages/js/` | Published |
| `SynkVault.Sdk` | .NET (C#) | `packages/dotnet/` | Alpha |

---

## TypeScript / Node.js

See [`packages/js/README.md`](packages/js/README.md) for full documentation.

```bash
npm install @synkvault/sdk
```

```ts
import { SynkVaultClient } from '@synkvault/sdk'

const client = new SynkVaultClient({
  baseUrl: 'https://api.synkvault.com',
  orgId: 'your-org-uuid',
  apiKey: 'svk_live_...',
})

const health = await client.health.check()
```

---

## .NET (C#)

See [`packages/dotnet/README.md`](packages/dotnet/README.md) for full documentation.

```bash
dotnet add package SynkVault.Sdk
```

```csharp
using SynkVault.Sdk;

var client = new SynkVaultClient(new SynkVaultConfig
{
    BaseUrl = "https://api.synkvault.com",
    OrgId   = "your-org-uuid",
    ApiKey  = "svk_live_...",
});

var health = await client.Health.CheckAsync();
```

---

## Repository Layout

```
sdk/
├── packages/
│   ├── js/         TypeScript SDK (@synkvault/sdk)
│   └── dotnet/     .NET SDK (SynkVault.Sdk)
├── package.json    pnpm workspace root
└── pnpm-workspace.yaml
```

## Development

```bash
# Install all JS dependencies
pnpm install

# Build all JS packages
pnpm build

# Test all JS packages
pnpm test

# .NET SDK
cd packages/dotnet
dotnet build
dotnet test
```

## License

MIT
