# Management Resources

Management endpoints for API health and organization administration.

## Overview

Management resources provide:
- **Health** — API connectivity and status monitoring
- **Orgs** — Organization and user management

---

## Health

Verify API connectivity and version.

### `client.Health.CheckAsync()`

```csharp
var result = await client.Health.CheckAsync();
// {
//   Status = "ok",
//   Timestamp = DateTime.Parse("2026-04-20T12:00:00Z"),
//   Version = "1.0.0"
// }
```

**Use case**: Health checks, monitoring, uptime verification.

---

## Orgs

Manage organizations and users.

### `client.Orgs.ListAsync()`

List all organizations accessible to the authenticated user. **Does not require `org_id`.**

```csharp
var result = await client.Orgs.ListAsync();
// { Data = [
//   { Id = "7f1b...", Name = "Acme Corp", Role = "org_admin" },
//   { Id = "a2c3...", Name = "Widgets Inc", Role = "org_member" }
// ]}
```

**Use case**: Multi-org UIs, org switchers, user permission checks.

---

### `client.Orgs.ListUsersAsync(orgId?)`

List all users in an organization. Requires `org_owner` or `org_admin` role.

```csharp
// Defaults to current org (from config)
var result = await client.Orgs.ListUsersAsync();

// List users in a specific org
var result = await client.Orgs.ListUsersAsync("7f1b...");
```

**Response**:
```csharp
// {
//   Data = [
//     { Id = "user-123", Name = "Alice", Role = "org_admin" },
//     { Id = "user-456", Name = "Bob", Role = "org_member" }
//   ]
// }
```

---

## Error Handling

All resources throw `SynkVaultException` on failure:

```csharp
try
{
    var result = await client.Orgs.ListUsersAsync();
}
catch (SynkVaultException ex)
{
    Console.WriteLine(ex.StatusCode);  // HTTP status
    Console.WriteLine(ex.Message);     // Error message
    Console.WriteLine(ex.Data);        // Raw response body
}
```

---

## See Also

- [Ontology Resource](ONTOLOGY.md) — Schema definitions
- [Knowledge Resource](KNOWLEDGE.md) — Query data graphs
- [Ingest Resource](RESOURCES.md#ingest) — Load data
- [Documents Resource](RESOURCES.md#documents) — Manage files
- [Chat Documentation](CHAT.md) — AI agent conversations
