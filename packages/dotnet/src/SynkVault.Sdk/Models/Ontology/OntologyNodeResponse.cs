using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ontology;

public sealed record OntologyNodeResponse
{
    [JsonPropertyName("data")]
    public required OntologyNode Data { get; init; }
}
