import type { SynkVaultClient } from '../client.js'
import type {
  GetOntologyChildrenParams,
  GetOntologyParams,
  OntologyChildrenResponse,
  OntologyNodeResponse,
  OntologyResponse,
  UpdateOntologyNodeDescriptionBody,
  UpdateOntologyNodeDescriptionResponse,
  UpdateOntologyPropertyDescriptionBody,
  UpdateOntologyPropertyDescriptionResponse,
} from '../types.js'

export class OntologyResource {
  constructor(private readonly client: SynkVaultClient) {}

  /** Retrieve all ontology nodes for the organization. */
  list(params?: GetOntologyParams): Promise<OntologyResponse> {
    return this.client.request<OntologyResponse>('GET', '/api/v1/ontology', {
      params: params as Record<string, string | number | boolean | undefined>,
    })
  }

  /** Retrieve a single ontology node by ID. */
  get(nodeId: string): Promise<OntologyNodeResponse> {
    return this.client.request<OntologyNodeResponse>(
      'GET',
      `/api/v1/ontology/${encodeURIComponent(nodeId)}`,
    )
  }

  /** Retrieve children / descendants of an ontology node. */
  getChildren(
    nodeId: string,
    params?: GetOntologyChildrenParams,
  ): Promise<OntologyChildrenResponse> {
    return this.client.request<OntologyChildrenResponse>(
      'GET',
      `/api/v1/ontology/${encodeURIComponent(nodeId)}/children`,
      {
        params: params as Record<string, unknown>,
      },
    )
  }

  /** Update the description of an ontology node. */
  updateDescription(
    nodeId: string,
    body: UpdateOntologyNodeDescriptionBody,
  ): Promise<UpdateOntologyNodeDescriptionResponse> {
    return this.client.request<UpdateOntologyNodeDescriptionResponse>(
      'PATCH',
      `/api/v1/ontology/${encodeURIComponent(nodeId)}/description`,
      { body },
    )
  }

  /** Update the description of a property on an ontology node. */
  updatePropertyDescription(
    nodeId: string,
    body: UpdateOntologyPropertyDescriptionBody,
  ): Promise<UpdateOntologyPropertyDescriptionResponse> {
    return this.client.request<UpdateOntologyPropertyDescriptionResponse>(
      'PATCH',
      `/api/v1/ontology/${encodeURIComponent(nodeId)}/properties/description`,
      { body },
    )
  }
}
