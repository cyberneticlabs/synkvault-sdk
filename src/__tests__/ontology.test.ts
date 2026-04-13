import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient } from '../client.js'
import type { OntologyNode } from '../types.js'

const BASE_CONFIG = {
  baseUrl: 'https://api.example.com',
  orgId: 'org-123',
  apiKey: 'svk_test_key',
}

const NODE: OntologyNode = {
  id: 'node-uuid',
  name: 'city',
  label: 'City',
  parentId: null,
  allowDirectQuery: true,
  includeInMapPlotting: false,
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

describe('OntologyResource', () => {
  afterEach(() => vi.unstubAllGlobals())

  describe('list()', () => {
    beforeEach(() => stubFetch({ data: [NODE], count: 1 }))

    it('calls GET /api/v1/ontology', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.list()
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('/api/v1/ontology')
      expect(url).not.toContain('/ontology/')
    })

    it('sends include_descriptions and include_properties params', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.list({
        include_descriptions: true,
        include_properties: false,
      })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('include_descriptions=true')
      expect(url).toContain('include_properties=false')
    })
  })

  describe('get()', () => {
    beforeEach(() => stubFetch({ data: NODE }))

    it('calls GET /api/v1/ontology/{node_id}', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.get('node-uuid')
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('/api/v1/ontology/node-uuid')
    })

    it('URL-encodes the node ID', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.get('has spaces')
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('has%20spaces')
    })
  })

  describe('getChildren()', () => {
    beforeEach(() => stubFetch({ data: [NODE], count: 1 }))

    it('calls GET /api/v1/ontology/{node_id}/children', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.getChildren('node-uuid')
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('/api/v1/ontology/node-uuid/children')
    })

    it('sends depth param', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.getChildren('node-uuid', { depth: 3 })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('depth=3')
    })
  })

  describe('updateDescription()', () => {
    beforeEach(() => stubFetch({ data: NODE }))

    it('calls PATCH /api/v1/ontology/{node_id}/description', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.updateDescription('node-uuid', { description: 'A city node' })
      const [url, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string, RequestInit]
      expect(url).toContain('/api/v1/ontology/node-uuid/description')
      expect(init.method).toBe('PATCH')
    })

    it('sends description in the request body', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.updateDescription('node-uuid', { description: 'A city node' })
      const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string, RequestInit]
      expect(JSON.parse(init.body as string)).toEqual({ description: 'A city node' })
    })

    it('URL-encodes the node ID', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.updateDescription('has spaces', { description: 'x' })
      const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
      expect(url).toContain('has%20spaces')
    })
  })

  describe('updatePropertyDescription()', () => {
    beforeEach(() => stubFetch({ data: { properties: { color: {} } } }))

    it('calls PATCH /api/v1/ontology/{node_id}/properties/description', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.updatePropertyDescription('node-uuid', {
        property_name: 'color',
        description: 'The color of the node',
      })
      const [url, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string, RequestInit]
      expect(url).toContain('/api/v1/ontology/node-uuid/properties/description')
      expect(init.method).toBe('PATCH')
    })

    it('sends property_name and description in the request body', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ontology.updatePropertyDescription('node-uuid', {
        property_name: 'color',
        description: 'The color of the node',
      })
      const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string, RequestInit]
      expect(JSON.parse(init.body as string)).toEqual({
        property_name: 'color',
        description: 'The color of the node',
      })
    })
  })
})
