import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient } from '../client.js'

const BASE_CONFIG = {
  baseUrl: 'https://api.example.com',
  orgId: 'org-123',
  apiKey: 'svk_test_key',
}

const ORGS_RESPONSE = {
  success: true,
  data: [
    { id: 'org-123', name: 'Acme Corp', role: 'org_admin' },
    { id: 'org-456', name: 'Beta Ltd', role: 'org_user' },
  ],
}

const ORG_USERS_RESPONSE = {
  success: true,
  data: [
    { id: 'user-1', name: 'Alice', role: 'org_owner' },
    { id: 'user-2', name: 'Bob', role: 'org_admin' },
  ],
}

describe('OrgsResource', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        statusText: 'OK',
        text: () => Promise.resolve(JSON.stringify(ORGS_RESPONSE)),
      }),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('calls GET /api/v1/orgs', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.orgs.list()
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('/api/v1/orgs')
  })

  it('does not append org_id', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.orgs.list()
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).not.toContain('org_id')
  })

  it('sends auth headers', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.orgs.list()
    const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
      string,
      RequestInit,
    ]
    expect((init.headers as Record<string, string>)['X-Api-Key']).toBe('svk_test_key')
  })

  it('returns the orgs payload', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    const result = await client.orgs.list()
    expect(result).toEqual(ORGS_RESPONSE)
  })
})

describe('OrgsResource.listUsers', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        statusText: 'OK',
        text: () => Promise.resolve(JSON.stringify(ORG_USERS_RESPONSE)),
      }),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('calls GET /api/v1/organizations/{org_id}/users using config orgId', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.orgs.listUsers()
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('/api/v1/organizations/org-123/users')
  })

  it('uses provided orgId override', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.orgs.listUsers('org-override')
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('/api/v1/organizations/org-override/users')
  })

  it('does not append org_id as query param', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.orgs.listUsers()
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).not.toContain('org_id=')
  })

  it('returns the org users payload', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    const result = await client.orgs.listUsers()
    expect(result).toEqual(ORG_USERS_RESPONSE)
  })
})
