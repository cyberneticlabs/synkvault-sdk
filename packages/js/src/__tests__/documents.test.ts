import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient } from '../client.js'
import { SynkVaultError } from '../errors.js'

const BASE_CONFIG = {
  baseUrl: 'https://api.example.com',
  orgId: 'org-123',
  apiKey: 'svk_test_key',
}

const DOCUMENT_RESPONSE = {
  id: 'doc-abc',
  original_file_name: 'report.pdf',
  document_size: 204800,
  status: 'queued',
  created_on: '2026-04-06T10:00:00.000Z',
}

const DOCUMENTS_RESPONSE = {
  success: true,
  data: [DOCUMENT_RESPONSE],
  pagination: {
    page: 1,
    page_size: 20,
    total_count: 1,
    total_pages: 1,
  },
}

const DOCUMENT_WITH_EXTRACTED = {
  ...DOCUMENT_RESPONSE,
  extracted_properties: { title: 'Annual Report', author: 'Finance Dept' },
}

function stubFetch(body: unknown, status = 200, ok = true) {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue({
      ok,
      status,
      statusText: ok ? 'OK' : 'Error',
      text: () => Promise.resolve(JSON.stringify(body)),
    }),
  )
}

describe('DocumentsResource.list', () => {
  afterEach(() => vi.unstubAllGlobals())

  it('returns paginated documents', async () => {
    stubFetch(DOCUMENTS_RESPONSE)
    const client = new SynkVaultClient(BASE_CONFIG)
    const result = await client.documents.list()
    expect(result).toEqual(DOCUMENTS_RESPONSE)
  })

  it('passes filter params', async () => {
    stubFetch(DOCUMENTS_RESPONSE)
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.documents.list({ status: 'processed', page: 2, page_size: 10 })
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('status=processed')
    expect(url).toContain('page=2')
    expect(url).toContain('page_size=10')
  })

  it('appends org_id query param', async () => {
    stubFetch(DOCUMENTS_RESPONSE)
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.documents.list()
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('org_id=org-123')
  })
})

describe('DocumentsResource.get', () => {
  afterEach(() => vi.unstubAllGlobals())

  it('returns a document by id', async () => {
    stubFetch(DOCUMENT_RESPONSE)
    const client = new SynkVaultClient(BASE_CONFIG)
    const result = await client.documents.get('doc-abc')
    expect(result).toEqual(DOCUMENT_RESPONSE)
  })

  it('includes extracted_properties when include=extracted', async () => {
    stubFetch(DOCUMENT_WITH_EXTRACTED)
    const client = new SynkVaultClient(BASE_CONFIG)
    const result = await client.documents.get('doc-abc', { include: 'extracted' })
    expect(result.extracted_properties).toBeDefined()
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('include=extracted')
  })

  it('encodes document id in path', async () => {
    stubFetch(DOCUMENT_RESPONSE)
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.documents.get('doc/with-slash')
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('doc%2Fwith-slash')
  })

  it('throws SynkVaultError on 404', async () => {
    stubFetch({ message: 'Document not found' }, 404, false)
    const client = new SynkVaultClient(BASE_CONFIG)
    await expect(client.documents.get('nonexistent')).rejects.toBeInstanceOf(SynkVaultError)
    await expect(client.documents.get('nonexistent')).rejects.toMatchObject({
      statusCode: 404,
    })
  })
})

describe('DocumentsResource.upload', () => {
  afterEach(() => vi.unstubAllGlobals())

  it('returns the created document response', async () => {
    stubFetch(DOCUMENT_RESPONSE)
    const client = new SynkVaultClient(BASE_CONFIG)
    const file = new Blob(['pdf content'], { type: 'application/pdf' })
    const result = await client.documents.upload({ file })
    expect(result).toEqual(DOCUMENT_RESPONSE)
  })

  it('posts to /api/v1/ingest/documents', async () => {
    stubFetch(DOCUMENT_RESPONSE)
    const client = new SynkVaultClient(BASE_CONFIG)
    const file = new Blob(['pdf content'], { type: 'application/pdf' })
    await client.documents.upload({ file })
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('/api/v1/ingest/documents')
  })

  it('includes dryrun param when specified', async () => {
    stubFetch(DOCUMENT_RESPONSE)
    const client = new SynkVaultClient(BASE_CONFIG)
    const file = new Blob(['pdf content'], { type: 'application/pdf' })
    await client.documents.upload({ file, dryrun: true })
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('dryrun=true')
  })

  it('throws SynkVaultError on 403', async () => {
    stubFetch({ message: 'Forbidden' }, 403, false)
    const client = new SynkVaultClient(BASE_CONFIG)
    const file = new Blob(['pdf content'])
    await expect(client.documents.upload({ file })).rejects.toBeInstanceOf(SynkVaultError)
    await expect(client.documents.upload({ file })).rejects.toMatchObject({
      statusCode: 403,
    })
  })
})
