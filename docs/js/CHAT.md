# Chat Feature Documentation

## Overview

The Chat API enables you to send messages to a SynkVault AI agent and receive streaming responses via Server-Sent Events (SSE). The response is parsed into structured `ChatEvent` objects for easy consumption.

## Quick Start

```ts
import { SynkVaultClient } from '@synkvault/sdk'

const client = new SynkVaultClient({
  baseUrl: 'https://api.synkvault.com',
  orgId: 'your-org-uuid',
  apiKey: 'svk_live_...',
})

const stream = await client.chat.run({
  message: 'What are the key entities in our knowledge base?',
})

for await (const event of stream) {
  if (event.event === 'RunResponseContentDelta') {
    process.stdout.write(event.content || '')
  }
}
```

## API Reference

### `client.chat.run(params)`

Send a message to the AI agent and receive a streaming response.

**Signature**:
```ts
run(params: ChatRunParams): Promise<ReadableStream<ChatEvent>>
```

**Parameters** (`ChatRunParams`):

| Field | Type | Required | Notes |
|---|---|---|---|
| `message` | string | yes | Message to send to the agent |
| `sessionId` | string | no | Continue an existing conversation; omit to start new |
| `userId` | string | no | User ID for context and analytics |
| `stream` | boolean | no | Default: `true`. If `false`, receive final response in one event |

**Returns**: `Promise<ReadableStream<ChatEvent>>` — async iterable stream of events

**Throws**: `SynkVaultError` if the request fails (authentication, network, invalid params)

```ts
try {
  const stream = await client.chat.run({ message: 'Hello' })
} catch (err) {
  if (err instanceof SynkVaultError) {
    console.error(`Error ${err.statusCode}: ${err.message}`)
  }
}
```

## ChatEvent

Each event in the stream is a `ChatEvent` object with:

```ts
interface ChatEvent {
  event: string              // Event type (see below)
  content?: string           // Response content (text)
  reasoning_content?: string // Internal reasoning (if enabled)
  session_id?: string        // Session ID (populated on first event of new session)
  tool?: {
    name: string             // Tool name
    [key: string]: unknown   // Tool-specific fields
  }
  [key: string]: unknown     // Other fields from the backend
}
```

### Event Types

#### `RunResponseContentDelta`
Streaming response token. Appears multiple times during a single message response.

```ts
{
  event: 'RunResponseContentDelta',
  content: 'The top entities are...',
  reasoning_content: 'Analyzing the query...' // optional
}
```

**Use case**: Implement token-by-token streaming UI. Concatenate `content` fields to build the full response.

---

#### `TeamRunCompleted`
Final response. Marks the end of the response for this turn.

```ts
{
  event: 'TeamRunCompleted',
  content: 'The top entities are Company, Person, Location...'
}
```

**Use case**: Know when the response is complete; compare with accumulated `RunResponseContentDelta` tokens.

---

#### `RunToolCallStarted` / `RunToolCallCompleted`
The agent invoked a tool (e.g., queried the knowledge base).

```ts
{
  event: 'RunToolCallStarted',
  tool: {
    name: 'query_knowledge_base',
    // ... tool-specific params
  }
}
{
  event: 'RunToolCallCompleted',
  content: '[{"entity": "Company", "count": 152}]'
}
```

**Use case**: Track agent actions; show "Querying knowledge base..." UI while a tool is in flight.

---

#### `ReasoningStarted` / `ReasoningContentDelta` / `ReasoningCompleted`
Internal reasoning tokens (if the agent's reasoning is exposed). Use like `RunResponseContentDelta` but for reasoning text.

```ts
{
  event: 'ReasoningContentDelta',
  reasoning_content: 'The query mentions top entities...'
}
```

**Use case**: Display reasoning in an expandable section or debug panel.

---

### Session Management

Each chat session has a unique `session_id`. Use it to continue multi-turn conversations.

**Starting a new session** (no `sessionId` provided):
```ts
const stream = await client.chat.run({
  message: 'What is SynkVault?',
})
// First event will carry session_id; save it
let sessionId: string | undefined
for await (const event of stream) {
  if (event.session_id) {
    sessionId = event.session_id
    console.log('Started session:', sessionId)
  }
}
```

**Continuing a session** (provide `sessionId`):
```ts
const stream = await client.chat.run({
  message: 'Tell me more',
  sessionId: 'sess-abc123', // from previous response
})
```

Sessions are managed server-side and may have a TTL; always handle the case where a session expires (the server returns a new `session_id` to start fresh).

## Examples

### Streaming to Console

```ts
const stream = await client.chat.run({
  message: 'Summarize the top 5 company entities',
})

for await (const event of stream) {
  if (event.event === 'RunResponseContentDelta') {
    process.stdout.write(event.content || '')
  }
}
console.log('\n[Response complete]')
```

### Building a Chat Interface

```ts
async function* chatMessages(messages: string[]) {
  for (const message of messages) {
    const stream = await client.chat.run({ message })
    let fullResponse = ''
    for await (const event of stream) {
      if (event.event === 'RunResponseContentDelta') {
        fullResponse += event.content || ''
      }
      if (event.event === 'TeamRunCompleted') {
        yield { role: 'assistant', content: fullResponse }
      }
    }
  }
}
```

### Multi-turn Conversation

```ts
let sessionId: string | undefined

async function chat(userMessage: string) {
  const stream = await client.chat.run({
    message: userMessage,
    sessionId,
  })

  let response = ''
  for await (const event of stream) {
    if (event.session_id && !sessionId) {
      sessionId = event.session_id // Save on first event
    }
    if (event.event === 'RunResponseContentDelta') {
      response += event.content || ''
    }
  }
  return response
}

// Turn 1
await chat('What is SynkVault?')
// Turn 2 (continues with same sessionId)
await chat('How does it handle multitenancy?')
```

### Handling Errors

```ts
import { SynkVaultClient, SynkVaultError } from '@synkvault/sdk'

try {
  const stream = await client.chat.run({ message: 'Hi' })
  for await (const event of stream) {
    console.log(event)
  }
} catch (err) {
  if (err instanceof SynkVaultError) {
    if (err.statusCode === 401) {
      console.error('Invalid API key or authentication token')
    } else if (err.statusCode === 429) {
      console.error('Rate limited; try again later')
    } else {
      console.error(`API error: ${err.message}`)
    }
  } else {
    console.error('Unexpected error:', err)
  }
}
```

## Streaming & Backpressure

The `ReadableStream<ChatEvent>` respects backpressure. If your consumer is slow, the stream will pause reading from the network until the buffer drains.

**Do not manually consume chunks without backpressure handling**:
```ts
// ❌ Dangerous: reads all chunks at once, may buffer unbounded memory
while (true) {
  const chunk = await reader.read()
  if (chunk.done) break
  process.stdout.write(chunk.value.content || '')
}

// ✅ Correct: use for-await, which respects backpressure
for await (const event of stream) {
  process.stdout.write(event.content || '')
}
```

## Performance Notes

- **SSE streams are long-lived**: Do not set aggressive timeouts. The SDK applies a timeout only to the initial connection, not the stream itself.
- **Event parsing**: Each line in the SSE response must be valid JSON. Malformed lines are silently skipped.
- **Memory**: Streaming ensures you don't load the entire response into memory at once — process events as they arrive.

## See Also

- [README.md](../packages/js/README.md) — Quick start and other resources
- [API Reference](https://synkvault-web-dev-partners-yqmxkygnlq-nw.a.run.app/api/v1/docs/openapi.json) — OpenAPI spec
