# @synkvault/sdk

Official TypeScript SDK for the [SynkVault](https://synkvault.com) Partners API.

## Requirements

- Node.js 18+
- A SynkVault Partner account with an API key

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

All requests are authenticated via API key, sent as the `X-Api-Key` header.

### Getting your Organization ID

1. Log in to your SynkVault account (you must be an **Admin** or **Owner** of the organization)
2. If you have multiple organizations, switch to the desired one
3. In the left menu, go to **Settings > Organizations**
4. Under the **General** tab, copy your **Organization ID**

### Getting an API Key

1. Click your **user avatar** in the top right and select **My Account**
2. In the left menu, select **API Keys**
3. Create a new key, copy it immediately, and store it somewhere safe — **this is the only time you will see it**

## Configuration

```ts
interface SynkVaultConfig {
  baseUrl:  string   // Partners API base URL (required)
  orgId:    string   // Your organization UUID (required)
  apiKey:   string   // API key (required)
  timeout?: number   // Request timeout in ms (default: 30000)
}
```

## Resources

### `client.orgs`

#### `list()`

List all organizations accessible to the authenticated credential. Does not require `org_id`.

```ts
const { data } = await client.orgs.list()
// [{ id: '7f1b...', name: 'Acme Corp', role: 'org_admin' }, ...]
```

#### `listUsers(orgId?)`

List all users in an organization. Requires `org_owner` or `org_admin` role.

```ts
const { data } = await client.orgs.listUsers()
const { data } = await client.orgs.listUsers('7f1b...')
```

---

### `client.health`

```ts
const result = await client.health.check()
// { status: 'ok', timestamp: '...', version: '1.0.0' }
```

---

### `client.ontology`

#### `list(params?)`

```ts
const { data, count } = await client.ontology.list({ include_descriptions: true })
```

#### `get(nodeId)`

```ts
const { data } = await client.ontology.get('node-uuid')
```

#### `getChildren(nodeId, params?)`

```ts
const { data } = await client.ontology.getChildren('node-uuid', { depth: 3 })
```

#### `updateDescription(nodeId, body)`

```ts
await client.ontology.updateDescription('node-uuid', { description: 'A person entity' })
```

#### `updatePropertyDescription(nodeId, body)`

```ts
await client.ontology.updatePropertyDescription('node-uuid', {
  property_name: 'age',
  description: 'Age in years',
})
```

---

### `client.knowledge`

#### `listNodes(params)`

```ts
const result = await client.knowledge.listNodes({
  node_name: 'city',
  page: 1,
  page_size: 20,
  nested: true,
})
console.log(result.data, result.pagination, result.inboundRelationships)
```

#### `getNode(params)`

```ts
const result = await client.knowledge.getNode({ record_id: 'city:abc123' })
```

---

### `client.ingest`

#### `ingest.text(body)`

```ts
await client.ingest.text({ content: '# Notes\n\nContent here...' })
```

#### `ingest.json(body)`

```ts
await client.ingest.json({ target_class: 'announcement', data: { title: 'Q2 All-Hands' } })
```

#### `ingest.url(body)`

```ts
await client.ingest.url({ url: 'https://example.com/article' })
```

---

### `client.documents`

#### `upload({ file, dryrun? })`

```ts
const file = new File([buffer], 'report.pdf', { type: 'application/pdf' })
const doc = await client.documents.upload({ file })
```

#### `list(params?)`

```ts
const { data, pagination } = await client.documents.list({ status: 'processed' })
```

#### `get(id, params?)`

```ts
const doc = await client.documents.get('doc-abc', { include: 'extracted' })
```

---

## Error Handling

```ts
import { SynkVaultClient, SynkVaultError } from '@synkvault/sdk'

try {
  await client.ontology.get('nonexistent-id')
} catch (err) {
  if (err instanceof SynkVaultError) {
    console.error(err.statusCode)  // e.g. 404
    console.error(err.message)     // e.g. "Node not found"
    console.error(err.data)        // raw response body
  }
}
```

---

## Integration Testing

```bash
SYNKVAULT_TEST_BASE_URL=http://localhost:3102 \
SYNKVAULT_TEST_ORG_ID=your-org-uuid \
SYNKVAULT_TEST_API_KEY=your-api-key \
pnpm test
```

---

## License

MIT
