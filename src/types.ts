// ── Auth / Config ─────────────────────────────────────────────────────────────

export interface SynkVaultConfig {
  /** Base URL of the SynkVault Partners API, e.g. https://api.synkvault.com */
  baseUrl: string
  /** Organization ID — required for all non-health endpoints */
  orgId: string
  /** API key auth (X-Api-Key header) */
  apiKey?: string
  /** JWT bearer token auth (Authorization: Bearer <token>) */
  token?: string
  /** Request timeout in milliseconds. Default: 30000 */
  timeout?: number
}

// ── Orgs ──────────────────────────────────────────────────────────────────────

export interface Org {
  id: string
  name: string
  role: string
}

export interface OrgsResponse {
  success: boolean
  data: Org[]
}

// ── Health ────────────────────────────────────────────────────────────────────

export interface HealthResponse {
  status: string
  timestamp: string
  version: string
}

// ── Ontology ──────────────────────────────────────────────────────────────────

export interface OntologyNode {
  id: string
  name: string
  label: string
  parentId: string | null
  description?: string
  properties?: Record<string, unknown>
  unique_properties?: Record<string, unknown>
  allowDirectQuery: boolean
  includeInMapPlotting: boolean
}

export interface OntologyResponse {
  data: OntologyNode[]
  count: number
}

export interface OntologyNodeResponse {
  data: OntologyNode
}

export interface OntologyChildrenResponse {
  data: OntologyNode[]
  count: number
}

// ── Knowledge ─────────────────────────────────────────────────────────────────

export interface PaginationMeta {
  page: number
  page_size: number
  total_count: number
  total_pages: number
}

export interface KnowledgeNodesResponse {
  success: boolean
  data: Record<string, unknown>[]
  pagination: PaginationMeta
  edgeProperties?: string[]
}

export interface KnowledgeNodeResponse {
  success: boolean
  data: Record<string, unknown>
  edgeProperties?: string[]
}

// ── Ingest ────────────────────────────────────────────────────────────────────

export interface IngestResponse {
  success: boolean
  message: string
}

// ── Query param / request body shapes ────────────────────────────────────────

export interface GetOntologyParams {
  include_descriptions?: boolean
  include_properties?: boolean
}

export interface GetOntologyChildrenParams {
  /** Depth of descendants to retrieve. Clamped 1–10. Default: 1 */
  depth?: number
}

export interface GetKnowledgeNodesParams {
  node_name: string
  page?: number
  page_size?: number
  start_date?: string
  end_date?: string
  nested?: boolean
  node_id?: string
}

export interface GetKnowledgeNodeParams {
  record_id: string
  nested?: boolean
  node_id?: string
}

export interface IngestTextBody {
  content: string
}

export interface IngestJsonBody {
  target_class: string
  data: Record<string, unknown>
}

export interface IngestUrlBody {
  url: string
}
