using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Orgs;

public sealed record Org
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("role")]
    public required string Role { get; init; }
}
