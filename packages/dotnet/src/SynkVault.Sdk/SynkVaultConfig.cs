namespace SynkVault.Sdk;

/// <summary>Configuration for <see cref="SynkVaultClient"/>.</summary>
public sealed class SynkVaultConfig
{
    /// <summary>Base URL of the SynkVault Partners API, e.g. https://api.synkvault.com</summary>
    public required string BaseUrl { get; init; }

    /// <summary>Organization ID — required for all non-health endpoints.</summary>
    public required string OrgId { get; init; }

    /// <summary>API key auth (X-Api-Key header). Either ApiKey or Token must be provided.</summary>
    public string? ApiKey { get; init; }

    /// <summary>JWT token auth (Authorization: Bearer header). Either ApiKey or Token must be provided.</summary>
    public string? Token { get; init; }

    /// <summary>Request timeout in milliseconds. Default: 30000.</summary>
    public int Timeout { get; init; } = 30_000;
}
