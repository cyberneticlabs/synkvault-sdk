# JavaScript/TypeScript SDK

Official Node.js & browser SDK for the SynkVault Partners API.

## Installation

```bash
npm install @synkvault/sdk
```

**Requirements**: Node >=18

## Quick Start

### 1. Initialize Client

```ts
import { SynkVaultClient } from '@synkvault/sdk'

const client = new SynkVaultClient({
  baseUrl: 'https://api.synkvault.com',
  orgId: 'your-org-uuid',
  apiKey: 'svk_live_...',
})
```

### 2. Use a Resource

```ts
// Check API health
const health = await client.health.check()

// List companies
const result = await client.knowledge.listNodes({ node_name: 'Company' })

// Chat with AI agent (streaming)
const stream = await client.chat.run({ message: 'Summarize our data' })
for await (const event of stream) {
  console.log(event.content)
}
```

## Documentation

- **[Management Resource](MANAGEMENT.md)** — Health checks & organization management
- **[Ontology Resource](ONTOLOGY.md)** — Schema definitions and entity types
- **[Knowledge Resource](KNOWLEDGE.md)** — Query and browse knowledge graphs
- **[Ingest Resource](INGEST.md)** — Load data (text, JSON, URLs, documents)
- **[Chat Feature](CHAT.md)** — Streaming conversations, events, session management
- **[Main Package README](../../packages/js/README.md)** — Installation & configuration details

## Error Handling

All errors are thrown as `SynkVaultError`:

```ts
try {
  await client.knowledge.listNodes({ node_name: 'Company' })
} catch (err) {
  if (err instanceof SynkVaultError) {
    console.error(err.statusCode, err.message, err.data)
  }
}
```

## Testing

To run tests against a live API:

```bash
SYNKVAULT_TEST_BASE_URL=http://localhost:3102 \
SYNKVAULT_TEST_ORG_ID=your-org-uuid \
SYNKVAULT_TEST_API_KEY=your-api-key \
pnpm test
```

## Support

- **API Reference**: [OpenAPI Spec](https://synkvault-web-dev-partners-yqmxkygnlq-nw.a.run.app/api/v1/docs/openapi.json)
- **npm Package**: [@synkvault/sdk](https://www.npmjs.com/package/@synkvault/sdk)
- **License**: MIT
