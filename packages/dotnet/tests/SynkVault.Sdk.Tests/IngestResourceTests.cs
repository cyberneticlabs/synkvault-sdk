using SynkVault.Sdk.Models.Ingest;
using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

public sealed class IngestResourceTests
{
    private static readonly IngestResponse OkFixture = new()
    {
        Success = true,
        Message = "Ingested successfully",
    };

    [Fact]
    public async Task TextAsync_ReturnsIngestResponse()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, OkFixture);

        var result = await client.Ingest.TextAsync(
            new IngestTextBody { Content = "# Hello World" });

        Assert.True(result.Success);
        Assert.Equal("Ingested successfully", result.Message);
    }

    [Fact]
    public async Task JsonAsync_ReturnsIngestResponse()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, OkFixture);

        var result = await client.Ingest.JsonAsync(new IngestJsonBody
        {
            TargetClass = "Person",
            Data = new { name = "Alice", age = 30 },
        });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task UrlAsync_ReturnsIngestResponse()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, OkFixture);

        var result = await client.Ingest.UrlAsync(
            new IngestUrlBody { Url = "https://example.com/article" });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task TextAsync_ThrowsSynkVaultException_OnError()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(400, new { message = "Content too large" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Ingest.TextAsync(
                new IngestTextBody { Content = "too big" }));

        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("Content too large", ex.Message);
    }
}
