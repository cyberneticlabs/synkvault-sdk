namespace SynkVault.Sdk.Models.Documents;

public sealed record GetDocumentParams
{
    /// <summary>Pass <c>"extracted"</c> to include extracted properties in the response.</summary>
    public string? Include { get; init; }

    internal Dictionary<string, string?> ToQueryDict() => new()
    {
        ["include"] = Include,
    };
}
