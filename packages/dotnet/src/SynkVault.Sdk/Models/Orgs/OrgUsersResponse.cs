using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Orgs;

public sealed record OrgUsersResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    [JsonPropertyName("data")]
    public required List<OrgUser> Data { get; init; }
}
