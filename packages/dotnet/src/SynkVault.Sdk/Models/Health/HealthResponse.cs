using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Health;

public sealed record HealthResponse
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("timestamp")]
    public required string Timestamp { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }
}
