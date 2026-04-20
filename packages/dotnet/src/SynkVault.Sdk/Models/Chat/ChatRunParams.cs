namespace SynkVault.Sdk.Models.Chat;

public sealed record ChatRunParams
{
    public required string Message { get; init; }
    public string? SessionId { get; init; }
    public bool Stream { get; init; } = true;
    public string? UserId { get; init; }
}
