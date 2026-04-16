using System.Text;
using System.Text.Json;
using SynkVault.Sdk.Models.Documents;
using SynkVault.Sdk.Models.Ingest;
using SynkVault.Sdk.Models.Knowledge;
using SynkVault.Sdk.Models.Ontology;
using SynkVault.Sdk.Models.Orgs;
using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

/// <summary>
/// End-to-end developer workflow scenarios.
/// Each test emulates how a real consumer would use the SDK.
/// </summary>
public sealed class ScenariosTests
{
    // ── Scenario 1: Discover orgs then query knowledge ────────────────────

    [Fact]
    public async Task Scenario_DiscoverOrgs_ThenQueryKnowledge()
    {
        var (client, handler) = TestHelper.MakeClient();

        handler.EnqueueResponse(200, new OrgsResponse
        {
            Success = true,
            Data = [new Org { Id = "org-acme", Name = "Acme", Role = "admin" }],
        });
        handler.EnqueueResponse(200, new
        {
            success = true,
            data = new[] { new Dictionary<string, string> { ["id"] = "p1", ["name"] = "Alice" } },
            pagination = new PaginationMeta { Page = 1, PageSize = 20, TotalCount = 1, TotalPages = 1 },
        });

        var orgs = await client.Orgs.ListAsync();
        var firstOrgId = orgs.Data[0].Id;

        // Create a new client scoped to the discovered org
        var (scopedClient, scopedHandler) = TestHelper.MakeClient();
        scopedHandler.EnqueueResponse(200, new
        {
            success = true,
            data = new[] { new Dictionary<string, string> { ["id"] = "p1", ["name"] = "Alice" } },
            pagination = new PaginationMeta { Page = 1, PageSize = 20, TotalCount = 1, TotalPages = 1 },
        });

        var nodes = await scopedClient.Knowledge.ListNodesAsync(
            new GetKnowledgeNodesParams { NodeName = "Person" });

        Assert.Equal("org-acme", firstOrgId);
        Assert.Single(nodes.Data);
    }

    // ── Scenario 2: Browse ontology then ingest JSON ──────────────────────

    [Fact]
    public async Task Scenario_BrowseOntology_ThenIngestJson()
    {
        var (client, handler) = TestHelper.MakeClient();

        var personNode = new OntologyNode
        {
            Id = "node-person",
            Name = "Person",
            Label = "Person",
            ParentId = null,
            AllowDirectQuery = true,
            IncludeInMapPlotting = false,
        };
        handler.EnqueueResponse(200, new { data = new[] { personNode }, count = 1 });
        handler.EnqueueResponse(200, new IngestResponse { Success = true, Message = "ok" });

        var ontology = await client.Ontology.ListAsync();
        var targetClass = ontology.Data[0].Name;

        var ingestResult = await client.Ingest.JsonAsync(new IngestJsonBody
        {
            TargetClass = targetClass,
            Data = new { name = "Bob", age = 25 },
        });

        Assert.Equal("Person", targetClass);
        Assert.True(ingestResult.Success);
    }

    // ── Scenario 3: Ingest from multiple sources concurrently ────────────

    [Fact]
    public async Task Scenario_IngestFromMultipleSources_Concurrently()
    {
        var (urlClient, urlHandler) = TestHelper.MakeClient();
        var (textClient, textHandler) = TestHelper.MakeClient();
        var (jsonClient, jsonHandler) = TestHelper.MakeClient();

        var ok = new IngestResponse { Success = true, Message = "ok" };
        urlHandler.EnqueueResponse(200, ok);
        textHandler.EnqueueResponse(200, ok);
        jsonHandler.EnqueueResponse(200, ok);

        var results = await Task.WhenAll(
            urlClient.Ingest.UrlAsync(new IngestUrlBody { Url = "https://example.com" }),
            textClient.Ingest.TextAsync(new IngestTextBody { Content = "# Article" }),
            jsonClient.Ingest.JsonAsync(new IngestJsonBody
            {
                TargetClass = "Event",
                Data = new { title = "Conference 2024" },
            }));

        Assert.All(results, r => Assert.True(r.Success));
    }

    // ── Scenario 4: Error handling ────────────────────────────────────────

    [Fact]
    public async Task Scenario_ErrorHandling_401_403_404()
    {
        var (client, handler) = TestHelper.MakeClient();

        handler.EnqueueResponse(401, new { message = "Unauthorized" });
        handler.EnqueueResponse(403, new { message = "Forbidden" });
        handler.EnqueueResponse(404, new { message = "Not Found" });

        var ex401 = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Orgs.ListAsync());
        Assert.Equal(401, ex401.StatusCode);

        var ex403 = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Orgs.ListUsersAsync());
        Assert.Equal(403, ex403.StatusCode);

        var ex404 = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Ontology.GetAsync("missing"));
        Assert.Equal(404, ex404.StatusCode);
    }

    // ── Scenario 5: Paginate through knowledge ────────────────────────────

    [Fact]
    public async Task Scenario_PaginateThroughKnowledge()
    {
        var (client, handler) = TestHelper.MakeClient();

        var page1 = new
        {
            success = true,
            data = new[] { new Dictionary<string, string> { ["id"] = "r1" } },
            pagination = new PaginationMeta { Page = 1, PageSize = 1, TotalCount = 2, TotalPages = 2 },
        };
        var page2 = new
        {
            success = true,
            data = new[] { new Dictionary<string, string> { ["id"] = "r2" } },
            pagination = new PaginationMeta { Page = 2, PageSize = 1, TotalCount = 2, TotalPages = 2 },
        };

        handler.EnqueueResponse(200, page1);
        handler.EnqueueResponse(200, page2);

        var allRecords = new List<Dictionary<string, JsonElement>>();

        var first = await client.Knowledge.ListNodesAsync(
            new GetKnowledgeNodesParams { NodeName = "Person", Page = 1, PageSize = 1 });
        allRecords.AddRange(first.Data);

        if (first.Pagination.TotalPages > 1)
        {
            var second = await client.Knowledge.ListNodesAsync(
                new GetKnowledgeNodesParams { NodeName = "Person", Page = 2, PageSize = 1 });
            allRecords.AddRange(second.Data);
        }

        Assert.Equal(2, allRecords.Count);
    }
}
