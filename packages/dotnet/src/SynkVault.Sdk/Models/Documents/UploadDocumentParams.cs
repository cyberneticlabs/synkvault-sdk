namespace SynkVault.Sdk.Models.Documents;

public sealed record UploadDocumentParams
{
    /// <summary>File content stream.</summary>
    public required Stream Content { get; init; }

    /// <summary>Original file name (e.g. "report.pdf").</summary>
    public required string FileName { get; init; }

    /// <summary>MIME type of the file. Default: application/octet-stream.</summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>When true, validates the document without persisting it.</summary>
    public bool? Dryrun { get; init; }
}
