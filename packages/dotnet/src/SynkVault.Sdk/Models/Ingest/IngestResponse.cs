using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ingest;

public sealed record IngestResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
