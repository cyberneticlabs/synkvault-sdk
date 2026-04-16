using System.Net.Http.Headers;
using SynkVault.Sdk.Internal;
using SynkVault.Sdk.Models.Documents;

namespace SynkVault.Sdk.Resources;

/// <summary>Document upload and management.</summary>
public sealed class DocumentsResource(SynkVaultClient client)
{
    /// <summary>Upload a document file for processing. Uses multipart/form-data.</summary>
    public Task<DocumentResponse> UploadAsync(
        UploadDocumentParams @params,
        CancellationToken cancellationToken = default)
    {
        var formData = new MultipartFormDataContent();
        var fileContent = new StreamContent(@params.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(@params.ContentType);
        formData.Add(fileContent, "file", @params.FileName);

        Dictionary<string, string?>? queryParams = @params.Dryrun.HasValue
            ? new() { ["dryrun"] = @params.Dryrun.Value.ToString().ToLowerInvariant() }
            : null;

        return client.UploadFileAsync<DocumentResponse>(
            "/api/v1/ingest/documents",
            formData,
            queryParams,
            cancellationToken);
    }

    /// <summary>List documents with optional filters and pagination.</summary>
    public Task<DocumentsResponse> ListAsync(
        ListDocumentsParams? @params = null,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<DocumentsResponse>(
            HttpMethod.Get,
            "/api/v1/ingest/documents",
            new RequestOptions { Params = @params?.ToQueryDict() },
            cancellationToken);

    /// <summary>Get a single document by ID.</summary>
    public Task<DocumentResponse> GetAsync(
        string id,
        GetDocumentParams? @params = null,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<DocumentResponse>(
            HttpMethod.Get,
            $"/api/v1/ingest/documents/{Uri.EscapeDataString(id)}",
            new RequestOptions { Params = @params?.ToQueryDict() },
            cancellationToken);
}
