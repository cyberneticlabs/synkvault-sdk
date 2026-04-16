# SynkVault SDK (@synkvault/sdk)

## Overview

Official TypeScript SDK for the SynkVault Partners API. A standalone, independently-versioned npm package consumed by external developers.

- **Package**: `@synkvault/sdk`
- **Node**: >=18
- **License**: MIT
- **Build**: tsup (dual ESM + CJS)
- **Tests**: Vitest

## Reference
This sdk implementation should always use the SynkVault OpenaAPI reference could be foudn in https://synkvault-web-dev-partners-yqmxkygnlq-nw.a.run.app/api/v1/docs/openapi.json

## Key Rules

### Test Philosophy

Every test must emulate how an external developer would use the SDK — not implementation details. See [.claude/rules/sdk-testing.md](.claude/rules/sdk-testing.md).

**Structure**:
- **Arrange**: Initialize client exactly as a consumer would (public constructor only)
- **Act**: Call public methods
- **Assert**: Check return values and thrown errors

**Exception**: `src/__tests__/client.test.ts` can inspect fetch internals; all resource tests must be consumer-facing.

### Code Organization

- `packages/js/src/client.ts` — Main client class
- `packages/js/src/resources/*.ts` — Resource classes (health, orgs, ingest, etc.)
- `packages/js/src/types.ts` — Public type exports
- `packages/js/src/errors.ts` — Error handling
- `packages/js/src/index.ts` — Public exports only

### Publishing

- Only `dist/`, `README.md`, and `package.json` are published
- `prepublishOnly` hook runs build + typecheck
- Public access via `publishConfig.access`

### Development

```bash
# Install (pnpm only)
pnpm install

# Build
pnpm build          # One-time
pnpm build:watch    # Watch mode

# Test
pnpm test           # Run once
pnpm test:watch     # Watch mode

# Check types
pnpm typecheck
```

## Scripts

- `build` — tsup build
- `build:watch` — Watch mode
- `typecheck` — tsc (no emit)
- `test` — vitest run
- `test:watch` — vitest watch
- `prepublishOnly` — build + typecheck (auto-runs before publish)

## Version

Alwasy pump the version when you are asked to push changes to master. Follow semantic versioning for GA.
