import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient } from '../client.js'

const BASE_CONFIG = {
  baseUrl: 'https://api.example.com',
  orgId: 'org-123',
  apiKey: 'svk_test_key',
}

const INGEST_RESPONSE = { success: true, message: 'Ingestion queued' }

function stubFetch() {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      statusText: 'OK',
      text: () => Promise.resolve(JSON.stringify(INGEST_RESPONSE)),
    }),
  )
}

describe('IngestResource', () => {
  beforeEach(() => stubFetch())
  afterEach(() => vi.unstubAllGlobals())

  describe('text()', () => {
    it('calls POST /api/v1/ingest/text', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ingest.text({ content: 'Hello world' })
      const [url, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
        string,
        RequestInit,
      ]
      expect(url).toContain('/api/v1/ingest/text')
      expect(init.method).toBe('POST')
    })

    it('sends content in request body', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ingest.text({ content: '# My doc' })
      const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
        string,
        RequestInit,
      ]
      expect(init.body).toBe(JSON.stringify({ content: '# My doc' }))
    })
  })

  describe('json()', () => {
    it('calls POST /api/v1/ingest/json', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ingest.json({
        target_class: 'announcement',
        data: { title: 'Hello' },
      })
      const [url, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
        string,
        RequestInit,
      ]
      expect(url).toContain('/api/v1/ingest/json')
      expect(init.method).toBe('POST')
    })

    it('sends target_class and data in request body', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      const body = { target_class: 'announcement', data: { title: 'Hello' } }
      await client.ingest.json(body)
      const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
        string,
        RequestInit,
      ]
      expect(init.body).toBe(JSON.stringify(body))
    })
  })

  describe('url()', () => {
    it('calls POST /api/v1/ingest/url', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ingest.url({ url: 'https://example.com/article' })
      const [url, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
        string,
        RequestInit,
      ]
      expect(url).toContain('/api/v1/ingest/url')
      expect(init.method).toBe('POST')
    })

    it('sends url in request body', async () => {
      const client = new SynkVaultClient(BASE_CONFIG)
      await client.ingest.url({ url: 'https://example.com' })
      const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
        string,
        RequestInit,
      ]
      expect(init.body).toBe(JSON.stringify({ url: 'https://example.com' }))
    })
  })
})
