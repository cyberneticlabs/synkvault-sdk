import type { SynkVaultClient } from '../client.js'
import type { HealthResponse } from '../types.js'

export class HealthResource {
  constructor(private readonly client: SynkVaultClient) {}

  /** Check if the API is running and healthy. No auth required. */
  check(): Promise<HealthResponse> {
    return this.client.request<HealthResponse>('GET', '/api/v1/health', {
      skipOrgId: true,
      skipAuth: true,
    })
  }
}
