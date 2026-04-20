namespace SynkVault.Sdk.Models.Knowledge;

public sealed record GetKnowledgeNodesParams
{
    public required string NodeName { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public bool? Nested { get; init; }
    public string? NodeId { get; init; }

    internal Dictionary<string, string?> ToQueryDict() => new()
    {
        ["node_name"] = NodeName,
        ["page"] = Page?.ToString(),
        ["page_size"] = PageSize?.ToString(),
        ["start_date"] = StartDate,
        ["end_date"] = EndDate,
        ["nested"] = Nested?.ToString().ToLowerInvariant(),
        ["node_id"] = NodeId,
    };
}
