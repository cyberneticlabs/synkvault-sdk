using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Knowledge;

public sealed record PaginationMeta
{
    [JsonPropertyName("page")]
    public required int Page { get; init; }

    [JsonPropertyName("page_size")]
    public required int PageSize { get; init; }

    [JsonPropertyName("total_count")]
    public required int TotalCount { get; init; }

    [JsonPropertyName("total_pages")]
    public required int TotalPages { get; init; }
}
