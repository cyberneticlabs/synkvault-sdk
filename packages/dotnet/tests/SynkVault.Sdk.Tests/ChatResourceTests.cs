using SynkVault.Sdk.Models.Chat;
using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

public sealed class ChatResourceTests
{
    private static string SseBody(params object[] events)
    {
        var lines = events.Select(e =>
            "data: " + System.Text.Json.JsonSerializer.Serialize(e));
        return string.Join("\n", lines) + "\n";
    }

    [Fact]
    public async Task RunAsync_YieldsParsedChatEvents()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, SseBody(
            new { @event = "message", content = "Hello!" },
            new { @event = "done" }
        ));

        var events = new List<ChatEvent>();
        await foreach (var evt in client.Chat.RunAsync(new ChatRunParams { Message = "Hi" }))
            events.Add(evt);

        Assert.Equal(2, events.Count);
        Assert.Equal("message", events[0].Event);
        Assert.Equal("Hello!", events[0].Content);
        Assert.Equal("done", events[1].Event);
    }

    [Fact]
    public async Task RunAsync_SurfacesSessionId()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, SseBody(
            new { @event = "message", content = "Hi", session_id = "sess-abc" }
        ));

        var events = new List<ChatEvent>();
        await foreach (var evt in client.Chat.RunAsync(new ChatRunParams { Message = "Hey" }))
            events.Add(evt);

        Assert.Equal("sess-abc", events[0].SessionId);
    }

    [Fact]
    public async Task RunAsync_PostsToCorrectEndpointWithOrgId()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, SseBody(new { @event = "done" }));

        await foreach (var _ in client.Chat.RunAsync(new ChatRunParams { Message = "Test" })) { }

        var req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);
        Assert.Contains("/api/v1/chat", req.RequestUri!.AbsolutePath);
        Assert.Contains("org_id=", req.RequestUri!.Query);
    }

    [Fact]
    public async Task RunAsync_ThrowsSynkVaultException_On401()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(401, new { message = "Unauthorized" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(async () =>
        {
            await foreach (var _ in client.Chat.RunAsync(new ChatRunParams { Message = "Hi" })) { }
        });

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("Unauthorized", ex.Message);
    }

    [Fact]
    public async Task RunAsync_IgnoresMalformedSseLines()
    {
        var (client, handler) = TestHelper.MakeClient();
        var body = "data: not-json\ndata: {\"event\":\"done\"}\n";
        handler.EnqueueResponse(200, body);

        var events = new List<ChatEvent>();
        await foreach (var evt in client.Chat.RunAsync(new ChatRunParams { Message = "Hi" }))
            events.Add(evt);

        Assert.Single(events);
        Assert.Equal("done", events[0].Event);
    }
}
