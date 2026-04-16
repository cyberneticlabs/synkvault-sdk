namespace SynkVault.Sdk.Models.Knowledge;

public sealed record GetKnowledgeNodeParams
{
    public required string RecordId { get; init; }
    public bool? Nested { get; init; }
    public string? NodeId { get; init; }

    internal Dictionary<string, string?> ToQueryDict() => new()
    {
        ["record_id"] = RecordId,
        ["nested"] = Nested?.ToString().ToLowerInvariant(),
        ["node_id"] = NodeId,
    };
}
