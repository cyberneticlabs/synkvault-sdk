using System.Text.Json;
using System.Text.Json.Serialization;

namespace SynkVault.Sdk.Models.Documents;

public sealed record DocumentResponse : Document
{
    [JsonPropertyName("extracted_properties")]
    public Dictionary<string, JsonElement>? ExtractedProperties { get; init; }
}
