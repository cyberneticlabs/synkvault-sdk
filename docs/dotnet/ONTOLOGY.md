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

### `client.Ontology.ListAsync(params?)`

List all ontology nodes (entity types).

```csharp
var result = await client.Ontology.ListAsync(new
{
    include_descriptions = true,
    page = 1,
    page_size = 50
});

// result.Data contains:
// [
//   {
//     Id = "node-123",
//     Name = "Company",
//     Label = "Company",
//     ParentId = null,
//     Description = "A legal business entity",
//     Properties = { ... },
//     AllowDirectQuery = true,
//     IncludeInMapPlotting = true
//   },
//   ...
// ]
```

**Parameters**:
- `include_descriptions` (bool, optional) — Include node descriptions
- `page` (int, optional) — Pagination page number (default: 1)
- `page_size` (int, optional) — Items per page (default: 25)

**Response fields**:
- `Id` — Unique ontology node identifier
- `Name` — Entity type name (e.g., "Company")
- `Label` — Display label
- `ParentId` — Parent node ID (for hierarchical schemas, null if root)
- `Description` — Human-readable description
- `Properties` — Dictionary mapping property names to their definitions
- `AllowDirectQuery` — Whether this node can be queried directly
- `IncludeInMapPlotting` — Whether to show in knowledge map visualizations

---

### `client.Ontology.GetAsync(nodeId)`

Get a single ontology node by ID.

```csharp
var result = await client.Ontology.GetAsync("node-123");

// result.Data:
// {
//   Id = "node-123",
//   Name = "Company",
//   Label = "Company",
//   Description = "A legal business entity",
//   Properties = { ... }
// }
```

**Parameters**:
- `nodeId` (string) — Ontology node ID

---

### `client.Ontology.GetChildrenAsync(nodeId, params?)`

Get child nodes of a parent node (for hierarchical ontologies).

```csharp
var result = await client.Ontology.GetChildrenAsync("node-123", new
{
    depth = 2
});

// result.Data contains child nodes
```

**Parameters**:
- `nodeId` (string) — Parent node ID
- `depth` (int, optional) — How many levels deep to fetch (default: 1)

---

## Updating Ontology

### `client.Ontology.UpdateDescriptionAsync(nodeId, body)`

Update a node's description. Requires `org_admin` or `org_owner` role.

```csharp
await client.Ontology.UpdateDescriptionAsync("node-123", new
{
    description = "A business entity registered with a government agency"
});
```

**Parameters**:
- `nodeId` (string) — Ontology node ID
- `body.description` (string) — New description

---

### `client.Ontology.UpdatePropertyDescriptionAsync(nodeId, body)`

Update the description of a property within a node. Requires admin privileges.

```csharp
await client.Ontology.UpdatePropertyDescriptionAsync("node-123", new
{
    property_name = "founded_year",
    description = "Year the company was incorporated"
});
```

**Parameters**:
- `nodeId` (string) — Ontology node ID
- `body.property_name` (string) — Property to update
- `body.description` (string) — New description

---

## Use Cases

- **Schema Discovery** — Understand what entity types exist in your domain
- **UI Generation** — Auto-generate forms based on entity properties
- **Validation** — Ensure incoming data matches defined properties
- **Documentation** — Self-documenting knowledge graph structure

---

## Error Handling

```csharp
try
{
    var result = await client.Ontology.GetAsync("nonexistent-id");
}
catch (SynkVaultException ex)
{
    if (ex.StatusCode == 404)
    {
        Console.WriteLine("Ontology node not found");
    }
    else
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

---

## See Also

- [Knowledge Resource](KNOWLEDGE.md) — Query and browse knowledge graph
- [Resources Overview](RESOURCES.md) — All resources
- [Chat Documentation](CHAT.md) — AI agent conversations
