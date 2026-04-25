using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

public sealed class MediaResourceTests
{
    private const string MediaPath = "org-123/doc-456/images/photo.jpg";

    [Fact]
    public async Task GetFileAsync_ReturnsStream()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, "BINARY_CONTENT");

        var result = await client.Media.GetFileAsync(MediaPath);

        Assert.NotNull(result);
        var ms = Assert.IsType<MemoryStream>(result);
        Assert.True(ms.Length > 0);
    }

    [Fact]
    public async Task GetFileAsync_CallsCorrectUrl()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, "BINARY_CONTENT");

        await client.Media.GetFileAsync(MediaPath);

        var req = handler.Requests[0];
        Assert.Contains($"/api/media/{MediaPath}", req.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetFileAsync_DoesNotAppendOrgId()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, "BINARY_CONTENT");

        await client.Media.GetFileAsync(MediaPath);

        var req = handler.Requests[0];
        Assert.DoesNotContain("org_id", req.RequestUri!.Query);
    }

    [Fact]
    public async Task GetFileAsync_DoesNotSendAuthHeaders()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, "BINARY_CONTENT");

        await client.Media.GetFileAsync(MediaPath);

        var req = handler.Requests[0];
        Assert.Null(req.Headers.Authorization);
        Assert.False(req.Headers.Contains("X-Api-Key"));
    }

    [Fact]
    public async Task GetFileAsync_ThrowsSynkVaultException_OnError()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(404, new { message = "Media file not found" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Media.GetFileAsync("bad/path"));

        Assert.Equal(404, ex.StatusCode);
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
