# Ontology Resource

The ontology is the schema — the set of entity types and relationships that define your knowledge domain.

## Overview

The ontology defines:
- **Entity Types** (nodes) — Categories of things in your domain (e.g., Company, Person, Product)
- **Properties** — Attributes of each entity type (e.g., Company has `name`, `founded_year`, `revenue`)
- **Relationships** — Connections between entity types (e.g., Person `works_at` Company)

All data in your knowledge graph must conform to the ontology.

---

## API Methods

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

**Response fields**:
- `id` — Unique ontology node identifier
- `name` — Entity type name (e.g., "Company")
- `label` — Display label
- `parentId` — Parent node ID (for hierarchical schemas, null if root)
- `description` — Human-readable description
- `properties` — Object mapping property names to their definitions
- `allowDirectQuery` — Whether this node can be queried directly
- `includeInMapPlotting` — Whether to show in knowledge map visualizations

---

### `client.ontology.get(nodeId)`

Get a single ontology node by ID.

```ts
const result = await client.ontology.get('node-123')
// { data: {
//   id: 'node-123',
//   name: 'Company',
//   label: 'Company',
//   description: 'A legal business entity',
//   properties: { ... }
// } }
```

**Parameters**:
- `nodeId: string` — Ontology node ID

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
- `nodeId: string` — Parent node ID
- `depth?: number` — How many levels deep to fetch (default: 1)

---

## Updating Ontology

### `client.ontology.updateDescription(nodeId, body)`

Update a node's description. Requires `org_admin` or `org_owner` role.

```ts
await client.ontology.updateDescription('node-123', {
  description: 'A business entity registered with a government agency',
})
```

**Parameters**:
- `nodeId: string` — Ontology node ID
- `body.description: string` — New description

---

### `client.ontology.updatePropertyDescription(nodeId, body)`

Update the description of a property within a node. Requires admin privileges.

```ts
await client.ontology.updatePropertyDescription('node-123', {
  property_name: 'founded_year',
  description: 'Year the company was incorporated',
})
```

**Parameters**:
- `nodeId: string` — Ontology node ID
- `body.property_name: string` — Property to update
- `body.description: string` — New description

---

## Use Cases

- **Schema Discovery** — Understand what entity types exist in your domain
- **UI Generation** — Auto-generate forms based on entity properties
- **Validation** — Ensure incoming data matches defined properties
- **Documentation** — Self-documenting knowledge graph structure

---

## Error Handling

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

- [Knowledge Resource](KNOWLEDGE.md) — Query and browse knowledge graph
- [Resources Overview](RESOURCES.md) — All resources
- [Chat Documentation](CHAT.md) — AI agent conversations
