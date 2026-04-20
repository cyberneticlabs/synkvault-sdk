# Management Resources

Management endpoints for API health and organization administration.

## Overview

Management resources provide:
- **Health** — API connectivity and status monitoring
- **Orgs** — Organization and user management

---

## Health

Verify API connectivity and version.

### `client.health.check()`

```ts
const result = await client.health.check()
// {
//   status: 'ok',
//   timestamp: '2026-04-20T12:00:00Z',
//   version: '1.0.0'
// }
```

**Use case**: Health checks, monitoring, uptime verification.

---

## Orgs

Manage organizations and users.

### `client.orgs.list()`

List all organizations accessible to the authenticated user. **Does not require `org_id`.**

```ts
const result = await client.orgs.list()
// { data: [
//   { id: '7f1b...', name: 'Acme Corp', role: 'org_admin' },
//   { id: 'a2c3...', name: 'Widgets Inc', role: 'org_member' }
// ]}
```

**Use case**: Multi-org UIs, org switchers, user permission checks.

---

### `client.orgs.listUsers(orgId?)`

List all users in an organization. Requires `org_owner` or `org_admin` role.

```ts
const result = await client.orgs.listUsers()
// Defaults to current org (from config)

const result = await client.orgs.listUsers('7f1b...')
// List users in a specific org
```

**Response**:
```ts
{
  data: [
    { id: 'user-123', name: 'Alice', role: 'org_admin' },
    { id: 'user-456', name: 'Bob', role: 'org_member' }
  ]
}
```

---

## Error Handling

All resources throw `SynkVaultError` on failure:

```ts
import { SynkVaultError } from '@synkvault/sdk'

try {
  await client.orgs.listUsers()
} catch (err) {
  if (err instanceof SynkVaultError) {
    console.error(err.statusCode)  // HTTP status
    console.error(err.message)     // Error message
    console.error(err.data)        // Raw response body
  }
}
```

---

## See Also

- [Ontology Resource](ONTOLOGY.md) — Schema definitions
- [Knowledge Resource](KNOWLEDGE.md) — Query data graphs
- [Ingest Resource](RESOURCES.md#ingest) — Load data
- [Documents Resource](RESOURCES.md#documents) — Manage files
- [Chat Documentation](CHAT.md) — AI agent conversations
