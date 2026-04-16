using System.Text;
using SynkVault.Sdk.Models.Documents;
using SynkVault.Sdk.Models.Knowledge;
using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

public sealed class DocumentsResourceTests
{
    private static readonly PaginationMeta PageMeta = new()
    {
        Page = 1,
        PageSize = 20,
        TotalCount = 2,
        TotalPages = 1,
    };

    private static DocumentResponse MakeDocResponse(string id) => new()
    {
        Id = id,
        OriginalFileName = $"{id}.pdf",
        DocumentSize = 1024,
        Status = "processed",
        CreatedOn = "2024-01-01T00:00:00Z",
    };

    [Fact]
    public async Task UploadAsync_ReturnsDocumentResponse()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, MakeDocResponse("doc-1"));

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("PDF content"));
        var result = await client.Documents.UploadAsync(new UploadDocumentParams
        {
            Content = stream,
            FileName = "test.pdf",
            ContentType = "application/pdf",
        });

        Assert.Equal("doc-1", result.Id);
        Assert.Equal("doc-1.pdf", result.OriginalFileName);
    }

    [Fact]
    public async Task UploadAsync_WithDryrun_PassesQueryParam()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, MakeDocResponse("doc-dry"));

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await client.Documents.UploadAsync(new UploadDocumentParams
        {
            Content = stream,
            FileName = "test.pdf",
            Dryrun = true,
        });

        var req = handler.Requests[0];
        Assert.Contains("dryrun=true", req.RequestUri!.Query);
    }

    [Fact]
    public async Task ListAsync_ReturnsDocumentList()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new
        {
            success = true,
            data = new[] { MakeDocResponse("doc-1"), MakeDocResponse("doc-2") },
            pagination = PageMeta,
        });

        var result = await client.Documents.ListAsync();

        Assert.True(result.Success);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.TotalCount);
    }

    [Fact]
    public async Task ListAsync_WithParams_PassesQueryParams()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new
        {
            success = true,
            data = Array.Empty<object>(),
            pagination = PageMeta,
        });

        await client.Documents.ListAsync(new ListDocumentsParams
        {
            Status = "processed",
            Page = 1,
            PageSize = 10,
            Search = "report",
        });

        var query = handler.Requests[0].RequestUri!.Query;
        Assert.Contains("status=processed", query);
        Assert.Contains("page=1", query);
        Assert.Contains("search=report", query);
    }

    [Fact]
    public async Task GetAsync_ReturnsSingleDocument()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, MakeDocResponse("doc-42"));

        var result = await client.Documents.GetAsync("doc-42");

        Assert.Equal("doc-42", result.Id);
        Assert.Equal("processed", result.Status);
    }

    [Fact]
    public async Task GetAsync_WithExtracted_PassesQueryParam()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, MakeDocResponse("doc-1"));

        await client.Documents.GetAsync("doc-1", new GetDocumentParams { Include = "extracted" });

        var query = handler.Requests[0].RequestUri!.Query;
        Assert.Contains("include=extracted", query);
    }

    [Fact]
    public async Task GetAsync_ThrowsSynkVaultException_On404()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(404, new { message = "Document not found" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Documents.GetAsync("missing-doc"));

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("Document not found", ex.Message);
    }
}
