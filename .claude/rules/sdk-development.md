---
description: Development workflow and scripts for the SynkVault SDK monorepo — applies to all packages
paths:
  - "packages/**"
---

# SDK Development Workflow

## Commands (pnpm only)

```bash
pnpm install          # Install dependencies

pnpm build            # One-time build (tsup, dual ESM + CJS)
pnpm build:watch      # Watch mode

pnpm test             # Run tests once (Vitest)
pnpm test:watch       # Watch mode

pnpm typecheck        # tsc no-emit type check
```

## Scripts reference

| Script | What it does |
|---|---|
| `build` | tsup build |
| `build:watch` | Watch mode |
| `typecheck` | tsc (no emit) |
| `test` | vitest run |
| `test:watch` | vitest watch |
| `prepublishOnly` | build + typecheck (auto-runs before publish) |

## Publishing

- Only `dist/`, `README.md`, and `package.json` are published
- `prepublishOnly` runs build + typecheck automatically
- Public access via `publishConfig.access`
- Bump the version (semver) before every push to master