namespace SynkVault.Sdk.Models.Ontology;

public sealed record GetOntologyChildrenParams
{
    /// <summary>Depth of descendants to retrieve. Clamped 1–10. Default: 1.</summary>
    public int? Depth { get; init; }

    internal Dictionary<string, string?> ToQueryDict() => new()
    {
        ["depth"] = Depth?.ToString(),
    };
}
