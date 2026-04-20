# SynkVault SDK (@synkvault/sdk)

## Overview

Official TypeScript SDK for the SynkVault Partners API. Standalone, independently-versioned npm package for external developers.

- **Package**: `@synkvault/sdk` | **Node**: >=18 | **Build**: tsup (ESM + CJS) | **Tests**: Vitest
- **OpenAPI reference**: `https://synkvault-web-dev-partners-yqmxkygnlq-nw.a.run.app/api/v1/docs/openapi.json`

## Code Layout

- `packages/js/src/client.ts` — Main client class
- `packages/js/src/resources/*.ts` — Resource classes
- `packages/js/src/types.ts` — Public types
- `packages/js/src/errors.ts` — Error handling
- `packages/js/src/index.ts` — Public exports only

## Rules

- Tests must emulate an external developer consumer — see [sdk-testing.md](rules/sdk-testing.md)
- Cross-SDK feature parity — see [sdk-feature-map.md](rules/sdk-feature-map.md)
- Development commands and publishing — see [sdk-development.md](rules/sdk-development.md)
- Bump the version (semver) before every push to master