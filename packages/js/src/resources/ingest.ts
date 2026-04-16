import type { SynkVaultClient } from '../client.js'
import type {
  IngestJsonBody,
  IngestResponse,
  IngestTextBody,
  IngestUrlBody,
} from '../types.js'

export class IngestResource {
  constructor(private readonly client: SynkVaultClient) {}

  /** Ingest freeform text / markdown content. Max 500 KB. */
  text(body: IngestTextBody): Promise<IngestResponse> {
    return this.client.request<IngestResponse>('POST', '/api/v1/ingest/text', {
      body,
    })
  }

  /** Ingest structured JSON data for a specific ontology class. */
  json(body: IngestJsonBody): Promise<IngestResponse> {
    return this.client.request<IngestResponse>('POST', '/api/v1/ingest/json', {
      body,
    })
  }

  /** Ingest content from an HTTP/HTTPS URL. */
  url(body: IngestUrlBody): Promise<IngestResponse> {
    return this.client.request<IngestResponse>('POST', '/api/v1/ingest/url', {
      body,
    })
  }
}
