# Knowledge Resource

Query and browse the knowledge graph — the actual data that conforms to the ontology schema.

## Overview

The knowledge graph stores all your data as **records** (instances of ontology entity types). Each record has:
- **RecordId** — Unique identifier (format: `NodeType:key`)
- **NodeName** — The entity type it belongs to
- **Properties** — The data fields
- **Relationships** — Links to other records

---

## API Methods

### `client.Knowledge.ListNodesAsync(params)`

Search for all records of a specific node type with pagination and optional relationship loading.

```csharp
var result = await client.Knowledge.ListNodesAsync(new
{
    node_name = "Company",
    page = 1,
    page_size = 20,
    nested = true
});

// result.Data contains:
// [
//   {
//     RecordId = "Company:abc-123",
//     NodeName = "Company",
//     Properties = { name = "Acme Corp", revenue = "1B" },
//     InboundRelationships = [
//       { SourceNode = "Person", PropName = "works_at", Key = "Person:xyz" }
//     ]
//   },
//   ...
// ]
// 
// result.Pagination:
// {
//   Page = 1,
//   PageSize = 20,
//   TotalCount = 142,
//   TotalPages = 8
// }
```

**Parameters**:
- `node_name` (string, required) — Entity type to search (e.g., "Company", "Person")
- `page` (int, optional) — Pagination page number (default: 1)
- `page_size` (int, optional) — Items per page (default: 25, max: 500)
- `nested` (bool, optional) — Include related records (default: false)

**Response fields**:
- `Data` — Array of records matching the query
- `Pagination` — Paging metadata including total count and pages
- `InboundRelationships` — Reverse relationships (entities pointing to the queried type)

**Example: Iterate all companies**:
```csharp
int page = 1;
bool hasMore = true;

while (hasMore)
{
    var result = await client.Knowledge.ListNodesAsync(new
    {
        node_name = "Company",
        page,
        page_size = 50
    });

    foreach (var company in result.Data)
    {
        Console.WriteLine(company.Properties["name"]);
    }

    hasMore = page < result.Pagination.TotalPages;
    page++;
}
```

---

### `client.Knowledge.GetNodeAsync(params)`

Get a single record by its full record ID.

```csharp
var result = await client.Knowledge.GetNodeAsync(new
{
    record_id = "Company:abc-123"
});

// result.Data:
// {
//   RecordId = "Company:abc-123",
//   NodeName = "Company",
//   Properties = { name = "Acme Corp", founded_year = 2010, ... },
//   InboundRelationships = [
//     { SourceNode = "Person", PropName = "ceo", Key = "Person:ceo-123" }
//   ]
// }
```

**Parameters**:
- `record_id` (string, required) — Full record ID (format: `NodeName:key`)

**Response fields**:
- `RecordId` — The record's unique identifier
- `NodeName` — The entity type
- `Properties` — All data fields for this record
- `InboundRelationships` — All other records that link to this one

---

## Use Cases

### Discovery & Exploration
```csharp
// Find all companies
var companies = await client.Knowledge.ListNodesAsync(new
{
    node_name = "Company"
});

// Get details about one company
var company = await client.Knowledge.GetNodeAsync(new
{
    record_id = companies.Data[0].RecordId
});
```

### Building UIs
```csharp
// Load paginated list for a data table
var page1 = await client.Knowledge.ListNodesAsync(new
{
    node_name = "Person",
    page = 1,
    page_size = 25
});
```

### Graph Navigation
```csharp
// Get a record with all its inbound relationships
var person = await client.Knowledge.GetNodeAsync(new
{
    record_id = "Person:john-doe"
});

// Use the InboundRelationships to navigate the graph
foreach (var rel in person.InboundRelationships)
{
    Console.WriteLine($"{rel.SourceNode} {rel.PropName} this person");
}
```

---

## Pagination

All list responses include pagination metadata:

```csharp
var pagination = result.Pagination;
// {
//   Page = 1,           // Current page (1-indexed)
//   PageSize = 20,      // Items returned
//   TotalCount = 142,   // Total records matching query
//   TotalPages = 8      // Total pages available
// }
```

Use `TotalPages` to determine when to stop paginating.

---

## Error Handling

```csharp
try
{
    var result = await client.Knowledge.GetNodeAsync(new
    {
        record_id = "Company:nonexistent"
    });
}
catch (SynkVaultException ex)
{
    if (ex.StatusCode == 404)
    {
        Console.WriteLine("Record not found");
    }
    else
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

---

## See Also

- [Ontology Resource](ONTOLOGY.md) — Schema definitions for your domain
- [Ingest Resource](RESOURCES.md#ingest) — Load data into the knowledge graph
- [Resources Overview](RESOURCES.md) — All resources
- [Chat Documentation](CHAT.md) — AI agent conversations over knowledge
