# SDK Resources

Detailed API documentation for all SynkVault SDK resources.

## Table of Contents

1. **[Health](RESOURCES.md#health)** — API health check
2. **[Orgs](RESOURCES.md#orgs)** — Organization and user management
3. **[Ontology](RESOURCES.md#ontology)** — Schema and entity definitions
4. **[Knowledge](RESOURCES.md#knowledge)** — Query and browse knowledge graph
5. **[Ingest](RESOURCES.md#ingest)** — Load data into SynkVault
6. **[Documents](RESOURCES.md#documents)** — Upload and manage documents
7. **[Chat](CHAT.md)** — AI agent conversations (detailed in separate doc)

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

## Ontology

The ontology is the schema — the set of entity types and relationships that define your knowledge domain.

### `client.ontology.list(params?)`

List all ontology nodes (entity types).

```ts
const result = await client.ontology.list({
  include_descriptions: true,
  page: 1,
  page_size: 50,
})
// { data: [
//   {
//     id: 'node-123',
//     name: 'Company',
//     label: 'Company',
//     parentId: null,
//     description: 'A legal business entity',
//     properties: { name: '...', founded: '...' },
//     allowDirectQuery: true,
//     includeInMapPlotting: true
//   },
//   ...
// ]}
```

**Parameters**:
- `include_descriptions?: boolean` — Include node descriptions
- `page?: number` — Pagination (default: 1)
- `page_size?: number` — Items per page (default: 25)

---

### `client.ontology.get(nodeId)`

Get a single ontology node by ID.

```ts
const result = await client.ontology.get('node-123')
// { data: { id: 'node-123', name: 'Company', ... } }
```

---

### `client.ontology.getChildren(nodeId, params?)`

Get child nodes of a parent node (for hierarchical ontologies).

```ts
const result = await client.ontology.getChildren('node-123', {
  depth: 2,
})
// { data: [ ...children... ] }
```

**Parameters**:
- `depth?: number` — How many levels deep to fetch (default: 1)

---

### `client.ontology.updateDescription(nodeId, body)`

Update a node's description. Requires `org_admin` or `org_owner` role.

```ts
await client.ontology.updateDescription('node-123', {
  description: 'A business entity registered with a government agency',
})
```

---

### `client.ontology.updatePropertyDescription(nodeId, body)`

Update the description of a property within a node. Requires admin privileges.

```ts
await client.ontology.updatePropertyDescription('node-123', {
  property_name: 'founded_year',
  description: 'Year the company was incorporated',
})
```

---

## Knowledge

Query and browse the knowledge graph — the actual data that conforms to the ontology schema.

### `client.knowledge.listNodes(params)`

Search for all records of a specific node type.

```ts
const result = await client.knowledge.listNodes({
  node_name: 'Company',
  page: 1,
  page_size: 20,
  nested: true,
})
// {
//   data: [
//     {
//       record_id: 'Company:abc-123',
//       node_name: 'Company',
//       properties: { name: 'Acme Corp', revenue: '1B' },
//       inboundRelationships: [
//         { sourceNode: 'Person', propName: 'works_at', key: 'Person:xyz' }
//       ]
//     },
//     ...
//   ],
//   pagination: {
//     page: 1,
//     page_size: 20,
//     total_count: 142,
//     total_pages: 8
//   },
//   inboundRelationships: [ ... ]
// }
```

**Parameters**:
- `node_name: string` — Required. Entity type to search (e.g., "Company", "Person")
- `page?: number` — Pagination (default: 1)
- `page_size?: number` — Items per page (default: 25)
- `nested?: boolean` — Include related records (default: false)

**Response fields**:
- `data[]` — Array of records
- `pagination` — Paging metadata
- `inboundRelationships[]` — Reverse relationships (entities pointing *to* the queried type)

---

### `client.knowledge.getNode(params)`

Get a single record by its full record ID.

```ts
const result = await client.knowledge.getNode({
  record_id: 'Company:abc-123',
})
// { data: {
//   record_id: 'Company:abc-123',
//   node_name: 'Company',
//   properties: { name: 'Acme Corp', ... },
//   inboundRelationships: [ ... ]
// }}
```

**Parameters**:
- `record_id: string` — Required. Full record ID (format: `NodeName:key`)

---

## Ingest

Load data into SynkVault. Choose the format that fits your data.

### `client.ingest.text(body)`

Ingest unstructured text. The backend extracts entities and relationships.

```ts
await client.ingest.text({
  content: `
    Acme Corp is a software company founded in 2010.
    CEO is Alice Johnson. Headquarters in San Francisco.
  `,
})
```

**Limits**: 500 KB per request.

**Use case**: Blog posts, documents, chat transcripts, notes — anything that should be extracted into entities.

---

### `client.ingest.json(body)`

Ingest structured data with a target entity type.

```ts
await client.ingest.json({
  target_class: 'Company',
  data: {
    name: 'Widgets Inc',
    founded: 2015,
    ceo: 'Bob Smith',
    revenue: '500M',
  },
})
```

**Parameters**:
- `target_class: string` — The ontology node to map data to
- `data: object` — Structured data matching the node's properties

**Use case**: Structured CSV, database exports, form submissions.

---

### `client.ingest.url(body)`

Fetch and ingest a URL. The backend extracts the page content.

```ts
await client.ingest.url({
  url: 'https://example.com/news/acme-launches-new-product',
})
```

**Use case**: Web scraping, RSS feeds, live article ingestion.

---

## Documents

Upload and manage uploaded files.

### `client.documents.upload({ file, dryrun? })`

Upload a file (PDF, Word, Excel, etc.) for processing. The backend extracts text and structured data.

```ts
const file = new File([buffer], 'annual-report.pdf', {
  type: 'application/pdf',
})

const doc = await client.documents.upload({ file })
// {
//   id: 'doc-abc-123',
//   original_file_name: 'annual-report.pdf',
//   document_size: 2500000,
//   status: 'pending',
//   created_on: '2026-04-20T12:00:00Z'
// }
```

**Parameters**:
- `file: Blob | File` — Document to upload
- `dryrun?: boolean` — Validate without processing (default: false)

**Response** (`Document`):
- `id` — Document ID (reference for querying extracted data)
- `original_file_name` — Uploaded filename
- `document_size` — File size in bytes
- `status` — `'pending'` | `'processing'` | `'processed'` | `'failed'`
- `created_on` — ISO timestamp

---

### `client.documents.list(params?)`

List uploaded documents with optional filtering.

```ts
const result = await client.documents.list({
  status: 'processed',
  page: 1,
  page_size: 20,
})
// {
//   data: [ ...documents... ],
//   pagination: { page: 1, page_size: 20, total_count: 142, total_pages: 8 }
// }
```

**Parameters**:
- `status?: string` — Filter by status
- `page?: number` — Pagination
- `page_size?: number` — Items per page

---

### `client.documents.get(id, params?)`

Get a specific document and optionally its extracted properties.

```ts
const doc = await client.documents.get('doc-abc-123', {
  include: 'extracted',
})
// {
//   id: 'doc-abc-123',
//   original_file_name: 'annual-report.pdf',
//   ...
//   extracted_properties: [
//     { key: 'company_name', value: 'Acme Corp' },
//     { key: 'revenue', value: '$1B' },
//     ...
//   ]
// }
```

**Parameters**:
- `include?: string` — `'extracted'` to include extracted data

---

## Error Handling

All resources throw `SynkVaultError` on failure:

```ts
import { SynkVaultError } from '@synkvault/sdk'

try {
  await client.ontology.get('nonexistent-id')
} catch (err) {
  if (err instanceof SynkVaultError) {
    console.error(err.statusCode)  // HTTP status (e.g., 404)
    console.error(err.message)     // Error message
    console.error(err.data)        // Raw response body
  }
}
```

---

## See Also

- [Chat Documentation](CHAT.md) — AI agent conversations
- [Main README](../packages/js/README.md) — Quick start
- [API Reference](https://synkvault-web-dev-partners-yqmxkygnlq-nw.a.run.app/api/v1/docs/openapi.json) — OpenAPI spec
