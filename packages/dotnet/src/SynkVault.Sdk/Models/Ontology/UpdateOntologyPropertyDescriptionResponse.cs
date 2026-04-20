using System.Text.Json;
using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ontology;

public sealed record UpdateOntologyPropertyDescriptionResponseData
{
    [JsonPropertyName("properties")]
    public required Dictionary<string, JsonElement> Properties { get; init; }
}

public sealed record UpdateOntologyPropertyDescriptionResponse
{
    [JsonPropertyName("data")]
    public required UpdateOntologyPropertyDescriptionResponseData Data { get; init; }
}
