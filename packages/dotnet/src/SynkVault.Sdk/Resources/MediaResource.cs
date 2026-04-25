namespace SynkVault.Sdk.Resources;

/// <summary>Media proxy for accessing stored files (images, video, etc.).</summary>
public sealed class MediaResource(SynkVaultClient client)
{
    /// <summary>
    /// Fetch a media file by its storage path. Returns a <see cref="Stream"/> of the raw bytes.
    /// No authentication required. The path is typically in the format returned by Knowledge Base
    /// endpoints: <c>{org_id}/{document_id}/images/{filename}.jpg</c>
    /// </summary>
    public Task<Stream> GetFileAsync(
        string path,
        CancellationToken cancellationToken = default)
        => client.FetchBinaryAsync($"/api/media/{path}", cancellationToken);
}
