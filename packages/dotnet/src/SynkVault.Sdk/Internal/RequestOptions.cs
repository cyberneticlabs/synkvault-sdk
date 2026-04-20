namespace SynkVault.Sdk.Internal;

internal sealed record RequestOptions
{
    public Dictionary<string, string?>? Params { get; init; }
    public object? Body { get; init; }
    public bool SkipOrgId { get; init; }
    public bool SkipAuth { get; init; }
}
