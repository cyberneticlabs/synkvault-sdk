namespace SynkVault.Sdk.Models.Ontology;

public sealed record GetOntologyParams
{
    public bool? IncludeDescriptions { get; init; }
    public bool? IncludeProperties { get; init; }

    internal Dictionary<string, string?> ToQueryDict() => new()
    {
        ["include_descriptions"] = IncludeDescriptions?.ToString().ToLowerInvariant(),
        ["include_properties"] = IncludeProperties?.ToString().ToLowerInvariant(),
    };
}
