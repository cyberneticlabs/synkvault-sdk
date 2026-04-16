using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ingest;

public sealed record IngestUrlBody
{
    [JsonPropertyName("url")]
    public required string Url { get; init; }
}
