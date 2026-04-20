using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

public sealed class ClientTests
{
    // ── Constructor validation ────────────────────────────────────────────

    [Fact]
    public void Constructor_MissingBaseUrl_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new SynkVaultClient(new SynkVaultConfig
        {
            BaseUrl = "",
            OrgId = "org-123",
            ApiKey = "key",
        }));
        Assert.Contains("baseUrl", ex.Message);
    }

    [Fact]
    public void Constructor_MissingOrgId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new SynkVaultClient(new SynkVaultConfig
        {
            BaseUrl = "https://api.example.com",
            OrgId = "",
            ApiKey = "key",
        }));
        Assert.Contains("orgId", ex.Message);
    }

    [Fact]
    public void Constructor_MissingApiKeyAndToken_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new SynkVaultClient(new SynkVaultConfig
        {
            BaseUrl = "https://api.example.com",
            OrgId = "org-123",
        }));
        Assert.Contains("apiKey", ex.Message);
    }

    [Fact]
    public void Constructor_WithApiKey_Succeeds()
    {
        var client = new SynkVaultClient(new SynkVaultConfig
        {
            BaseUrl = "https://api.example.com",
            OrgId = "org-123",
            ApiKey = "key",
        });
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithToken_Succeeds()
    {
        var client = new SynkVaultClient(new SynkVaultConfig
        {
            BaseUrl = "https://api.example.com",
            OrgId = "org-123",
            Token = "bearer-token",
        });
        Assert.NotNull(client);
    }

    // ── Auth headers ──────────────────────────────────────────────────────

    [Fact]
    public async Task Request_ApiKey_SendsXApiKeyHeader()
    {
        var (client, handler) = TestHelper.MakeClient(apiKey: "my-api-key");
        handler.EnqueueResponse(200, new { data = Array.Empty<object>(), count = 0 });

        await client.Ontology.ListAsync();

        var req = handler.Requests[0];
        Assert.True(req.Headers.TryGetValues("X-Api-Key", out var values));
        Assert.Equal("my-api-key", values!.First());
    }

    [Fact]
    public async Task Request_Token_SendsBearerHeader()
    {
        var (client, handler) = TestHelper.MakeClient(apiKey: null, token: "my-token");
        handler.EnqueueResponse(200, new { data = Array.Empty<object>(), count = 0 });

        await client.Ontology.ListAsync();

        var req = handler.Requests[0];
        Assert.Equal("Bearer", req.Headers.Authorization?.Scheme);
        Assert.Equal("my-token", req.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task Request_SkipAuth_NoAuthHeader()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new { status = "ok", timestamp = "t", version = "v" });

        await client.Health.CheckAsync();

        var req = handler.Requests[0];
        Assert.Null(req.Headers.Authorization);
        Assert.False(req.Headers.Contains("X-Api-Key"));
    }

    // ── org_id injection ──────────────────────────────────────────────────

    [Fact]
    public async Task Request_AppendsOrgId_ByDefault()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new { data = Array.Empty<object>(), count = 0 });

        await client.Ontology.ListAsync();

        var req = handler.Requests[0];
        Assert.Contains("org_id=org-123", req.RequestUri!.Query);
    }

    [Fact]
    public async Task Request_SkipOrgId_DoesNotAppendOrgId()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new { success = true, data = Array.Empty<object>() });

        await client.Orgs.ListAsync();

        var req = handler.Requests[0];
        Assert.DoesNotContain("org_id", req.RequestUri!.Query);
    }

    // ── Error handling ────────────────────────────────────────────────────

    [Fact]
    public async Task Request_Non2xx_ThrowsSynkVaultException()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(401, new { message = "Unauthorized" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Ontology.ListAsync());

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("Unauthorized", ex.Message);
    }

    [Fact]
    public async Task Request_404_ThrowsSynkVaultException()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(404, new { message = "Node not found" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Ontology.GetAsync("missing-node"));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task Request_500_FallsBackToStatusText()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(500, "Internal Server Error");

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Ontology.ListAsync());

        Assert.Equal(500, ex.StatusCode);
    }
}
