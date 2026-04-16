using System.Text.Json;
using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Ontology;

public sealed record OntologyNode
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("parentId")]
    public string? ParentId { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("properties")]
    public Dictionary<string, JsonElement>? Properties { get; init; }

    [JsonPropertyName("unique_properties")]
    public Dictionary<string, JsonElement>? UniqueProperties { get; init; }

    [JsonPropertyName("allowDirectQuery")]
    public required bool AllowDirectQuery { get; init; }

    [JsonPropertyName("includeInMapPlotting")]
    public required bool IncludeInMapPlotting { get; init; }
}
