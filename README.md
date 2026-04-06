# @synkvault/sdk

Official TypeScript SDK for the [SynkVault](https://synkvault.com) Partners API.

## Requirements

- Node.js 18+
- A SynkVault Partner account with an API key or JWT token

## Installation

```bash
npm install @synkvault/sdk
# or
pnpm add @synkvault/sdk
# or
yarn add @synkvault/sdk
```

## Quick Start

```ts
import { SynkVaultClient } from '@synkvault/sdk'

const client = new SynkVaultClient({
  baseUrl: 'https://api.synkvault.com',
  orgId: 'your-org-uuid',
  apiKey: 'svk_live_...',
})

const health = await client.health.check()
console.log(health.status) // "ok"
```

## Authentication

The client supports two authentication methods. Provide one:

| Method    | Config field | Header sent            |
|-----------|--------------|------------------------|
| API Key   | `apiKey`     | `X-Api-Key: <key>`     |
| JWT Token | `token`      | `Authorization: Bearer <token>` |

```ts
// API key
const client = new SynkVaultClient({
  baseUrl: 'https://api.synkvault.com',
  orgId: 'your-org-uuid',
  apiKey: 'svk_live_...',
})

// JWT token
const client = new SynkVaultClient({
  baseUrl: 'https://api.synkvault.com',
  orgId: 'your-org-uuid',
  token: 'eyJhbGci...',
})
```

## Configuration

```ts
interface SynkVaultConfig {
  baseUrl: string    // Partners API base URL (required)
  orgId:   string    // Your organization UUID (required)
  apiKey?: string    // API key auth (one of apiKey or token required)
  token?:  string    // JWT bearer token auth
  timeout?: number   // Request timeout in ms (default: 30000)
}
```

## Resources

### `client.orgs`

List all organizations accessible to the authenticated credential. Does not require `org_id`.

```ts
const { data } = await client.orgs.list()
// [{ id: '7f1b...', name: 'Acme Corp', role: 'org_admin' }, ...]
```

Use this to discover which `org_id` values to pass when calling other resources.

---

### `client.health`

Check API availability. Does not require authentication.

```ts
const result = await client.health.check()
// { status: 'ok', timestamp: '2026-01-01T00:00:00.000Z', version: '1.0.0' }
```

---

### `client.ontology`

Browse your organization's ontology — the structured type system that describes your knowledge base.

#### `list(params?)`

Retrieve all ontology nodes.

```ts
const { data, count } = await client.ontology.list()

// With options
const { data } = await client.ontology.list({
  include_descriptions: true,  // default: true
  include_properties: true,    // default: true
})
```

#### `get(nodeId)`

Retrieve a single ontology node by ID.

```ts
const { data } = await client.ontology.get('node-uuid')
console.log(data.name)  // e.g. "city"
```

#### `getChildren(nodeId, params?)`

Retrieve children / descendants of a node.

```ts
const { data } = await client.ontology.getChildren('node-uuid', { depth: 3 })
```

`depth` is clamped between 1 and 10 (default: 1).

**OntologyNode shape:**
```ts
{
  id:                  string
  name:                string
  label:               string
  parentId:            string | null
  description?:        string
  properties?:         Record<string, unknown>
  unique_properties?:  Record<string, unknown>
  allowDirectQuery:    boolean
  includeInMapPlotting: boolean
}
```

---

### `client.knowledge`

Query records stored in your knowledge base.

#### `listNodes(params)`

Retrieve paginated records for a given ontology type.

```ts
const result = await client.knowledge.listNodes({
  node_name: 'city',       // required — ontology type name
  page: 1,                 // default: 1
  page_size: 20,           // default: 10, max: 100
  nested: true,            // resolve relationships (default: false)
  start_date: '2024-01-01T00:00:00Z',  // filter by meta.timestamp
  end_date:   '2024-12-31T23:59:59Z',
  node_id: 'ontology-node-uuid',       // optional — for edge resolution
})

console.log(result.data)               // array of records
console.log(result.pagination)         // { page, page_size, total_count, total_pages }
console.log(result.edgeProperties)     // columns resolved via graph traversal
```

#### `getNode(params)`

Retrieve a single record by its SurrealDB record ID.

```ts
const result = await client.knowledge.getNode({
  record_id: 'city:abc123',  // required
  nested: true,
})

console.log(result.data)
```

---

### `client.ingest`

Push content into your knowledge base for processing.

#### `ingest.text(body)`

Ingest freeform text or markdown content (max 500 KB).

```ts
await client.ingest.text({
  content: '# Meeting Notes\n\nDecided to expand into APAC markets...',
})
```

#### `ingest.json(body)`

Ingest structured data mapped to a specific ontology class.

```ts
await client.ingest.json({
  target_class: 'announcement',  // ontology node name or path (e.g. "doc.announcement")
  data: {
    title: 'Q2 All-Hands',
    date:  '2026-06-01',
    body:  'Join us for our quarterly all-hands meeting.',
  },
})
```

#### `ingest.url(body)`

Ingest content from an HTTP/HTTPS URL.

```ts
await client.ingest.url({
  url: 'https://example.com/press-release',
})
```

**Ingest response:**
```ts
{ success: boolean, message: string }
```

---

## Error Handling

All API errors throw a `SynkVaultError`:

```ts
import { SynkVaultClient, SynkVaultError } from '@synkvault/sdk'

try {
  await client.ontology.get('nonexistent-id')
} catch (err) {
  if (err instanceof SynkVaultError) {
    console.error(err.statusCode)  // e.g. 404
    console.error(err.message)     // e.g. "Node not found"
    console.error(err.data)        // raw response body (if any)
  }
}
```

| Status | Meaning |
|--------|---------|
| 400    | Bad request — invalid or missing parameters |
| 401    | Unauthorized — invalid or missing credentials |
| 403    | Forbidden — your credentials don't have access to this organization |
| 404    | Not found |
| 500    | Internal server error |

---

## TypeScript

The SDK is written in TypeScript and ships its own type declarations. No `@types/*` package needed.

All request parameter shapes and response interfaces are exported:

```ts
import type {
  SynkVaultConfig,
  Org,
  OrgsResponse,
  OntologyNode,
  OntologyResponse,
  OntologyNodeResponse,
  OntologyChildrenResponse,
  KnowledgeNodesResponse,
  KnowledgeNodeResponse,
  PaginationMeta,
  IngestResponse,
  HealthResponse,
  GetOntologyParams,
  GetOntologyChildrenParams,
  GetKnowledgeNodesParams,
  GetKnowledgeNodeParams,
  IngestTextBody,
  IngestJsonBody,
  IngestUrlBody,
} from '@synkvault/sdk'
```

---

## Integration Testing

Set environment variables to run the integration test suite against a live instance:

```bash
SYNKVAULT_TEST_BASE_URL=http://localhost:3102 \
SYNKVAULT_TEST_ORG_ID=your-org-uuid \
SYNKVAULT_TEST_API_KEY=your-api-key \
pnpm test
```

Integration tests are skipped automatically when `SYNKVAULT_TEST_BASE_URL` is not set.

---

## License

MIT
