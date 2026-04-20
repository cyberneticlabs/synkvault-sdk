import { SynkVaultError } from './errors.js'
import { ChatResource } from './resources/chat.js'
import { DocumentsResource } from './resources/documents.js'
import { HealthResource } from './resources/health.js'
import { IngestResource } from './resources/ingest.js'
import { KnowledgeResource } from './resources/knowledge.js'
import { OntologyResource } from './resources/ontology.js'
import { OrgsResource } from './resources/orgs.js'
import type { ChatEvent, SynkVaultConfig } from './types.js'

interface RequestOptions {
  params?: Record<string, unknown>
  body?: unknown
  skipOrgId?: boolean
  skipAuth?: boolean
}

export class SynkVaultClient {
  readonly health: HealthResource
  readonly orgs: OrgsResource
  readonly ontology: OntologyResource
  readonly knowledge: KnowledgeResource
  readonly ingest: IngestResource
  readonly documents: DocumentsResource
  readonly chat: ChatResource

  private readonly config: SynkVaultConfig

  constructor(config: SynkVaultConfig) {
    if (!config.baseUrl) throw new Error('SynkVaultClient: baseUrl is required')
    if (!config.orgId) throw new Error('SynkVaultClient: orgId is required')
    if (!config.apiKey && !config.token) {
      throw new Error('SynkVaultClient: either apiKey or token must be provided')
    }

    this.config = config
    this.health = new HealthResource(this)
    this.orgs = new OrgsResource(this)
    this.ontology = new OntologyResource(this)
    this.knowledge = new KnowledgeResource(this)
    this.ingest = new IngestResource(this)
    this.documents = new DocumentsResource(this)
    this.chat = new ChatResource(this)
  }

  getOrgId(): string {
    return this.config.orgId
  }

  async request<T>(
    method: 'GET' | 'POST' | 'PATCH',
    path: string,
    options?: RequestOptions,
  ): Promise<T> {
    const url = new URL(path, this.config.baseUrl)

    if (!options?.skipOrgId) {
      url.searchParams.set('org_id', this.config.orgId)
    }

    if (options?.params) {
      for (const [k, v] of Object.entries(options.params)) {
        if (v !== undefined && v !== null) {
          url.searchParams.set(k, String(v))
        }
      }
    }

    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    }

    if (!options?.skipAuth) {
      if (this.config.token) {
        headers['Authorization'] = `Bearer ${this.config.token}`
      } else if (this.config.apiKey) {
        headers['X-Api-Key'] = this.config.apiKey
      }
    }

    const controller = new AbortController()
    const timeoutId = setTimeout(
      () => controller.abort(),
      this.config.timeout ?? 30_000,
    )

    let response: Response
    try {
      response = await fetch(url.toString(), {
        method,
        headers,
        body: options?.body !== undefined ? JSON.stringify(options.body) : undefined,
        signal: controller.signal,
      })
    } finally {
      clearTimeout(timeoutId)
    }

    const raw = await response.text()
    const data = this.parseResponseBody(raw)

    if (!response.ok) {
      throw new SynkVaultError(response.status, this.extractErrorMessage(data, response.statusText), data)
    }

    return data as T
  }

  async uploadFile<T>(
    path: string,
    formData: FormData,
    params?: Record<string, unknown>,
  ): Promise<T> {
    const url = new URL(path, this.config.baseUrl)
    url.searchParams.set('org_id', this.config.orgId)

    if (params) {
      for (const [k, v] of Object.entries(params)) {
        if (v !== undefined && v !== null) {
          url.searchParams.set(k, String(v))
        }
      }
    }

    const controller = new AbortController()
    const timeoutId = setTimeout(
      () => controller.abort(),
      this.config.timeout ?? 30_000,
    )

    let response: Response
    try {
      response = await fetch(url.toString(), {
        method: 'POST',
        headers: this.config.token
        ? { Authorization: `Bearer ${this.config.token}` }
        : { 'X-Api-Key': this.config.apiKey! },
        body: formData,
        signal: controller.signal,
      })
    } finally {
      clearTimeout(timeoutId)
    }

    const raw = await response.text()
    const data = this.parseResponseBody(raw)

    if (!response.ok) {
      throw new SynkVaultError(response.status, this.extractErrorMessage(data, response.statusText), data)
    }

    return data as T
  }

  private parseResponseBody(raw: string): unknown {
    try {
      return raw ? JSON.parse(raw) : null
    } catch {
      return { message: raw }
    }
  }

  private extractErrorMessage(data: unknown, fallback: string): string {
    return (
      (data as Record<string, string> | null)?.statusMessage ??
      (data as Record<string, string> | null)?.message ??
      (data as Record<string, string> | null)?.error ??
      fallback
    )
  }

  async streamRequest(
    path: string,
    formData: FormData,
  ): Promise<ReadableStream<ChatEvent>> {
    const url = new URL(path, this.config.baseUrl)
    url.searchParams.set('org_id', this.config.orgId)

    const headers: Record<string, string> = this.config.token
      ? { Authorization: `Bearer ${this.config.token}` }
      : { 'X-Api-Key': this.config.apiKey! }

    const controller = new AbortController()
    const timeoutId = setTimeout(
      () => controller.abort(),
      this.config.timeout ?? 30_000,
    )

    let response: Response
    try {
      response = await fetch(url.toString(), {
        method: 'POST',
        headers,
        body: formData,
        signal: controller.signal,
      })
    } finally {
      clearTimeout(timeoutId)
    }

    if (!response.ok) {
      const raw = await response.text()
      const data = this.parseResponseBody(raw)
      throw new SynkVaultError(response.status, this.extractErrorMessage(data, response.statusText), data)
    }

    if (!response.body) {
      throw new SynkVaultError(0, 'Response body is null — streaming not supported in this environment', null)
    }

    const reader = response.body.getReader()
    const decoder = new TextDecoder()
    let buffer = ''

    return new ReadableStream<ChatEvent>({
      async pull(controller) {
        const { done, value } = await reader.read()
        if (done) {
          controller.close()
          return
        }
        buffer += decoder.decode(value, { stream: true })
        const lines = buffer.split('\n')
        buffer = lines.pop() ?? ''
        for (const line of lines) {
          if (line.startsWith('data: ')) {
            try {
              controller.enqueue(JSON.parse(line.slice(6)) as ChatEvent)
            } catch {
              // malformed SSE line — skip
            }
          }
        }
      },
      cancel() {
        reader.cancel()
      },
    })
  }
}
