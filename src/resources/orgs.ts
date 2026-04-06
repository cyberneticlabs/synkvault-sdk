import type { SynkVaultClient } from '../client.js'
import type { OrgsResponse } from '../types.js'

export class OrgsResource {
  constructor(private readonly client: SynkVaultClient) {}

  /** List all organizations accessible to the authenticated credential. */
  list(): Promise<OrgsResponse> {
    return this.client.request<OrgsResponse>('GET', '/api/v1/orgs', {
      skipOrgId: true,
    })
  }
}
