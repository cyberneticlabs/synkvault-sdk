using System.Text.Json;
using SynkVault.Sdk.Models.Knowledge;
using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

public sealed class KnowledgeResourceTests
{
    private static readonly PaginationMeta PageMeta = new()
    {
        Page = 1,
        PageSize = 20,
        TotalCount = 1,
        TotalPages = 1,
    };

    [Fact]
    public async Task ListNodesAsync_ReturnsNodes()
    {
        var (client, handler) = TestHelper.MakeClient();
        var row = new Dictionary<string, object> { ["id"] = "rec-1", ["name"] = "Alice" };
        handler.EnqueueResponse(200, new
        {
            success = true,
            data = new[] { row },
            pagination = PageMeta,
        });

        var result = await client.Knowledge.ListNodesAsync(
            new GetKnowledgeNodesParams { NodeName = "Person" });

        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal(1, result.Pagination.TotalCount);
    }

    [Fact]
    public async Task ListNodesAsync_WithAllParams_PassesQueryString()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new
        {
            success = true,
            data = Array.Empty<object>(),
            pagination = PageMeta,
        });

        await client.Knowledge.ListNodesAsync(new GetKnowledgeNodesParams
        {
            NodeName = "Location",
            Page = 2,
            PageSize = 10,
            StartDate = "2024-01-01",
            EndDate = "2024-12-31",
            Nested = true,
            NodeId = "node-1",
        });

        var query = handler.Requests[0].RequestUri!.Query;
        Assert.Contains("node_name=Location", query);
        Assert.Contains("page=2", query);
        Assert.Contains("page_size=10", query);
        Assert.Contains("nested=true", query);
        Assert.Contains("node_id=node-1", query);
    }

    [Fact]
    public async Task GetNodeAsync_ReturnsSingleNode()
    {
        var (client, handler) = TestHelper.MakeClient();
        var row = new Dictionary<string, object> { ["id"] = "rec-42" };
        handler.EnqueueResponse(200, new
        {
            success = true,
            data = row,
        });

        var result = await client.Knowledge.GetNodeAsync(
            new GetKnowledgeNodeParams { RecordId = "rec-42" });

        Assert.True(result.Success);
        Assert.Equal(JsonValueKind.String, result.Data["id"].ValueKind);
    }

    [Fact]
    public async Task GetNodeAsync_ReturnsInboundRelationships_WhenPresent()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new
        {
            success = true,
            data = new Dictionary<string, string> { ["id"] = "r1" },
            inboundRelationships = new[]
            {
                new { sourceNode = "Order", propName = "customer", key = "orders" },
            },
        });

        var result = await client.Knowledge.GetNodeAsync(
            new GetKnowledgeNodeParams { RecordId = "r1" });

        Assert.NotNull(result.InboundRelationships);
        Assert.Single(result.InboundRelationships!);
        Assert.Equal("Order", result.InboundRelationships![0].SourceNode);
    }

    [Fact]
    public async Task ListNodesAsync_ThrowsSynkVaultException_OnError()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(403, new { message = "Forbidden" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Knowledge.ListNodesAsync(
                new GetKnowledgeNodesParams { NodeName = "Secret" }));

        Assert.Equal(403, ex.StatusCode);
    }
}
