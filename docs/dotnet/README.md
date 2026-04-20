# .NET SDK

Official .NET SDK for the SynkVault Partners API.

## Installation

NuGet package coming soon.

```bash
dotnet add package SynkVault.Sdk
```

**Requirements**: .NET 6+

## Quick Start

### 1. Initialize Client

```csharp
using SynkVault.Sdk;

var client = new SynkVaultClient(new SynkVaultClientOptions
{
    BaseUrl = "https://api.synkvault.com",
    OrgId = "your-org-uuid",
    ApiKey = "svk_live_...",
});
```

### 2. Use a Resource

```csharp
// Check API health
var health = await client.Health.CheckAsync();

// List companies
var result = await client.Knowledge.ListNodesAsync(
    new { node_name = "Company" });

// Chat with AI agent (streaming)
var stream = await client.Chat.RunAsync(
    new { message = "Summarize our data" });

await foreach (var @event in stream)
{
    Console.WriteLine(@event.Content);
}
```

## Documentation

- **[Management Resource](MANAGEMENT.md)** — Health checks & organization management
- **[Ontology Resource](ONTOLOGY.md)** — Schema definitions and entity types
- **[Knowledge Resource](KNOWLEDGE.md)** — Query and browse knowledge graphs
- **[Ingest Resource](INGEST.md)** — Load data (text, JSON, URLs, documents)
- **[Chat Feature](CHAT.md)** — Streaming conversations, events, session management

## Error Handling

All errors are thrown as `SynkVaultException`:

```csharp
try
{
    await client.Knowledge.ListNodesAsync(new { node_name = "Company" });
}
catch (SynkVaultException ex)
{
    Console.WriteLine($"{ex.StatusCode}: {ex.Message}");
    Console.WriteLine(ex.Data);
}
```

## Support

- **API Reference**: [OpenAPI Spec](https://synkvault-web-dev-partners-yqmxkygnlq-nw.a.run.app/api/v1/docs/openapi.json)
- **License**: MIT
