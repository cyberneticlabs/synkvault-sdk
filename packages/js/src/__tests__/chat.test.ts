import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { SynkVaultClient, SynkVaultError } from '../index.js'
import type { ChatEvent } from '../index.js'

const BASE_CONFIG = {
  baseUrl: 'https://api.example.com',
  orgId: 'org-123',
  apiKey: 'svk_test_key',
}

function makeSSEStream(events: ChatEvent[]): ReadableStream<Uint8Array> {
  const encoder = new TextEncoder()
  const lines = events.map((e) => `data: ${JSON.stringify(e)}\n\n`).join('')
  return new ReadableStream({
    start(controller) {
      controller.enqueue(encoder.encode(lines))
      controller.close()
    },
  })
}

function stubFetchStream(events: ChatEvent[]) {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      statusText: 'OK',
      body: makeSSEStream(events),
    }),
  )
}

function stubFetchError(status: number, message: string) {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue({
      ok: false,
      status,
      statusText: message,
      body: null,
      text: () => Promise.resolve(JSON.stringify({ message })),
    }),
  )
}

describe('ChatResource', () => {
  afterEach(() => vi.unstubAllGlobals())

  describe('run()', () => {
    it('yields parsed ChatEvent objects from SSE stream', async () => {
      const events: ChatEvent[] = [
        { event: 'RunResponseContentDelta', content: 'Hello ' },
        { event: 'RunResponseContentDelta', content: 'world' },
        { event: 'TeamRunCompleted', content: 'Hello world' },
      ]
      stubFetchStream(events)
      const client = new SynkVaultClient(BASE_CONFIG)
      const stream = await client.chat.run({ message: 'Hi' })
      const reader = stream.getReader()
      const received: ChatEvent[] = []
      while (true) {
        const { done, value } = await reader.read()
        if (done) break
        received.push(value)
      }
      expect(received).toEqual(events)
    })

    it('surfaces session_id from events', async () => {
      const events: ChatEvent[] = [
        { event: 'RunResponseContentDelta', content: 'Hi', session_id: 'sess-abc' },
      ]
      stubFetchStream(events)
      const client = new SynkVaultClient(BASE_CONFIG)
      const stream = await client.chat.run({ message: 'Hello' })
      const reader = stream.getReader()
      const { value } = await reader.read()
      expect(value?.session_id).toBe('sess-abc')
    })

    it('throws SynkVaultError with statusCode 401 on unauthorized', async () => {
      stubFetchError(401, 'Unauthorized')
      const client = new SynkVaultClient(BASE_CONFIG)
      await expect(client.chat.run({ message: 'Hi' })).rejects.toThrow(SynkVaultError)
      await expect(client.chat.run({ message: 'Hi' })).rejects.toMatchObject({
        statusCode: 401,
      })
    })
  })
})
