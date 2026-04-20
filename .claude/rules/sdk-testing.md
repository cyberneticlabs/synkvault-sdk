---
description: SDK unit test guidelines — applies only to tests inside packages/js/src
paths:
  - "packages/js/src/**/*.test.ts"
  - "packages/js/src/**/*.spec.ts"
---

# SDK Unit Test Guidelines

## Core principle: test as a developer consumer

Every test must emulate how an external developer would use the SDK — not how it is implemented internally. The test author's mental model should be: *"I just installed `@synkvault/sdk`. Does it do what the README promises?"*

## What this means in practice

**Always:**
- Initialize the client exactly as a consumer would, using only the public constructor and public config options.
- Call only public methods exposed by the client (e.g. `client.health.check()`, `client.orgs.list()`).
- Assert on **return values and thrown errors** — the observable contract of the SDK.
- Keep `BASE_CONFIG` minimal: `{ baseUrl, orgId, apiKey }` (or `token`). Never add internal-only fields.

**Avoid:**
- Inspecting `fetch` mock call arguments to verify URLs, headers, or request bodies — this tests internal implementation, not developer-visible behavior. Use return-value assertions instead.
- Accessing internal properties or methods not exported from `index.ts`.
- Writing tests that would break if the HTTP transport is swapped out but the public API contract stays the same.

## Exception: transport-level tests

`client.test.ts` is the one file allowed to inspect fetch internals — it tests the `request()` method's transport contract (auth headers, query params, timeout, body serialization). All resource tests (`health.test.ts`, `orgs.test.ts`, etc.) must stay consumer-facing.

## Test structure template

```ts
// 1. Arrange — set up exactly as a developer consumer would
const client = new SynkVaultClient({ baseUrl: '...', orgId: '...', apiKey: '...' })
vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: true, status: 200, text: () => Promise.resolve(JSON.stringify(FIXTURE)) }))

// 2. Act — call the public SDK method
const result = await client.resource.method(args)

// 3. Assert — check what the developer receives back
expect(result).toEqual(FIXTURE)
```

For error cases, assert on the thrown `SynkVaultError` shape (message, `statusCode`) — not on which HTTP status code the mock returned.

## Naming

Describe blocks name the resource class (e.g. `describe('HealthResource')`). `it()` strings read as product requirements: `'returns the health payload'`, `'throws SynkVaultError with statusCode 404 when node is not found'`.