import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient } from '../client.js'

const BASE_CONFIG = {
  baseUrl: 'https://api.example.com',
  orgId: 'org-123',
  apiKey: 'svk_test_key',
}

const MEDIA_PATH = 'org-123/doc-456/images/photo.jpg'
const MEDIA_BLOB = new Blob([new Uint8Array([0xff, 0xd8, 0xff, 0xe0])], { type: 'image/jpeg' })

describe('MediaResource', () => {
  beforeEach(() => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        statusText: 'OK',
        blob: () => Promise.resolve(MEDIA_BLOB),
        text: () => Promise.resolve(''),
      }),
    )
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('calls GET /api/media/{path}', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.media.getFile(MEDIA_PATH)
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).toContain(`/api/media/${MEDIA_PATH}`)
  })

  it('does not append org_id', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.media.getFile(MEDIA_PATH)
    const [url] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [string]
    expect(url).not.toContain('org_id')
  })

  it('does not send auth headers', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    await client.media.getFile(MEDIA_PATH)
    const [, init] = (fetch as ReturnType<typeof vi.fn>).mock.calls[0] as [
      string,
      RequestInit,
    ]
    const headers = (init.headers ?? {}) as Record<string, string>
    expect(headers['X-Api-Key']).toBeUndefined()
    expect(headers['Authorization']).toBeUndefined()
  })

  it('returns the media Blob', async () => {
    const client = new SynkVaultClient(BASE_CONFIG)
    const result = await client.media.getFile(MEDIA_PATH)
    expect(result).toBeInstanceOf(Blob)
    expect(result.type).toBe('image/jpeg')
  })

  it('throws SynkVaultError when the file is not found', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 404,
        statusText: 'Not Found',
        text: () => Promise.resolve(JSON.stringify({ message: 'Media file not found' })),
      }),
    )
    const client = new SynkVaultClient(BASE_CONFIG)
    await expect(client.media.getFile('bad/path')).rejects.toMatchObject({
      statusCode: 404,
      message: 'Media file not found',
    })
  })
})
