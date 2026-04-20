import type { SynkVaultClient } from '../client.js'
import type {
  DocumentResponse,
  DocumentsResponse,
  GetDocumentParams,
  ListDocumentsParams,
  UploadDocumentParams,
} from '../types.js'

export class DocumentsResource {
  constructor(private readonly client: SynkVaultClient) {}

  /** Upload a document file for processing. Uses multipart/form-data. */
  upload(params: UploadDocumentParams): Promise<DocumentResponse> {
    const formData = new FormData()
    formData.append('file', params.file)
    return this.client.uploadFile<DocumentResponse>(
      '/api/v1/ingest/documents',
      formData,
      params.dryrun !== undefined ? { dryrun: params.dryrun } : undefined,
    )
  }

  /** List documents with optional filters and pagination. */
  list(params?: ListDocumentsParams): Promise<DocumentsResponse> {
    return this.client.request<DocumentsResponse>(
      'GET',
      '/api/v1/ingest/documents',
      { params: params as Record<string, unknown> },
    )
  }

  /** Get a single document by ID. */
  get(id: string, params?: GetDocumentParams): Promise<DocumentResponse> {
    return this.client.request<DocumentResponse>(
      'GET',
      `/api/v1/ingest/documents/${encodeURIComponent(id)}`,
      { params: params as Record<string, unknown> },
    )
  }
}
