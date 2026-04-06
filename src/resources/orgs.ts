import type { SynkVaultClient } from '../client.js'
import type { OrgUsersResponse, OrgsResponse } from '../types.js'

export class OrgsResource {
  constructor(private readonly client: SynkVaultClient) {}

  /** List all organizations accessible to the authenticated credential. */
  list(): Promise<OrgsResponse> {
    return this.client.request<OrgsResponse>('GET', '/api/v1/orgs', {
      skipOrgId: true,
    })
  }

  /** List users in an organization. Requires org_owner or org_admin role. */
  listUsers(orgId?: string): Promise<OrgUsersResponse> {
    const id = orgId ?? this.client.getOrgId()
    return this.client.request<OrgUsersResponse>(
      'GET',
      `/api/v1/organizations/${encodeURIComponent(id)}/users`,
      { skipOrgId: true },
    )
  }
}
