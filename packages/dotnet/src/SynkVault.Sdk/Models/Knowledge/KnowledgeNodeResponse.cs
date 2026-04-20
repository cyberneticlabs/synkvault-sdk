using System.Text.Json;
using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Knowledge;

public sealed record KnowledgeNodeResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    [JsonPropertyName("data")]
    public required Dictionary<string, JsonElement> Data { get; init; }

    [JsonPropertyName("edgeProperties")]
    public List<string>? EdgeProperties { get; init; }

    [JsonPropertyName("inboundRelationships")]
    public List<InboundRelationship>? InboundRelationships { get; init; }
}
