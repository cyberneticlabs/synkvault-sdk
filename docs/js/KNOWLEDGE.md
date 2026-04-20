# Knowledge Resource

Query and browse the knowledge graph — the actual data that conforms to the ontology schema.

## Overview

The knowledge graph stores all your data as **records** (instances of ontology entity types). Each record has:
- **record_id** — Unique identifier (format: `NodeType:key`)
- **node_name** — The entity type it belongs to
- **properties** — The data fields
- **relationships** — Links to other records

---

## API Methods

### `client.knowledge.listNodes(params)`

Search for all records of a specific node type with pagination and optional relationship loading.

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
- `page?: number` — Pagination page number (default: 1)
- `page_size?: number` — Items per page (default: 25, max: 500)
- `nested?: boolean` — Include related records (default: false)

**Response fields**:
- `data[]` — Array of records matching the query
- `pagination` — Paging metadata including total count and pages
- `inboundRelationships[]` — Reverse relationships (entities pointing *to* the queried type)

**Example: Iterate all companies**:
```ts
let page = 1
let hasMore = true

while (hasMore) {
  const result = await client.knowledge.listNodes({
    node_name: 'Company',
    page,
    page_size: 50,
  })

  result.data.forEach(company => {
    console.log(company.properties.name)
  })

  hasMore = page < result.pagination.total_pages
  page++
}
```

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
//   properties: { name: 'Acme Corp', founded_year: 2010, ... },
//   inboundRelationships: [
//     { sourceNode: 'Person', propName: 'ceo', key: 'Person:ceo-123' }
//   ]
// }}
```

**Parameters**:
- `record_id: string` — Required. Full record ID (format: `NodeName:key`)

**Response fields**:
- `record_id` — The record's unique identifier
- `node_name` — The entity type
- `properties` — All data fields for this record
- `inboundRelationships` — All other records that link to this one

---

## Use Cases

### Discovery & Exploration
```ts
// Find all companies
const companies = await client.knowledge.listNodes({ node_name: 'Company' })

// Get details about one company
const company = await client.knowledge.getNode({
  record_id: companies.data[0].record_id
})
```

### Building UIs
```ts
// Load paginated list for a data table
const page1 = await client.knowledge.listNodes({
  node_name: 'Person',
  page: 1,
  page_size: 25,
})
```

### Graph Navigation
```ts
// Get a record with all its inbound relationships
const person = await client.knowledge.getNode({
  record_id: 'Person:john-doe',
})

// Use the inboundRelationships to navigate the graph
person.inboundRelationships.forEach(rel => {
  console.log(`${rel.sourceNode} ${rel.propName} this person`)
})
```

---

## Pagination

All list responses include pagination metadata:

```ts
{
  pagination: {
    page: 1,              // Current page (1-indexed)
    page_size: 20,        // Items returned
    total_count: 142,     // Total records matching query
    total_pages: 8        // Total pages available
  }
}
```

Use `total_pages` to determine when to stop paginating.

---

## Error Handling

```ts
import { SynkVaultError } from '@synkvault/sdk'

try {
  await client.knowledge.getNode({ record_id: 'Company:nonexistent' })
} catch (err) {
  if (err instanceof SynkVaultError) {
    if (err.statusCode === 404) {
      console.log('Record not found')
    } else {
      console.error(err.message)
    }
  }
}
```

---

## See Also

- [Ontology Resource](ONTOLOGY.md) — Schema definitions for your domain
- [Ingest Resource](RESOURCES.md#ingest) — Load data into the knowledge graph
- [Resources Overview](RESOURCES.md) — All resources
- [Chat Documentation](CHAT.md) — AI agent conversations over knowledge
