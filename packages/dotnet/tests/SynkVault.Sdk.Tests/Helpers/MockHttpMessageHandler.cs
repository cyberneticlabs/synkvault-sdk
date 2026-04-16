using System.Net;
using System.Text;
using System.Text.Json;

namespace SynkVault.Sdk.Tests.Helpers;

/// <summary>
/// A queue-based <see cref="HttpMessageHandler"/> for unit tests.
/// Enqueue responses before each act; inspect <see cref="Requests"/> after.
/// </summary>
internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<(int StatusCode, string Body)> _responses = new();

    public List<HttpRequestMessage> Requests { get; } = [];

    public void EnqueueResponse(int statusCode, object body)
        => _responses.Enqueue((statusCode, JsonSerializer.Serialize(body)));

    public void EnqueueResponse(int statusCode, string rawBody)
        => _responses.Enqueue((statusCode, rawBody));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);

        var (statusCode, body) = _responses.TryDequeue(out var r)
            ? r
            : (200, "{}");

        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };

        return Task.FromResult(response);
    }
}
