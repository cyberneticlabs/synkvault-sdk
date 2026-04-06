/**
 * Integration test — requires a running SynkVault Partners API instance.
 *
 * Set SYNKVAULT_TEST_BASE_URL to run (e.g. http://localhost:3102).
 * Tests are skipped automatically when the env var is not present.
 *
 * Example:
 *   SYNKVAULT_TEST_BASE_URL=http://localhost:3102 pnpm test
 */
import { describe, expect, it } from 'vitest'
import { SynkVaultClient } from '../../client.js'

const BASE_URL = process.env.SYNKVAULT_TEST_BASE_URL
const ORG_ID = process.env.SYNKVAULT_TEST_ORG_ID ?? 'integration-test-org'
const API_KEY = process.env.SYNKVAULT_TEST_API_KEY ?? 'integration-test-key'

describe.skipIf(!BASE_URL)('Integration — HealthResource', () => {
  it('check() returns ok status from live server', async () => {
    const client = new SynkVaultClient({
      baseUrl: BASE_URL!,
      orgId: ORG_ID,
      apiKey: API_KEY,
    })

    const result = await client.health.check()

    expect(result.status).toBe('ok')
    expect(typeof result.timestamp).toBe('string')
    expect(typeof result.version).toBe('string')
  })
})
