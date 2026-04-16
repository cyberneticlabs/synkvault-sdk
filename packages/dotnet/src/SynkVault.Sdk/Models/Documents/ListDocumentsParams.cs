namespace SynkVault.Sdk.Models.Documents;

public sealed record ListDocumentsParams
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? Status { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public string? Search { get; init; }
    public bool? AllUsers { get; init; }
    public string? ByUsers { get; init; }

    internal Dictionary<string, string?> ToQueryDict() => new()
    {
        ["page"] = Page?.ToString(),
        ["page_size"] = PageSize?.ToString(),
        ["status"] = Status,
        ["start_date"] = StartDate,
        ["end_date"] = EndDate,
        ["search"] = Search,
        ["all_users"] = AllUsers?.ToString().ToLowerInvariant(),
        ["by_users"] = ByUsers,
    };
}
