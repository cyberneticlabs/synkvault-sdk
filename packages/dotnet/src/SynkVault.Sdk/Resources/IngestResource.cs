using SynkVault.Sdk.Internal;
using SynkVault.Sdk.Models.Ingest;

namespace SynkVault.Sdk.Resources;

/// <summary>Content ingestion (text, JSON, URL).</summary>
public sealed class IngestResource(SynkVaultClient client)
{
    /// <summary>Ingest freeform text / markdown content. Max 500 KB.</summary>
    public Task<IngestResponse> TextAsync(
        IngestTextBody body,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<IngestResponse>(
            HttpMethod.Post,
            "/api/v1/ingest/text",
            new RequestOptions { Body = body },
            cancellationToken);

    /// <summary>Ingest structured JSON data for a specific ontology class.</summary>
    public Task<IngestResponse> JsonAsync(
        IngestJsonBody body,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<IngestResponse>(
            HttpMethod.Post,
            "/api/v1/ingest/json",
            new RequestOptions { Body = body },
            cancellationToken);

    /// <summary>Ingest content from an HTTP/HTTPS URL.</summary>
    public Task<IngestResponse> UrlAsync(
        IngestUrlBody body,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<IngestResponse>(
            HttpMethod.Post,
            "/api/v1/ingest/url",
            new RequestOptions { Body = body },
            cancellationToken);
}
