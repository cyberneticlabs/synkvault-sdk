export class SynkVaultError extends Error {
  constructor(
    public readonly statusCode: number,
    message: string,
    public readonly data?: unknown,
  ) {
    super(message)
    this.name = 'SynkVaultError'
    // Maintain proper prototype chain for instanceof checks
    Object.setPrototypeOf(this, new.target.prototype)
  }
}
