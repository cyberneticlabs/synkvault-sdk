import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient } from '../client.js'

const BASE_CONFIG = {
  baseUrl: 'https://api.example.com',
  orgId: 'org-123',
  apiKey: 'svk_test_key',
}

function stubFetch(body: unknown) {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      statusText: 'OK',
      text: () => Promise.resolve(JSON.stringify(body)),
    }),
  )
}

describe('KnowledgeResource', () => {
  afterEach(() => vi.unstubAllGlobals())

  describe('listNodes()', () => {
    beforeEach(() =>
      stubFetch({
        success: true,
        data: [],
        pagination: { page: 1, page_size: 10, total_count: 0, total_pages: 0 },
      }),
    )

    it('calls GET /api/v1/knowledge/nodes', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.knowledge.listNodes({ node_name: 'city' })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('/api/v1/knowledge/nodes')
    })

    it('sends node_name param', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.knowledge.listNodes({ node_name: 'city' })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('node_name=city')
    })

    it('sends pagination params', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.knowledge.listNodes({ node_name: 'city', page: 2, page_size: 25 })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('page=2')
      expect(url).toContain('page_size=25')
    })

    it('sends nested and node_id params', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.knowledge.listNodes({
        node_name: 'city',
        nested: true,
        node_id: 'uuid-abc',
      })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('nested=true')
      expect(url).toContain('node_id=uuid-abc')
    })

    it('sends date filters', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.knowledge.listNodes({
        node_name: 'city',
        start_date: '2024-01-01T00:00:00Z',
        end_date: '2024-12-31T23:59:59Z',
      })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('start_date=')
      expect(url).toContain('end_date=')
    })

    it('returns inboundRelationships when present in response', async () => {
      const inboundRelationships = [
        { sourceNode: 'restaurant', propName: 'city', key: 'restaurant_via_city' },
      ]
      stubFetch({
        success: true,
        data: [{ id: 'city:london', name: 'London' }],
        pagination: { page: 1, page_size: 10, total_count: 1, total_pages: 1 },
        edgeProperties: [],
        inboundRelationships,
      })
      const client = new SynkVaultClient(BASE_CONFIG)
      const result = await client.knowledge.listNodes({ node_name: 'city', nested: true })
      expect(result.inboundRelationships).toEqual(inboundRelationships)
    })
  })

  describe('getNode()', () => {
    beforeEach(() => stubFetch({ success: true, data: { id: 'city:abc' } }))

    it('calls GET /api/v1/knowledge/node', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.knowledge.getNode({ record_id: 'city:abc' })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('/api/v1/knowledge/node')
    })

    it('sends record_id param', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.knowledge.getNode({ record_id: 'city:abc' })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('record_id=city%3Aabc')
    })

    it('returns inboundRelationships when present in single-node response', async () => {
      const inboundRelationships = [
        { sourceNode: 'restaurant', propName: 'city', key: 'restaurant_via_city' },
      ]
      stubFetch({
        success: true,
        data: { id: 'city:abc', name: 'London', restaurant_via_city: [] },
        edgeProperties: [],
        inboundRelationships,
      })
      const client = new SynkVaultClient(BASE_CONFIG)
      const result = await client.knowledge.getNode({ record_id: 'city:abc', nested: true })
      expect(result.inboundRelationships).toEqual(inboundRelationships)
    })
  })
})
