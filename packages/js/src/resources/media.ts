import type { SynkVaultClient } from '../client.js'

export class MediaResource {
  constructor(private readonly client: SynkVaultClient) {}

  /** Fetch a stored media file. Returns raw binary as a Blob. No auth required. */
  getFile(path: string): Promise<Blob> {
    return this.client.fetchBinary(`/api/media/${path}`)
  }
}
