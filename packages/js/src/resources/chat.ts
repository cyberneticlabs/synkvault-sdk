import type { SynkVaultClient } from '../client.js'
import type { ChatEvent, ChatRunParams } from '../types.js'

export class ChatResource {
  constructor(private readonly client: SynkVaultClient) {}

  /** Send a message and receive a ReadableStream of ChatEvents via SSE. */
  run(params: ChatRunParams): Promise<ReadableStream<ChatEvent>> {
    const { message, sessionId, stream = true, userId } = params
    const formData = new FormData()
    formData.append('message', message)
    formData.append('stream', stream ? 'true' : 'false')
    if (sessionId) formData.append('session_id', sessionId)
    if (userId) formData.append('user_id', userId)
    return this.client.streamRequest('/api/v1/chat', formData)
  }
}
