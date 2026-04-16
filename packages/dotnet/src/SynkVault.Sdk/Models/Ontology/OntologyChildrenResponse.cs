using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ontology;

public sealed record OntologyChildrenResponse
{
    [JsonPropertyName("data")]
    public required List<OntologyNode> Data { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }
}
