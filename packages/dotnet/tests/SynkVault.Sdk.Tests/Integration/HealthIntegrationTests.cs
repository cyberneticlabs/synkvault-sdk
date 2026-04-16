namespace SynkVault.Sdk.Tests.Integration;

/// <summary>
/// Live integration tests against a real SynkVault API instance.
/// Set SYNKVAULT_TEST_BASE_URL and SYNKVAULT_TEST_API_KEY environment variables to run.
/// </summary>
public sealed class HealthIntegrationTests
{
    private static readonly string? BaseUrl =
        Environment.GetEnvironmentVariable("SYNKVAULT_TEST_BASE_URL");

    private static readonly string? ApiKey =
        Environment.GetEnvironmentVariable("SYNKVAULT_TEST_API_KEY");

    private static readonly string? OrgId =
        Environment.GetEnvironmentVariable("SYNKVAULT_TEST_ORG_ID");

    [Fact(Skip = "Set SYNKVAULT_TEST_BASE_URL, SYNKVAULT_TEST_API_KEY, SYNKVAULT_TEST_ORG_ID to run")]
    public async Task CheckAsync_LiveServer_ReturnsOkStatus()
    {
        if (BaseUrl is null || ApiKey is null || OrgId is null)
            return;

        var client = new SynkVaultClient(new SynkVaultConfig
        {
            BaseUrl = BaseUrl,
            OrgId = OrgId,
            ApiKey = ApiKey,
        });

        var result = await client.Health.CheckAsync();

        Assert.NotEmpty(result.Status);
        Assert.NotEmpty(result.Version);
    }
}
