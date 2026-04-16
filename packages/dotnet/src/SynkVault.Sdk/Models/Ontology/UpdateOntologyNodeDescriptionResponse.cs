using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ontology;

public sealed record UpdateOntologyNodeDescriptionResponse
{
    [JsonPropertyName("data")]
    public required OntologyNode Data { get; init; }
}
