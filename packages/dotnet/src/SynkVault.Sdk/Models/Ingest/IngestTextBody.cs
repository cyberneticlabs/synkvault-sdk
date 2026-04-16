using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ingest;

public sealed record IngestTextBody
{
    [JsonPropertyName("content")]
    public required string Content { get; init; }
}
