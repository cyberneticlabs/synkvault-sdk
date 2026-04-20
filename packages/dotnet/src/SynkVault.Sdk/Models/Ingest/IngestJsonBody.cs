using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ingest;

public sealed record IngestJsonBody
{
    [JsonPropertyName("target_class")]
    public required string TargetClass { get; init; }

    /// <summary>
    /// Arbitrary structured data for the target ontology class.
    /// Pass any POCO, anonymous type, or <c>Dictionary&lt;string, object?&gt;</c>.
    /// </summary>
    [JsonPropertyName("data")]
    public required object Data { get; init; }
}
