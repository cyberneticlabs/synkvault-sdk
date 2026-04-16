using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Knowledge;

public sealed record InboundRelationship
{
    /// <summary>The ontology node that references the queried node.</summary>
    [JsonPropertyName("sourceNode")]
    public required string SourceNode { get; init; }

    /// <summary>The property name on the source node that holds the reference.</summary>
    [JsonPropertyName("propName")]
    public required string PropName { get; init; }

    /// <summary>The key injected into each data record containing the array of referencing records.</summary>
    [JsonPropertyName("key")]
    public required string Key { get; init; }
}
