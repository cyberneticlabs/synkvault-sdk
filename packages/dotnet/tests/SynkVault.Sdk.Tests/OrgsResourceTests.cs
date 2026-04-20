using SynkVault.Sdk.Models.Orgs;
using SynkVault.Sdk.Tests.Helpers;

namespace SynkVault.Sdk.Tests;

public sealed class OrgsResourceTests
{
    private static readonly OrgsResponse OrgListFixture = new()
    {
        Success = true,
        Data =
        [
            new Org { Id = "org-1", Name = "Acme Corp", Role = "admin" },
            new Org { Id = "org-2", Name = "Beta Ltd", Role = "member" },
        ],
    };

    private static readonly OrgUsersResponse UsersFixture = new()
    {
        Success = true,
        Data =
        [
            new OrgUser { Id = "user-1", Name = "Alice", Role = "org_admin" },
        ],
    };

    [Fact]
    public async Task ListAsync_ReturnsOrgs()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, OrgListFixture);

        var result = await client.Orgs.ListAsync();

        Assert.True(result.Success);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("Acme Corp", result.Data[0].Name);
    }

    [Fact]
    public async Task ListUsersAsync_DefaultOrgId_ReturnsUsers()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, UsersFixture);

        var result = await client.Orgs.ListUsersAsync();

        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal("Alice", result.Data[0].Name);
    }

    [Fact]
    public async Task ListUsersAsync_ExplicitOrgId_UsesProvidedId()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(200, UsersFixture);

        await client.Orgs.ListUsersAsync("custom-org-id");

        var req = handler.Requests[0];
        Assert.Contains("custom-org-id", req.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task ListAsync_ThrowsSynkVaultException_On401()
    {
        var (client, handler) = TestHelper.MakeClient();
        handler.EnqueueResponse(401, new { message = "Unauthorized" });

        var ex = await Assert.ThrowsAsync<SynkVaultException>(
            () => client.Orgs.ListAsync());

        Assert.Equal(401, ex.StatusCode);
    }
}
