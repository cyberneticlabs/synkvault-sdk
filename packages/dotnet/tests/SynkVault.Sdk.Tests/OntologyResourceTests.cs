using SynkVault.Sdk.Models.Ontology;
using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

public sealed class OntologyResourceTests
{
    private static readonly OntologyNode NodeFixture = new()
    {
        Id = "node-1",
        Name = "Person",
        Label = "Person",
        ParentId = null,
        AllowDirectQuery = true,
        IncludeInMapPlotting = false,
    };

    [Fact]
    public async Task ListAsync_ReturnsOntologyNodes()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new { data = new[] { NodeFixture }, count = 1 });

        var result = await client.Ontology.ListAsync();

        Assert.Single(result.Data);
        Assert.Equal(1, result.Count);
        Assert.Equal("Person", result.Data[0].Name);
    }

    [Fact]
    public async Task ListAsync_WithParams_PassesQueryParams()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new { data = Array.Empty<object>(), count = 0 });

        await client.Ontology.ListAsync(new GetOntologyParams
        {
            IncludeDescriptions = true,
            IncludeProperties = true,
        });

        var req = handler.Requests[0];
        Assert.Contains("include_descriptions=true", req.RequestUri!.Query);
        Assert.Contains("include_properties=true", req.RequestUri!.Query);
    }

    [Fact]
    public async Task GetAsync_ReturnsSingleNode()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new { data = NodeFixture });

        var result = await client.Ontology.GetAsync("node-1");

        Assert.Equal("node-1", result.Data.Id);
        Assert.Equal("Person", result.Data.Name);
    }

    [Fact]
    public async Task GetAsync_ThrowsSynkVaultException_On404()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(404, new { message = "Node not found" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Ontology.GetAsync("missing"));

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("Node not found", ex.Message);
    }

    [Fact]
    public async Task GetChildrenAsync_ReturnsChildren()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new { data = new[] { NodeFixture }, count = 1 });

        var result = await client.Ontology.GetChildrenAsync("node-1",
            new GetOntologyChildrenParams { Depth = 2 });

        Assert.Single(result.Data);
        Assert.Equal(1, result.Count);
    }

    // Regression: allowDirectQuery and includeInMapPlotting are optional in the API schema.
    // If absent from the response, deserialization must not throw.

    [Fact]
    public async Task ListAsync_NodeMissingOptionalBoolFields_DoesNotThrow()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new
        {
            data = new[] { new { id = "node-1", name = "Person", label = "Person", parentId = (string?)null } },
            count = 1,
        });

        var result = await client.Ontology.ListAsync();

        Assert.Single(result.Data);
        Assert.False(result.Data[0].AllowDirectQuery);
        Assert.False(result.Data[0].IncludeInMapPlotting);
    }

    [Fact]
    public async Task GetAsync_NodeMissingOptionalBoolFields_DoesNotThrow()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new
        {
            data = new { id = "node-1", name = "Person", label = "Person", parentId = (string?)null },
        });

        var result = await client.Ontology.GetAsync("node-1");

        Assert.False(result.Data.AllowDirectQuery);
        Assert.False(result.Data.IncludeInMapPlotting);
    }

    [Fact]
    public async Task GetChildrenAsync_NodeMissingOptionalBoolFields_DoesNotThrow()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new
        {
            data = new[] { new { id = "node-1", name = "Person", label = "Person", parentId = (string?)null } },
            count = 1,
        });

        var result = await client.Ontology.GetChildrenAsync("node-1");

        Assert.Single(result.Data);
        Assert.False(result.Data[0].AllowDirectQuery);
        Assert.False(result.Data[0].IncludeInMapPlotting);
    }

    [Fact]
    public async Task UpdateDescriptionAsync_ReturnsUpdatedNode()
    {
        var (client, handler) = TestHelper.MakeClient();
        var updated = NodeFixture with { Description = "Updated desc" };
        handler.EnqueueResponse(200, new { data = updated });

        var result = await client.Ontology.UpdateDescriptionAsync(
            "node-1",
            new UpdateOntologyNodeDescriptionBody { Description = "Updated desc" });

        Assert.Equal("Updated desc", result.Data.Description);
    }

    [Fact]
    public async Task UpdatePropertyDescriptionAsync_ReturnsUpdatedProperties()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, new
        {
            data = new
            {
                properties = new Dictionary<string, string> { ["age"] = "The person's age" },
            },
        });

        var result = await client.Ontology.UpdatePropertyDescriptionAsync(
            "node-1",
            new UpdateOntologyPropertyDescriptionBody
            {
                PropertyName = "age",
                Description = "The person's age",
            });

        Assert.True(result.Data.Properties.ContainsKey("age"));
    }
}
