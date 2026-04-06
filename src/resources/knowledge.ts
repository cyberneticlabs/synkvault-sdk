import type { SynkVaultClient } from '../client.js'
import type {
  GetKnowledgeNodeParams,
  GetKnowledgeNodesParams,
  KnowledgeNodeResponse,
  KnowledgeNodesResponse,
} from '../types.js'

export class KnowledgeResource {
  constructor(private readonly client: SynkVaultClient) {}

  /** Retrieve paginated nodes from the organization's knowledge base. */
  listNodes(params: GetKnowledgeNodesParams): Promise<KnowledgeNodesResponse> {
    return this.client.request<KnowledgeNodesResponse>(
      'GET',
      '/api/v1/knowledge/nodes',
      {
        params: params as unknown as Record<string, unknown>,
      },
    )
  }

  /** Retrieve a single knowledge base node by record ID. */
  getNode(params: GetKnowledgeNodeParams): Promise<KnowledgeNodeResponse> {
    return this.client.request<KnowledgeNodeResponse>(
      'GET',
      '/api/v1/knowledge/node',
      {
        params: params as unknown as Record<string, unknown>,
      },
    )
  }
}
