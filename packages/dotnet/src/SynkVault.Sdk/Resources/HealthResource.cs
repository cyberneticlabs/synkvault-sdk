using SynkVault.Sdk.Internal;
using SynkVault.Sdk.Models.Health;

namespace SynkVault.Sdk.Resources;

/// <summary>Health check endpoint. No auth required.</summary>
public sealed class HealthResource(SynkVaultClient client)
{
    /// <summary>Check if the API is running and healthy. No auth required.</summary>
    public Task<HealthResponse> CheckAsync(CancellationToken cancellationToken = default)
        => client.RequestAsync<HealthResponse>(
            HttpMethod.Get,
            "/api/v1/health",
            new RequestOptions { SkipOrgId = true, SkipAuth = true },
            cancellationToken);
}
