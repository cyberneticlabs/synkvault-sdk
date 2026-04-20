# Ingest Resource

Load data into SynkVault. Multiple ingestion methods support different data formats and extraction strategies.

## Overview

The Ingest resource provides methods to load data into your knowledge graph:
- **Text** — Unstructured content (documents, articles, transcripts)
- **JSON** — Structured records with target entity types
- **URL** — Web content extraction
- **Documents** — Files (PDF, Word, Excel) with text and table extraction

---

## Text Ingestion

### `client.Ingest.TextAsync(body)`

Ingest unstructured text. The backend extracts entities and relationships automatically.

```csharp
await client.Ingest.TextAsync(new
{
    content = @"
        Acme Corp is a software company founded in 2010.
        CEO is Alice Johnson. Headquarters in San Francisco.
    "
});
```

**Parameters**:
- `content` (string) — Text to ingest (up to 500 KB)

**Use case**: Blog posts, documents, chat transcripts, notes, PDF extracted text.

**Example: Bulk ingest articles**:
```csharp
var articles = new[]
{
    "Apple announced new products...",
    "Microsoft released Q4 earnings...",
    "Google opened new offices..."
};

foreach (var article in articles)
{
    await client.Ingest.TextAsync(new { content = article });
}
```

---

## Structured JSON Ingestion

### `client.Ingest.JsonAsync(body)`

Ingest structured data with a target entity type. Use when data is already parsed/normalized.

```csharp
await client.Ingest.JsonAsync(new
{
    target_class = "Company",
    data = new
    {
        name = "Widgets Inc",
        founded = 2015,
        ceo = "Bob Smith",
        revenue = "500M"
    }
});
```

**Parameters**:
- `target_class` (string) — Ontology entity type (e.g., "Company", "Person")
- `data` (object) — Structured data matching the node's properties

**Use case**: CSV exports, database records, form submissions, API responses.

**Example: Ingest CSV data**:
```csharp
using CsvHelper;
using System.Globalization;

using (var reader = new StreamReader("companies.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<dynamic>();

    foreach (var record in records)
    {
        await client.Ingest.JsonAsync(new
        {
            target_class = "Company",
            data = new
            {
                name = record.name,
                founded = int.Parse(record.year),
                revenue = record.revenue
            }
        });
    }
}
```

---

## URL Ingestion

### `client.Ingest.UrlAsync(body)`

Fetch and ingest a URL. The backend extracts readable page content.

```csharp
await client.Ingest.UrlAsync(new
{
    url = "https://example.com/news/acme-launches-new-product"
});
```

**Parameters**:
- `url` (string) — HTTP(S) URL to fetch and ingest

**Use case**: Web scraping, RSS feeds, live article ingestion, competitor research.

**Example: Monitor news sources**:
```csharp
var newsUrls = new[]
{
    "https://news.techcrunch.com/article-1",
    "https://news.venturebeat.com/article-2",
    "https://news.forbes.com/article-3"
};

foreach (var url in newsUrls)
{
    await client.Ingest.UrlAsync(new { url });
}
```

---

## Document Ingestion

Upload files for text and structured data extraction. Supports PDF, Word, Excel, and plain text files.

### `client.Documents.UploadAsync(file, dryrun?)`

Upload a file for processing. The backend extracts text and structured data.

```csharp
var fileBytes = await File.ReadAllBytesAsync("annual-report.pdf");
var doc = await client.Documents.UploadAsync(new
{
    file = fileBytes,
    fileName = "annual-report.pdf"
});

// {
//   Id = "doc-abc-123",
//   OriginalFileName = "annual-report.pdf",
//   DocumentSize = 2500000,
//   Status = "pending",
//   CreatedOn = DateTime.Parse("2026-04-20T12:00:00Z")
// }
```

**Parameters**:
- `file` (byte[]) — Document bytes to upload
- `fileName` (string) — Original filename
- `dryrun` (bool, optional) — Validate without processing (default: false)

**Response** (`Document`):
- `Id` — Document ID (reference for querying extracted data)
- `OriginalFileName` — Uploaded filename
- `DocumentSize` — File size in bytes
- `Status` — `"pending"` | `"processing"` | `"processed"` | `"failed"`
- `CreatedOn` — ISO timestamp

**Supported file types**:
- PDF (`.pdf`)
- Word (`.docx`, `.doc`)
- Excel (`.xlsx`, `.xls`)
- Plain text (`.txt`)
- CSV (`.csv`)

**Example: Upload from file path**:
```csharp
var filePath = "report.pdf";
var fileBytes = await File.ReadAllBytesAsync(filePath);

var doc = await client.Documents.UploadAsync(new
{
    file = fileBytes,
    fileName = Path.GetFileName(filePath)
});

Console.WriteLine($"Document {doc.Id} queued for processing");
```

**Example: Upload with directory scan**:
```csharp
var directory = new DirectoryInfo("./documents");

foreach (var fileInfo in directory.GetFiles("*.pdf"))
{
    var bytes = await File.ReadAllBytesAsync(fileInfo.FullName);

    var doc = await client.Documents.UploadAsync(new
    {
        file = bytes,
        fileName = fileInfo.Name
    });

    Console.WriteLine($"Uploaded: {doc.OriginalFileName}");
}
```

---

### `client.Documents.ListAsync(params?)`

List uploaded documents with optional filtering.

```csharp
var result = await client.Documents.ListAsync(new
{
    status = "processed",
    page = 1,
    page_size = 20
});

// {
//   Data = [ ...documents... ],
//   Pagination = { Page = 1, PageSize = 20, TotalCount = 142, TotalPages = 8 }
// }
```

**Parameters**:
- `status` (string, optional) — Filter by status (`"pending"`, `"processing"`, `"processed"`, `"failed"`)
- `page` (int, optional) — Pagination page (default: 1)
- `page_size` (int, optional) — Items per page (default: 25)

---

### `client.Documents.GetAsync(id, params?)`

Get a specific document and optionally its extracted properties.

```csharp
var doc = await client.Documents.GetAsync("doc-abc-123", new
{
    include = "extracted"
});

// {
//   Id = "doc-abc-123",
//   OriginalFileName = "annual-report.pdf",
//   Status = "processed",
//   ExtractedProperties = [
//     { Key = "company_name", Value = "Acme Corp" },
//     { Key = "revenue", Value = "$1B" },
//     { Key = "employees", Value = "5000" },
//     ...
//   ]
// }
```

**Parameters**:
- `include` (string, optional) — `"extracted"` to include extracted data

---

## Document Processing Workflow

Documents go through a processing pipeline:

1. **Upload** → File queued for processing (`status: "pending"`)
2. **Processing** → Backend extracts text and tables (`status: "processing"`)
3. **Complete** → Extracted data available (`status: "processed"`)
4. **Error** → Processing failed (`status: "failed"`)

**Poll for completion**:
```csharp
var doc = await client.Documents.UploadAsync(new
{
    file = fileBytes,
    fileName = "report.pdf"
});

// Poll until processed
string status = doc.Status;
while (status != "processed" && status != "failed")
{
    await Task.Delay(2000);
    var updated = await client.Documents.GetAsync(doc.Id);
    status = updated.Status;
    Console.WriteLine($"Status: {status}");
}

if (status == "processed")
{
    var extracted = await client.Documents.GetAsync(doc.Id, new { include = "extracted" });
    Console.WriteLine("Extracted properties:");
    foreach (var prop in extracted.ExtractedProperties)
    {
        Console.WriteLine($"  {prop.Key}: {prop.Value}");
    }
}
```

---

## Choosing an Ingestion Method

| Method | Best for | Limits |
|---|---|---|
| **Text** | Unstructured content, articles, transcripts | 500 KB |
| **JSON** | Structured records, CSV/database exports | Depends on record size |
| **URL** | Web content, news feeds, live articles | Content size varies |
| **Documents** | Files (PDF, Word, Excel), forms, reports | File size varies, async |

---

## Error Handling

```csharp
try
{
    await client.Ingest.JsonAsync(new
    {
        target_class = "NonexistentType",
        data = new { name = "Test" }
    });
}
catch (SynkVaultException ex)
{
    if (ex.StatusCode == 400)
    {
        Console.WriteLine("Invalid entity type or data");
    }
    else if (ex.StatusCode == 413)
    {
        Console.WriteLine("Content too large");
    }
    else
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

---

## See Also

- [Knowledge Resource](KNOWLEDGE.md) — Query ingested data
- [Ontology Resource](ONTOLOGY.md) — Understand entity types
- [Management Resource](MANAGEMENT.md) — Health & organization management
- [Chat Documentation](CHAT.md) — AI conversations over ingested data
