using SynkVault.Sdk.Models.Health;
using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

public sealed class HealthResourceTests
{
    private static readonly HealthResponse Fixture = new()
    {
        Status = "ok",
        Timestamp = "2024-01-01T00:00:00Z",
        Version = "1.0.0",
    };

    [Fact]
    public async Task CheckAsync_ReturnsHealthPayload()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, Fixture);

        var result = await client.Health.CheckAsync();

        Assert.Equal(Fixture.Status, result.Status);
        Assert.Equal(Fixture.Timestamp, result.Timestamp);
        Assert.Equal(Fixture.Version, result.Version);
    }

    [Fact]
    public async Task CheckAsync_ThrowsSynkVaultException_OnError()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(503, new { message = "Service unavailable" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Health.CheckAsync());

        Assert.Equal(503, ex.StatusCode);
    }
}
