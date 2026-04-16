using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Documents;

public record Document
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("original_file_name")]
    public required string OriginalFileName { get; init; }

    [JsonPropertyName("document_size")]
    public required long DocumentSize { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("created_on")]
    public required string CreatedOn { get; init; }
}
