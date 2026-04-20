using System.Text.Json.Serialization;
using SynkVault.Sdk.Models.Knowledge;

namespace SynkVault.Sdk.Models.Documents;

public sealed record DocumentsResponse
{
    [JsonPropertyName("success")]
    public required bool Success { get; init; }

    [JsonPropertyName("data")]
    public required List<Document> Data { get; init; }

    [JsonPropertyName("pagination")]
    public required PaginationMeta Pagination { get; init; }
}
