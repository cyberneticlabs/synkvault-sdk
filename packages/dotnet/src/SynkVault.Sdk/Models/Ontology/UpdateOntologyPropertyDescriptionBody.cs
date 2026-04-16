using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ontology;

public sealed record UpdateOntologyPropertyDescriptionBody
{
    [JsonPropertyName("property_name")]
    public required string PropertyName { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }
}
