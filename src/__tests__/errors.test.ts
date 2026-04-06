import { describe, expect, it } from 'vitest'
import { SynkVaultError } from '../errors.js'

describe('SynkVaultError', () => {
  it('is an instance of Error', () => {
    const err = new SynkVaultError(404, 'Not found')
    expect(err).toBeInstanceOf(Error)
  })

  it('is an instance of SynkVaultError', () => {
    const err = new SynkVaultError(404, 'Not found')
    expect(err).toBeInstanceOf(SynkVaultError)
  })

  it('exposes statusCode', () => {
    const err = new SynkVaultError(401, 'Unauthorized')
    expect(err.statusCode).toBe(401)
  })

  it('exposes message', () => {
    const err = new SynkVaultError(500, 'Internal Server Error')
    expect(err.message).toBe('Internal Server Error')
  })

  it('exposes optional data', () => {
    const data = { detail: 'extra info' }
    const err = new SynkVaultError(400, 'Bad request', data)
    expect(err.data).toEqual(data)
  })

  it('has name SynkVaultError', () => {
    const err = new SynkVaultError(403, 'Forbidden')
    expect(err.name).toBe('SynkVaultError')
  })
})
