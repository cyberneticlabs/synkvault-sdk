using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Orgs;

public sealed record OrgsResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    [JsonPropertyName("data")]
    public required List<Org> Data { get; init; }
}
