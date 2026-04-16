---
name: SDK Feature Map
description: Cross-SDK feature mapping — maps every SynkVault SDK feature to its implementation across all language SDKs. Use this as the reference when adding a new SDK or porting a feature.
type: reference
paths:
  - "packages/js/**"
  - "packages/dotnet/**"
---

# SynkVault SDK Feature Map

Maps every feature to its implementation across all SDK packages. When adding a new SDK (Python, Go, Ruby, etc.), use this file to ensure full feature parity. When adding a new feature to any SDK, update this file and port it to all other SDKs.

---

## Package Identity

| Attribute | TypeScript (`packages/js/`) | .NET (`packages/dotnet/`) | Future: Python | Future: Go |
|---|---|---|---|---|
| Package name | `@synkvault/sdk` | `SynkVault.Sdk` | `synkvault-sdk` | `synkvault-sdk` |
| Main class | `SynkVaultClient` | `SynkVaultClient` | `SynkVaultClient` | `SynkVaultClient` |
| Error type | `SynkVaultError` | `SynkVaultException` | `SynkVaultError` | `SynkVaultError` |
| Min runtime | Node.js 18 | .NET 9 | Python 3.11 | Go 1.22 |
| HTTP client | `fetch` (built-in) | `HttpClient` (BCL) | `httpx` / `aiohttp` | `net/http` |
| JSON | `JSON.parse` / `JSON.stringify` | `System.Text.Json` | `json` / `pydantic` | `encoding/json` |
| Async model | Promise / async-await | `Task<T>` / async-await | `asyncio` / `async def` | goroutines / channels |

---

## Client Constructor

| Concern | TypeScript | .NET |
|---|---|---|
| Config type | `SynkVaultConfig` interface | `SynkVaultConfig` sealed class |
| Required: baseUrl | `config.baseUrl` | `config.BaseUrl` |
| Required: orgId | `config.orgId` | `config.OrgId` |
| Auth — API key | `config.apiKey` → `X-Api-Key` | `config.ApiKey` → `X-Api-Key` |
| Auth — Bearer token | `config.token` → `Authorization: Bearer` | `config.Token` → `Authorization: Bearer` |
| Timeout config | `config.timeout` (ms, default 30000) | `config.Timeout` (ms, default 30000) |
| Validation error | throws `Error` | throws `ArgumentException` |
| Injectable HTTP | N/A (uses global `fetch`) | `new SynkVaultClient(config, httpClient)` |

---

## Core Request Method

| Concern | TypeScript (`client.request<T>`) | .NET (`client.RequestAsync<T>`) |
|---|---|---|
| Signature | `request<T>(method, path, options?)` | `RequestAsync<T>(method, path, options?, ct)` |
| HTTP methods | `'GET' \| 'POST' \| 'PATCH'` | `HttpMethod.Get/Post/Patch` |
| org_id injection | Appended as query param unless `skipOrgId: true` | Same; `RequestOptions.SkipOrgId` |
| Auth bypass | `skipAuth: true` | `RequestOptions.SkipAuth` |
| Query params | `options.params: Record<string, unknown>` | `options.Params: Dictionary<string, string?>` |
| Request body | `options.body: unknown` → `JSON.stringify` | `options.Body: object` → `JsonSerializer.Serialize` |
| Timeout | `AbortController` + `setTimeout` | `CancellationTokenSource` linked with caller token |
| Error parsing | Tries `statusMessage`, `message`, `error` fields | Same priority order |
| Error throw | `throw new SynkVaultError(status, msg, data)` | `throw new SynkVaultException(status, msg, data)` |

---

## File Upload Method

| Concern | TypeScript (`client.uploadFile<T>`) | .NET (`client.UploadFileAsync<T>`) |
|---|---|---|
| Signature | `uploadFile<T>(path, formData, params?)` | `UploadFileAsync<T>(path, formData, params?, ct)` |
| Always POST | Yes | Yes |
| org_id | Always appended (no skipOrgId option) | Same |
| Content-Type | Browser sets multipart boundary automatically | `HttpClient` sets it automatically |
| Auth | Same as `request<T>` | Same as `RequestAsync<T>` |

---

## Resources

### Health

| Method | TypeScript | .NET |
|---|---|---|
| Check health | `client.health.check()` | `client.Health.CheckAsync(ct?)` |
| Endpoint | `GET /api/v1/health` | Same |
| skipOrgId | `true` | `true` |
| skipAuth | `true` | `true` |
| Returns | `HealthResponse` | `HealthResponse` |

### Orgs

| Method | TypeScript | .NET |
|---|---|---|
| List orgs | `client.orgs.list()` | `client.Orgs.ListAsync(ct?)` |
| Endpoint | `GET /api/v1/orgs` | Same |
| skipOrgId | `true` | `true` |
| List users | `client.orgs.listUsers(orgId?)` | `client.Orgs.ListUsersAsync(orgId?, ct?)` |
| Endpoint | `GET /api/v1/organizations/{id}/users` | Same |
| Default orgId | Falls back to `config.orgId` | Falls back to `config.OrgId` |

### Ontology

| Method | TypeScript | .NET |
|---|---|---|
| List all nodes | `client.ontology.list(params?)` | `client.Ontology.ListAsync(params?, ct?)` |
| Endpoint | `GET /api/v1/ontology` | Same |
| Get node | `client.ontology.get(nodeId)` | `client.Ontology.GetAsync(nodeId, ct?)` |
| Endpoint | `GET /api/v1/ontology/{nodeId}` (URL-encoded) | Same |
| Get children | `client.ontology.getChildren(nodeId, params?)` | `client.Ontology.GetChildrenAsync(nodeId, params?, ct?)` |
| Endpoint | `GET /api/v1/ontology/{nodeId}/children` | Same |
| Update description | `client.ontology.updateDescription(nodeId, body)` | `client.Ontology.UpdateDescriptionAsync(nodeId, body, ct?)` |
| Endpoint | `PATCH /api/v1/ontology/{nodeId}/description` | Same |
| Update prop desc | `client.ontology.updatePropertyDescription(nodeId, body)` | `client.Ontology.UpdatePropertyDescriptionAsync(nodeId, body, ct?)` |
| Endpoint | `PATCH /api/v1/ontology/{nodeId}/properties/description` | Same |

### Knowledge

| Method | TypeScript | .NET |
|---|---|---|
| List nodes | `client.knowledge.listNodes(params)` | `client.Knowledge.ListNodesAsync(params, ct?)` |
| Endpoint | `GET /api/v1/knowledge/nodes` | Same |
| Required param | `node_name` | `NodeName` |
| Get node | `client.knowledge.getNode(params)` | `client.Knowledge.GetNodeAsync(params, ct?)` |
| Endpoint | `GET /api/v1/knowledge/node` | Same |
| Required param | `record_id` | `RecordId` |

### Ingest

| Method | TypeScript | .NET |
|---|---|---|
| Ingest text | `client.ingest.text(body)` | `client.Ingest.TextAsync(body, ct?)` |
| Endpoint | `POST /api/v1/ingest/text` | Same |
| Max size | 500 KB | 500 KB |
| Ingest JSON | `client.ingest.json(body)` | `client.Ingest.JsonAsync(body, ct?)` |
| Endpoint | `POST /api/v1/ingest/json` | Same |
| Ingest URL | `client.ingest.url(body)` | `client.Ingest.UrlAsync(body, ct?)` |
| Endpoint | `POST /api/v1/ingest/url` | Same |

### Documents

| Method | TypeScript | .NET |
|---|---|---|
| Upload | `client.documents.upload({ file, dryrun? })` | `client.Documents.UploadAsync(params, ct?)` |
| Endpoint | `POST /api/v1/ingest/documents` (multipart) | Same |
| File input | `file: Blob \| File` | `Content: Stream + FileName: string + ContentType: string` |
| List | `client.documents.list(params?)` | `client.Documents.ListAsync(params?, ct?)` |
| Endpoint | `GET /api/v1/ingest/documents` | Same |
| Get | `client.documents.get(id, params?)` | `client.Documents.GetAsync(id, params?, ct?)` |
| Endpoint | `GET /api/v1/ingest/documents/{id}` (URL-encoded) | Same |

---

## Types / Models

### Naming Conventions

| TypeScript | .NET | Notes |
|---|---|---|
| `camelCase` properties | `PascalCase` properties | .NET convention |
| `snake_case` JSON fields | `[JsonPropertyName("snake_case")]` | Explicit attribute per field |
| `Record<string, unknown>` | `Dictionary<string, JsonElement>` | Full JSON fidelity |
| `Record<string, unknown>[]` | `List<Dictionary<string, JsonElement>>` | Knowledge data arrays |
| `Blob \| File` | `Stream` | .NET idiomatic file API |
| optional `?` | nullable `?` | C# nullable reference types |
| Union `string \| null` | `string?` | |

### Shared Model Shapes

| Model | TypeScript fields | .NET fields |
|---|---|---|
| `SynkVaultConfig` | `baseUrl, orgId, apiKey?, token?, timeout?` | `BaseUrl, OrgId, ApiKey?, Token?, Timeout` |
| `HealthResponse` | `status, timestamp, version` | `Status, Timestamp, Version` |
| `Org` | `id, name, role` | `Id, Name, Role` |
| `OrgUser` | `id, name, role` | `Id, Name, Role` |
| `OntologyNode` | `id, name, label, parentId, description?, properties?, unique_properties?, allowDirectQuery, includeInMapPlotting` | Same (PascalCase; `UniqueProperties`) |
| `PaginationMeta` | `page, page_size, total_count, total_pages` | `Page, PageSize, TotalCount, TotalPages` |
| `InboundRelationship` | `sourceNode, propName, key` | `SourceNode, PropName, Key` |
| `Document` | `id, original_file_name, document_size, status, created_on` | `Id, OriginalFileName, DocumentSize, Status, CreatedOn` |
| `DocumentResponse` | extends `Document` + `extracted_properties?` | record inherits `Document` + `ExtractedProperties?` |
| `IngestTextBody` | `content` | `Content` |
| `IngestJsonBody` | `target_class, data: Record<string, unknown>` | `TargetClass, Data: object` |
| `IngestUrlBody` | `url` | `Url` |

---

## Error Types

| Attribute | TypeScript (`SynkVaultError`) | .NET (`SynkVaultException`) |
|---|---|---|
| Base class | `Error` | `Exception` |
| HTTP status | `statusCode: number` | `StatusCode: int` |
| Message | `message: string` | `Message: string` |
| Raw response | `data?: unknown` | `ResponseData?: object` — renamed to avoid `Exception.Data` shadow |
| `instanceof` | `err instanceof SynkVaultError` | `ex is SynkVaultException` |

---

## Testing Patterns

| Aspect | TypeScript (Vitest) | .NET (xUnit) |
|---|---|---|
| Mock HTTP | `vi.stubGlobal('fetch', vi.fn())` | `MockHttpMessageHandler : HttpMessageHandler` |
| Client factory | `new SynkVaultClient(BASE_CONFIG)` | `TestHelper.MakeClient()` → `(client, handler)` |
| Queue responses | `mockFetch.mockResolvedValueOnce(...)` | `handler.EnqueueResponse(status, body)` |
| Assert return values | `expect(result).toEqual(fixture)` | `Assert.Equal(expected, actual)` |
| Assert errors | `await expect(promise).rejects.toThrow(SynkVaultError)` | `await Assert.ThrowsAsync<SynkVaultException>(...)` |
| Skip integration | `describe.skipIf(!BASE_URL)(...)` | `[Fact(Skip = "...")]` |
| Transport tests | `client.test.ts` only | `ClientTests.cs` only |
| Consumer tests | All other `*.test.ts` | All other `*Tests.cs` |
| Test naming | `it('returns the health payload')` | `CheckAsync_Always_ReturnsHealthPayload()` |

---

## Adding a New SDK

When porting to a new language, ensure:

1. **Client constructor** — validates `baseUrl`, `orgId`, and at least one auth credential; throws on missing required fields
2. **`request<T>`** — injects `org_id` query param, applies auth header, respects `skipOrgId`/`skipAuth`, handles timeout, throws the SDK error type on non-2xx
3. **`uploadFile<T>`** — multipart POST; always injects `org_id`; same auth/timeout logic
4. **All 6 resources** — `Health`, `Orgs`, `Ontology`, `Knowledge`, `Ingest`, `Documents` — all endpoints listed above
5. **Error type** — wraps HTTP status, message, and raw response
6. **All model types** — match the JSON field names exactly via serialization attributes or config
7. **Tests** — mirror the TypeScript test coverage: client transport tests + one test file per resource + scenarios + integration skip

Update this file with the new language column once implemented.
