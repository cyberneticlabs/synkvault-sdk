using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ontology;

public sealed record UpdateOntologyNodeDescriptionBody
{
    [JsonPropertyName("description")]
    public required string Description { get; init; }
}
