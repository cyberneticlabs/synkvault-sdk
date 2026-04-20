# SynkVault SDK Documentation

Comprehensive guides for using the SynkVault Partners API SDK.

## Quick Links

- **[Main README](../packages/js/README.md)** — Installation, configuration, quick start
- **[Resources](RESOURCES.md)** — Detailed API reference for all SDK resources
- **[Chat Feature](CHAT.md)** — Streaming AI conversations

## Documentation Structure

| Document | Purpose |
|---|---|
| [RESOURCES.md](RESOURCES.md) | Complete API reference with examples for Health, Orgs, Ontology, Knowledge, Ingest, and Documents |
| [CHAT.md](CHAT.md) | In-depth Chat API guide — streaming responses, events, session management, examples |

## Concepts

### Resources

The SDK is organized around **resources** — collections of related operations grouped by domain:

- **Health** — API status and version
- **Orgs** — Organization and user management
- **Ontology** — Schema definitions (entity types, relationships)
- **Knowledge** — Query the knowledge graph
- **Ingest** — Load data (text, JSON, URLs)
- **Documents** — Upload and extract from files
- **Chat** — AI agent conversations

Each resource is accessed via the client: `client.health`, `client.orgs`, etc.

### Authentication

All API requests require either:
- **API Key** (preferred for server-to-server): `X-Api-Key` header
- **Bearer Token** (OAuth, user sessions): `Authorization: Bearer` header

### Organization Context

Most requests automatically include your organization ID (`org_id`) as a query parameter. This scopes all data to your tenant.

### Errors

All errors are thrown as `SynkVaultError` exceptions:

```ts
try {
  await client.knowledge.listNodes({ node_name: 'Company' })
} catch (err) {
  if (err instanceof SynkVaultError) {
    console.error(err.statusCode, err.message, err.data)
  }
}
```

---

## Getting Started

### 1. Install

```bash
npm install @synkvault/sdk
```

### 2. Initialize Client

```ts
import { SynkVaultClient } from '@synkvault/sdk'

const client = new SynkVaultClient({
  baseUrl: 'https://api.synkvault.com',
  orgId: 'your-org-uuid',
  apiKey: 'svk_live_...',
})
```

### 3. Use a Resource

```ts
// Check API health
const health = await client.health.check()

// List companies
const result = await client.knowledge.listNodes({ node_name: 'Company' })

// Chat with AI agent
const stream = await client.chat.run({ message: 'Summarize our data' })
for await (const event of stream) {
  console.log(event.content)
}
```

For detailed examples, see [RESOURCES.md](RESOURCES.md) and [CHAT.md](CHAT.md).

---

## Integration Testing

To run tests against a live API:

```bash
SYNKVAULT_TEST_BASE_URL=http://localhost:3102 \
SYNKVAULT_TEST_ORG_ID=your-org-uuid \
SYNKVAULT_TEST_API_KEY=your-api-key \
pnpm test
```

---

## Support

- **API Reference**: [OpenAPI Spec](https://synkvault-web-dev-partners-yqmxkygnlq-nw.a.run.app/api/v1/docs/openapi.json)
- **Package**: [@synkvault/sdk on npm](https://www.npmjs.com/package/@synkvault/sdk)
- **License**: MIT
