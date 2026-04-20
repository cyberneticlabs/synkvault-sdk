# SynkVault.Sdk

Official .NET SDK for the [SynkVault](https://synkvault.com) Partners API.

## Requirements

- .NET 9.0+
- A SynkVault Partner account with an API key

## Installation

```bash
dotnet add package SynkVault.Sdk
```

## Quick Start

```csharp
using SynkVault.Sdk;

var client = new SynkVaultClient(new SynkVaultConfig
{
    BaseUrl = "https://api.synkvault.com",
    OrgId   = "your-org-uuid",
    ApiKey  = "svk_live_...",
});

var health = await client.Health.CheckAsync();
Console.WriteLine(health.Status); // "ok"
```

## Authentication

Authenticate with an **API key** (`X-Api-Key` header) or a **JWT bearer token**. One must be provided.

```csharp
// API key
var config = new SynkVaultConfig
{
    BaseUrl = "https://api.synkvault.com",
    OrgId   = "your-org-uuid",
    ApiKey  = "svk_live_...",
};

// Bearer token
var config = new SynkVaultConfig
{
    BaseUrl = "https://api.synkvault.com",
    OrgId   = "your-org-uuid",
    Token   = "eyJ...",
};
```

## Configuration

```csharp
public sealed class SynkVaultConfig
{
    public required string BaseUrl  { get; init; }  // Partners API base URL
    public required string OrgId    { get; init; }  // Your organization UUID
    public string? ApiKey           { get; init; }  // API key (X-Api-Key)
    public string? Token            { get; init; }  // Bearer token
    public int Timeout              { get; init; } = 30_000; // ms
}
```

## Dependency Injection

Pass an `HttpClient` for use with `IHttpClientFactory` or in tests:

```csharp
// With IHttpClientFactory
services.AddHttpClient<SynkVaultClient>();

// Or inject directly
var client = new SynkVaultClient(config, httpClient);
```

## Resources

### `client.Orgs`

#### `ListAsync()`

List all organizations accessible to the authenticated credential.

```csharp
var response = await client.Orgs.ListAsync();
// response.Data: List<Org> [{ Id, Name, Role }, ...]
```

#### `ListUsersAsync(orgId?)`

```csharp
var response = await client.Orgs.ListUsersAsync();           // uses config OrgId
var response = await client.Orgs.ListUsersAsync("7f1b...");  // explicit org
```

---

### `client.Health`

```csharp
var result = await client.Health.CheckAsync();
// result.Status, result.Timestamp, result.Version
```

No auth required.

---

### `client.Ontology`

#### `ListAsync(params?)`

```csharp
var result = await client.Ontology.ListAsync();
var result = await client.Ontology.ListAsync(new GetOntologyParams
{
    IncludeDescriptions = true,
    IncludeProperties   = true,
});
// result.Data: List<OntologyNode>, result.Count: int
```

#### `GetAsync(nodeId)`

```csharp
var result = await client.Ontology.GetAsync("node-uuid");
// result.Data: OntologyNode
```

#### `GetChildrenAsync(nodeId, params?)`

```csharp
var result = await client.Ontology.GetChildrenAsync("node-uuid",
    new GetOntologyChildrenParams { Depth = 3 });
// result.Data: List<OntologyNode>
```

`Depth` is clamped 1–10 (default: 1) server-side.

#### `UpdateDescriptionAsync(nodeId, body)`

```csharp
var result = await client.Ontology.UpdateDescriptionAsync(
    "node-uuid",
    new UpdateOntologyNodeDescriptionBody { Description = "A person entity" });
```

#### `UpdatePropertyDescriptionAsync(nodeId, body)`

```csharp
await client.Ontology.UpdatePropertyDescriptionAsync(
    "node-uuid",
    new UpdateOntologyPropertyDescriptionBody
    {
        PropertyName = "age",
        Description  = "Age in years",
    });
```

---

### `client.Knowledge`

#### `ListNodesAsync(params)`

```csharp
var result = await client.Knowledge.ListNodesAsync(new GetKnowledgeNodesParams
{
    NodeName  = "city",   // required
    Page      = 1,
    PageSize  = 20,
    Nested    = true,
    StartDate = "2024-01-01",
    EndDate   = "2024-12-31",
});
// result.Data: List<Dictionary<string, JsonElement>>
// result.Pagination: PaginationMeta
// result.InboundRelationships: List<InboundRelationship>?
```

#### `GetNodeAsync(params)`

```csharp
var result = await client.Knowledge.GetNodeAsync(new GetKnowledgeNodeParams
{
    RecordId = "city:abc123",  // required
    Nested   = true,
});
// result.Data: Dictionary<string, JsonElement>
```

---

### `client.Ingest`

#### `TextAsync(body)`

```csharp
await client.Ingest.TextAsync(new IngestTextBody
{
    Content = "# Meeting Notes\n\nDecided to expand into APAC...",
});
```

Max 500 KB.

#### `JsonAsync(body)`

```csharp
await client.Ingest.JsonAsync(new IngestJsonBody
{
    TargetClass = "announcement",
    Data        = new { Title = "Q2 All-Hands", Date = "2026-06-01" },
});
```

`Data` accepts any POCO, anonymous type, or `Dictionary<string, object?>`.

#### `UrlAsync(body)`

```csharp
await client.Ingest.UrlAsync(new IngestUrlBody
{
    Url = "https://example.com/press-release",
});
```

---

### `client.Documents`

#### `UploadAsync(params)`

> **Note:** Unlike the TypeScript SDK which accepts `Blob`/`File`, the .NET SDK accepts a `Stream` + `FileName`.

```csharp
await using var stream = File.OpenRead("report.pdf");

var doc = await client.Documents.UploadAsync(new UploadDocumentParams
{
    Content     = stream,
    FileName    = "report.pdf",
    ContentType = "application/pdf",
});
// doc.Id, doc.Status, doc.OriginalFileName, ...

// Dry run
await client.Documents.UploadAsync(new UploadDocumentParams
{
    Content  = stream,
    FileName = "report.pdf",
    Dryrun   = true,
});
```

#### `ListAsync(params?)`

```csharp
var result = await client.Documents.ListAsync();
var result = await client.Documents.ListAsync(new ListDocumentsParams
{
    Status   = "processed",
    Page     = 1,
    PageSize = 20,
    Search   = "annual report",
    AllUsers = true,   // org admins/owners only
});
// result.Data: List<Document>, result.Pagination: PaginationMeta
```

#### `GetAsync(id, params?)`

```csharp
var doc = await client.Documents.GetAsync("doc-abc");
var doc = await client.Documents.GetAsync("doc-abc", new GetDocumentParams { Include = "extracted" });
// doc.ExtractedProperties: Dictionary<string, JsonElement>?
```

---

## Error Handling

All API errors throw `SynkVaultException`:

```csharp
try
{
    await client.Ontology.GetAsync("nonexistent-id");
}
catch (SynkVaultException ex)
{
    Console.WriteLine(ex.StatusCode);    // e.g. 404
    Console.WriteLine(ex.Message);       // e.g. "Node not found"
    Console.WriteLine(ex.ResponseData);  // raw parsed response body (object?)
}
```

> **Note:** The .NET SDK uses `ResponseData` (not `Data`) to avoid shadowing `Exception.Data`.

| Status | Meaning |
|--------|---------|
| 400    | Bad request |
| 401    | Unauthorized |
| 403    | Forbidden |
| 404    | Not found |
| 500    | Internal server error |

---

## CancellationToken

All async methods accept an optional `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var result = await client.Health.CheckAsync(cts.Token);
```

---

## Integration Testing

```bash
SYNKVAULT_TEST_BASE_URL=http://localhost:3102 \
SYNKVAULT_TEST_ORG_ID=your-org-uuid \
SYNKVAULT_TEST_API_KEY=your-api-key \
dotnet test --filter "Integration"
```

---

## License

MIT
