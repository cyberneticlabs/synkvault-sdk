import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient } from '../client.js'

const BASE_CONFIG = {
  baseUrl: 'https://api.example.com',
  orgId: 'org-123',
  apiKey: 'svk_test_key',
}

const HEALTH_RESPONSE = {
  status: 'ok',
  timestamp: '2026-01-01T00:00:00.000Z',
  version: '1.0.0',
}

describe('HealthResource', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        statusText: 'OK',
        text: () => Promise.resolve(JSON.stringify(HEALTH_RESPONSE)),
      }),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('calls GET /api/v1/health', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.health.check()
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('/api/v1/health')
  })

  it('does not append org_id', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.health.check()
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).not.toContain('org_id')
  })

  it('does not send auth headers', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.health.check()
    const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
      string,
      RequestInit,
    ]
    const headers = init.headers as Record<string, string>
    expect(headers['X-Api-Key']).toBeUndefined()
    expect(headers['Authorization']).toBeUndefined()
  })

  it('returns the health payload', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    const result = await client.health.check()
    expect(result).toEqual(HEALTH_RESPONSE)
  })
})
