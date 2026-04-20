using System.Text.Json;
using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Chat;

public sealed record ChatEvent
{
    [JsonPropertyName("event")]
    public required string Event { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("reasoning_content")]
    public string? ReasoningContent { get; init; }

    [JsonPropertyName("session_id")]
    public string? SessionId { get; init; }

    [JsonPropertyName("tool")]
    public Dictionary<string, JsonElement>? Tool { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}
