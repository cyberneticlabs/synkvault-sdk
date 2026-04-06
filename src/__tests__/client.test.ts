import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient } from '../client.js'
import { SynkVaultError } from '../errors.js'

const BASE_CONFIG = {
  baseUrl: 'https://api.example.com',
  orgId: 'org-123',
  apiKey: 'svk_test_key',
}

function mockFetch(status: number, body: unknown) {
  return vi.fn().mockResolvedValue({
    ok: status >= 200 && status < 300,
    status,
    statusText: status === 200 ? 'OK' : 'Error',
    text: () => Promise.resolve(JSON.stringify(body)),
  })
}

describe('SynkVaultClient — constructor', () => {
  it('throws when baseUrl is missing', () => {
    expect(
      () => new SynkVaultClient({ baseUrl: '', orgId: 'x', apiKey: 'k' }),
    ).toThrow('baseUrl is required')
  })

  it('throws when orgId is missing', () => {
    expect(
      () =>
        new SynkVaultClient({
          baseUrl: 'https://api.example.com',
          orgId: '',
          apiKey: 'k',
        }),
    ).toThrow('orgId is required')
  })

  it('throws when neither apiKey nor token is provided', () => {
    expect(
      () =>
        new SynkVaultClient({
          baseUrl: 'https://api.example.com',
          orgId: 'org-123',
        }),
    ).toThrow('either apiKey or token must be provided')
  })

  it('constructs successfully with apiKey', () => {
    expect(() => new SynkVaultClient(BASE_CONFIG)).not.toThrow()
  })

  it('constructs successfully with token', () => {
    expect(
      () =>
        new SynkVaultClient({
          baseUrl: 'https://api.example.com',
          orgId: 'org-123',
          token: 'jwt.token.here',
        }),
    ).not.toThrow()
  })
})

describe('SynkVaultClient — request()', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', mockFetch(200, { status: 'ok' }))
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('attaches X-Api-Key header when apiKey is set', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.request('GET', '/api/v1/health', { skipOrgId: true, skipAuth: false })
    const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
      string,
      RequestInit,
    ]
    expect((init.headers as Record<string, string>)['X-Api-Key']).toBe(
      'svk_test_key',
    )
  })

  it('attaches Authorization Bearer header when token is set', async () => {
    vi.stubGlobal('fetch', mockFetch(200, { status: 'ok' }))
    const client = new SynkVaultClient({
      baseUrl: 'https://api.example.com',
      orgId: 'org-123',
      token: 'my.jwt.token',
    })
    await client.request('GET', '/api/v1/health', { skipOrgId: true, skipAuth: false })
    const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
      string,
      RequestInit,
    ]
    expect((init.headers as Record<string, string>)['Authorization']).toBe(
      'Bearer my.jwt.token',
    )
  })

  it('appends org_id query param by default', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.request('GET', '/api/v1/ontology')
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('org_id=org-123')
  })

  it('skips org_id when skipOrgId is true', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.request('GET', '/api/v1/health', { skipOrgId: true })
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).not.toContain('org_id')
  })

  it('skips auth headers when skipAuth is true', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.request('GET', '/api/v1/health', {
      skipOrgId: true,
      skipAuth: true,
    })
    const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
      string,
      RequestInit,
    ]
    expect((init.headers as Record<string, string>)['X-Api-Key']).toBeUndefined()
    expect(
      (init.headers as Record<string, string>)['Authorization'],
    ).toBeUndefined()
  })

  it.each([401, 403, 404, 500])(
    'throws SynkVaultError on %i response',
    async (status) => {
      vi.stubGlobal(
        'fetch',
        mockFetch(status, { message: 'Something went wrong' }),
      )
      const client = new SynkVaultClient(BASE_CONFIG)
      await expect(client.request('GET', '/api/v1/ontology')).rejects.toBeInstanceOf(
        SynkVaultError,
      )
    },
  )

  it('sets statusCode on SynkVaultError', async () => {
    vi.stubGlobal('fetch', mockFetch(404, { message: 'Node not found' }))
    const client = new SynkVaultClient(BASE_CONFIG)
    try {
      await client.request('GET', '/api/v1/ontology/bad-id')
    } catch (err) {
      expect(err).toBeInstanceOf(SynkVaultError)
      expect((err as SynkVaultError).statusCode).toBe(404)
    }
  })

  it('aborts after timeout', async () => {
    vi.useFakeTimers()
    vi.stubGlobal(
      'fetch',
      vi.fn().mockImplementation(
        (_url: string, init: RequestInit) =>
          new Promise<Response>((_resolve, reject) => {
            init.signal?.addEventListener('abort', () =>
              reject(new DOMException('Aborted', 'AbortError')),
            )
          }),
      ),
    )
    const client = new SynkVaultClient({ ...BASE_CONFIG, timeout: 100 })
    const promise = client.request('GET', '/api/v1/ontology')
    vi.advanceTimersByTime(200)
    await expect(promise).rejects.toThrow()
    vi.useRealTimers()
  })

  it('serializes extra query params', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.request('GET', '/api/v1/knowledge/nodes', {
      params: { node_name: 'city', page: 2, page_size: 50 },
    })
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain('node_name=city')
    expect(url).toContain('page=2')
    expect(url).toContain('page_size=50')
  })

  it('JSON-encodes the request body for POST', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    const body = { content: 'Hello world' }
    await client.request('POST', '/api/v1/ingest/text', { body })
    const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
      string,
      RequestInit,
    ]
    expect(init.body).toBe(JSON.stringify(body))
  })
})
