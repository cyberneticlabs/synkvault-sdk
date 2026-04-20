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

### `client.ingest.text(body)`

Ingest unstructured text. The backend extracts entities and relationships automatically.

```ts
await client.ingest.text({
  content: `
    Acme Corp is a software company founded in 2010.
    CEO is Alice Johnson. Headquarters in San Francisco.
  `,
})
```

**Parameters**:
- `content: string` — Text to ingest (up to 500 KB)

**Use case**: Blog posts, documents, chat transcripts, notes, PDF extracted text.

**Example: Bulk ingest articles**:
```ts
const articles = [
  'Apple announced new products...',
  'Microsoft released Q4 earnings...',
  'Google opened new offices...'
]

for (const article of articles) {
  await client.ingest.text({ content: article })
}
```

---

## Structured JSON Ingestion

### `client.ingest.json(body)`

Ingest structured data with a target entity type. Use when data is already parsed/normalized.

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
- `target_class: string` — Ontology entity type (e.g., "Company", "Person")
- `data: object` — Structured data matching the node's properties

**Use case**: CSV exports, database records, form submissions, API responses.

**Example: Ingest CSV data**:
```ts
import { parse } from 'csv-parse/sync'
import fs from 'fs'

const csvContent = fs.readFileSync('companies.csv', 'utf-8')
const records = parse(csvContent, { columns: true })

for (const record of records) {
  await client.ingest.json({
    target_class: 'Company',
    data: {
      name: record.name,
      founded: parseInt(record.year),
      revenue: record.revenue,
    },
  })
}
```

---

## URL Ingestion

### `client.ingest.url(body)`

Fetch and ingest a URL. The backend extracts readable page content.

```ts
await client.ingest.url({
  url: 'https://example.com/news/acme-launches-new-product',
})
```

**Parameters**:
- `url: string` — HTTP(S) URL to fetch and ingest

**Use case**: Web scraping, RSS feeds, live article ingestion, competitor research.

**Example: Monitor news sources**:
```ts
const newsUrls = [
  'https://news.techcrunch.com/article-1',
  'https://news.venturebeat.com/article-2',
  'https://news.forbes.com/article-3',
]

for (const url of newsUrls) {
  await client.ingest.url({ url })
}
```

---

## Document Ingestion

Upload files for text and structured data extraction. Supports PDF, Word, Excel, and plain text files.

### `client.documents.upload({ file, dryrun? })`

Upload a file for processing. The backend extracts text and structured data.

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
- `file: Blob | File` — Document to upload (PDF, Word, Excel, text)
- `dryrun?: boolean` — Validate without processing (default: false)

**Response** (`Document`):
- `id` — Document ID (reference for querying extracted data)
- `original_file_name` — Uploaded filename
- `document_size` — File size in bytes
- `status` — `'pending'` | `'processing'` | `'processed'` | `'failed'`
- `created_on` — ISO timestamp

**Supported file types**:
- PDF (`.pdf`)
- Word (`.docx`, `.doc`)
- Excel (`.xlsx`, `.xls`)
- Plain text (`.txt`)
- CSV (`.csv`)

**Example: Upload from Node.js**:
```ts
import fs from 'fs'

const buffer = fs.readFileSync('report.pdf')
const file = new File([buffer], 'report.pdf', { type: 'application/pdf' })

const doc = await client.documents.upload({ file })
console.log(`Document ${doc.id} queued for processing`)
```

**Example: Upload from browser**:
```html
<input type="file" id="fileInput" />
<script>
const fileInput = document.getElementById('fileInput')
fileInput.addEventListener('change', async (e) => {
  const file = e.target.files[0]
  const doc = await client.documents.upload({ file })
  console.log(`Uploaded: ${doc.original_file_name}`)
})
</script>
```

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
- `status?: string` — Filter by status (`'pending'`, `'processing'`, `'processed'`, `'failed'`)
- `page?: number` — Pagination page (default: 1)
- `page_size?: number` — Items per page (default: 25)

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
//   status: 'processed',
//   extracted_properties: [
//     { key: 'company_name', value: 'Acme Corp' },
//     { key: 'revenue', value: '$1B' },
//     { key: 'employees', value: '5000' },
//     ...
//   ]
// }
```

**Parameters**:
- `include?: string` — `'extracted'` to include extracted data

---

## Document Processing Workflow

Documents go through a processing pipeline:

1. **Upload** → File queued for processing (`status: 'pending'`)
2. **Processing** → Backend extracts text and tables (`status: 'processing'`)
3. **Complete** → Extracted data available (`status: 'processed'`)
4. **Error** → Processing failed (`status: 'failed'`)

**Poll for completion**:
```ts
const doc = await client.documents.upload({ file })

// Poll until processed
let status = doc.status
while (status !== 'processed' && status !== 'failed') {
  await new Promise(resolve => setTimeout(resolve, 2000))
  const updated = await client.documents.get(doc.id)
  status = updated.status
  console.log(`Status: ${status}`)
}

if (status === 'processed') {
  const extracted = await client.documents.get(doc.id, { include: 'extracted' })
  console.log('Extracted:', extracted.extracted_properties)
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

```ts
import { SynkVaultError } from '@synkvault/sdk'

try {
  await client.ingest.json({
    target_class: 'NonexistentType',
    data: { name: 'Test' },
  })
} catch (err) {
  if (err instanceof SynkVaultError) {
    if (err.statusCode === 400) {
      console.error('Invalid entity type or data')
    } else if (err.statusCode === 413) {
      console.error('Content too large')
    } else {
      console.error(err.message)
    }
  }
}
```

---

## See Also

- [Knowledge Resource](KNOWLEDGE.md) — Query ingested data
- [Ontology Resource](ONTOLOGY.md) — Understand entity types
- [Management Resource](MANAGEMENT.md) — Health & organization management
- [Chat Documentation](CHAT.md) — AI conversations over ingested data
