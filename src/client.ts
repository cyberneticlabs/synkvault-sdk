import { SynkVaultError } from './errors.js'
import { HealthResource } from './resources/health.js'
import { IngestResource } from './resources/ingest.js'
import { KnowledgeResource } from './resources/knowledge.js'
import { OntologyResource } from './resources/ontology.js'
import { OrgsResource } from './resources/orgs.js'
import type { SynkVaultConfig } from './types.js'

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

  private readonly config: SynkVaultConfig

  constructor(config: SynkVaultConfig) {
    if (!config.baseUrl) throw new Error('SynkVaultClient: baseUrl is required')
    if (!config.orgId) throw new Error('SynkVaultClient: orgId is required')
    if (!config.apiKey && !config.token) {
      throw new Error(
        'SynkVaultClient: either apiKey or token must be provided',
      )
    }

    this.config = config
    this.health = new HealthResource(this)
    this.orgs = new OrgsResource(this)
    this.ontology = new OntologyResource(this)
    this.knowledge = new KnowledgeResource(this)
    this.ingest = new IngestResource(this)
  }

  async request<T>(
    method: 'GET' | 'POST',
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
      }
      if (this.config.apiKey) {
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
    let data: unknown
    try {
      data = raw ? JSON.parse(raw) : null
    } catch {
      data = { message: raw }
    }

    if (!response.ok) {
      const msg =
        (data as Record<string, string> | null)?.statusMessage ??
        (data as Record<string, string> | null)?.message ??
        (data as Record<string, string> | null)?.error ??
        response.statusText
      throw new SynkVaultError(response.status, msg, data)
    }

    return data as T
  }
}
