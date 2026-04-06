/**
 * Developer use-case scenarios.
 *
 * Each describe block represents a realistic workflow a developer would follow
 * when integrating the SynkVault SDK. All network calls are intercepted via
 * fetch stubs — no running server required.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient, SynkVaultError } from '../index.js'

// ── Helpers ───────────────────────────────────────────────────────────────────

function makeFetch(...responses: Array<{ status: number; body: unknown }>) {
  let call = 0
  return vi.fn().mockImplementation(() => {
    const { status, body } = responses[Math.min(call++, responses.length - 1)]
    return Promise.resolve({
      ok: status >= 200 && status < 300,
      status,
      statusText: status === 200 ? 'OK' : 'Error',
      text: () => Promise.resolve(JSON.stringify(body)),
    })
  })
}

// ── Fixtures ──────────────────────────────────────────────────────────────────

const BASE_URL = 'https://api.synkvault.com'
const API_KEY = 'svk_live_abc123'
const ORG_ID = 'org-acme'

const ORGS = [
  { id: 'org-acme', name: 'Acme Corp', role: 'org_admin' },
  { id: 'org-beta', name: 'Beta Ltd', role: 'org_user' },
]

const ONTOLOGY_NODES = [
  {
    id: 'node-city',
    name: 'city',
    label: 'City',
    parentId: null,
    allowDirectQuery: true,
    includeInMapPlotting: true,
  },
  {
    id: 'node-company',
    name: 'company',
    label: 'Company',
    parentId: null,
    allowDirectQuery: true,
    includeInMapPlotting: false,
  },
]

const KNOWLEDGE_NODES = {
  success: true,
  data: [
    { id: 'city:london', name: 'London', country: 'UK' },
    { id: 'city:paris', name: 'Paris', country: 'France' },
  ],
  pagination: { page: 1, page_size: 10, total_count: 2, total_pages: 1 },
}

// ── Scenario 1: Discover orgs then query knowledge ────────────────────────────

describe('Scenario: discover orgs then query knowledge', () => {
  /**
   * A developer authenticates with an API key, lists accessible orgs to find
   * the right org_id, then queries the knowledge base for records.
   */

  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      makeFetch(
        { status: 200, body: { success: true, data: ORGS } },         // orgs.list()
        { status: 200, body: KNOWLEDGE_NODES },                        // knowledge.listNodes()
      ),
    )
  })

  afterEach(() => vi.unstubAllGlobals())

  it('lists orgs without org_id, then queries knowledge with the resolved org_id', async () => {
    const client = new SynkVaultClient({ baseUrl: BASE_URL, orgId: ORG_ID, apiKey: API_KEY })

    // Step 1: discover which orgs are accessible
    const { data: orgs } = await client.orgs.list()
    const target = orgs.find((o) => o.name === 'Acme Corp')
    expect(target?.id).toBe('org-acme')

    // Step 2: query knowledge using the resolved org
    const result = await client.knowledge.listNodes({ node_name: 'city' })
    expect(result.success).toBe(true)
    expect(result.data).toHaveLength(2)
    expect(result.pagination.total_count).toBe(2)

    // Verify org_id was NOT sent on the orgs call but WAS on the knowledge call
    const calls = (fetch as ReturnType<typeof vi.fn>).mock.calls as [string, RequestInit][]
    expect(calls[0][0]).not.toContain('org_id')
    expect(calls[1][0]).toContain('org_id=org-acme')
  })
})

// ── Scenario 2: Browse ontology then ingest structured data ──────────────────

describe('Scenario: browse ontology then ingest structured data', () => {
  /**
   * A developer lists the ontology to find the correct target_class name,
   * then ingests a JSON record mapped to that class.
   */

  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      makeFetch(
        { status: 200, body: { data: ONTOLOGY_NODES, count: 2 } },    // ontology.list()
        { status: 200, body: { success: true, message: 'Queued' } },   // ingest.json()
      ),
    )
  })

  afterEach(() => vi.unstubAllGlobals())

  it('lists ontology to resolve class name, then ingests a record', async () => {
    const client = new SynkVaultClient({ baseUrl: BASE_URL, orgId: ORG_ID, apiKey: API_KEY })

    // Step 1: find the correct ontology class
    const { data: nodes } = await client.ontology.list()
    const cityNode = nodes.find((n) => n.name === 'city')
    expect(cityNode).toBeDefined()

    // Step 2: ingest a record using the resolved class name
    const result = await client.ingest.json({
      target_class: cityNode!.name,
      data: { name: 'Berlin', country: 'Germany', population: 3600000 },
    })
    expect(result.success).toBe(true)

    // Verify the ingest body was JSON-encoded correctly
    const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[1] as [string, RequestInit]
    const body = JSON.parse(init.body as string)
    expect(body.target_class).toBe('city')
    expect(body.data.name).toBe('Berlin')
  })
})

// ── Scenario 3: Ingest from multiple sources ─────────────────────────────────

describe('Scenario: ingest from multiple sources', () => {
  /**
   * A developer ingests content from three different sources in sequence:
   * a URL, a raw text blob, and a structured JSON payload.
   */

  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      makeFetch(
        { status: 200, body: { success: true, message: 'URL queued' } },
        { status: 200, body: { success: true, message: 'Text queued' } },
        { status: 200, body: { success: true, message: 'JSON queued' } },
      ),
    )
  })

  afterEach(() => vi.unstubAllGlobals())

  it('ingests url, text, and json in sequence', async () => {
    const client = new SynkVaultClient({ baseUrl: BASE_URL, orgId: ORG_ID, apiKey: API_KEY })

    const [r1, r2, r3] = await Promise.all([
      client.ingest.url({ url: 'https://example.com/press-release' }),
      client.ingest.text({ content: '# Q2 Report\nRevenue up 20%...' }),
      client.ingest.json({ target_class: 'announcement', data: { title: 'Q2 All-Hands' } }),
    ])

    expect(r1.success).toBe(true)
    expect(r2.success).toBe(true)
    expect(r3.success).toBe(true)

    // All three calls must carry org_id and auth
    const calls = (fetch as ReturnType<typeof vi.fn>).mock.calls as [string, RequestInit][]
    for (const [url, init] of calls) {
      expect(url).toContain('org_id=org-acme')
      expect((init.headers as Record<string, string>)['X-Api-Key']).toBe(API_KEY)
    }
  })
})

// ── Scenario 4: Error handling ────────────────────────────────────────────────

describe('Scenario: error handling', () => {
  /**
   * A developer handles auth errors, missing org access, and not-found
   * responses using the SynkVaultError class.
   */

  afterEach(() => vi.unstubAllGlobals())

  it('throws SynkVaultError with statusCode 401 on bad credentials', async () => {
    vi.stubGlobal('fetch', makeFetch({ status: 401, body: { message: 'Unauthorized' } }))
    const client = new SynkVaultClient({ baseUrl: BASE_URL, orgId: ORG_ID, apiKey: 'bad-key' })

    await expect(client.orgs.list()).rejects.toBeInstanceOf(SynkVaultError)
    await expect(client.orgs.list()).rejects.toMatchObject({ statusCode: 401 })
  })

  it('throws SynkVaultError with statusCode 403 on org access denied', async () => {
    vi.stubGlobal('fetch', makeFetch({
      status: 403,
      body: { message: 'You do not have access to this organization' },
    }))
    const client = new SynkVaultClient({ baseUrl: BASE_URL, orgId: 'org-forbidden', apiKey: API_KEY })

    await expect(client.ontology.list()).rejects.toBeInstanceOf(SynkVaultError)
    await expect(client.ontology.list()).rejects.toMatchObject({ statusCode: 403 })
  })

  it('throws SynkVaultError with statusCode 404 when a node does not exist', async () => {
    vi.stubGlobal('fetch', makeFetch({ status: 404, body: { message: 'Node not found' } }))
    const client = new SynkVaultClient({ baseUrl: BASE_URL, orgId: ORG_ID, apiKey: API_KEY })

    let caught: SynkVaultError | null = null
    try {
      await client.ontology.get('nonexistent-node-id')
    } catch (err) {
      caught = err as SynkVaultError
    }
    expect(caught).toBeInstanceOf(SynkVaultError)
    expect(caught!.statusCode).toBe(404)
    expect(caught!.message).toBe('Node not found')
  })
})

// ── Scenario 5: Paginate through knowledge records ────────────────────────────

describe('Scenario: paginate through all knowledge records', () => {
  /**
   * A developer fetches all pages of records for a given node type by
   * inspecting pagination metadata and issuing sequential requests.
   */

  const page1 = {
    success: true,
    data: [{ id: 'city:london' }],
    pagination: { page: 1, page_size: 1, total_count: 2, total_pages: 2 },
  }
  const page2 = {
    success: true,
    data: [{ id: 'city:paris' }],
    pagination: { page: 2, page_size: 1, total_count: 2, total_pages: 2 },
  }

  beforeEach(() => {
    vi.stubGlobal('fetch', makeFetch(
      { status: 200, body: page1 },
      { status: 200, body: page2 },
    ))
  })

  afterEach(() => vi.unstubAllGlobals())

  it('fetches all pages until total_pages is reached', async () => {
    const client = new SynkVaultClient({ baseUrl: BASE_URL, orgId: ORG_ID, apiKey: API_KEY })

    const allRecords: unknown[] = []
    let page = 1
    let totalPages = 1

    do {
      const result = await client.knowledge.listNodes({ node_name: 'city', page, page_size: 1 })
      allRecords.push(...result.data)
      totalPages = result.pagination.total_pages
      page++
    } while (page <= totalPages)

    expect(allRecords).toHaveLength(2)

    const calls = (fetch as ReturnType<typeof vi.fn>).mock.calls as [string][]
    expect(calls[0][0]).toContain('page=1')
    expect(calls[1][0]).toContain('page=2')
  })
})
