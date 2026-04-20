using SynkVault.Sdk.Internal;
using SynkVault.Sdk.Models.Orgs;

namespace SynkVault.Sdk.Resources;

/// <summary>Organization and user management.</summary>
public sealed class OrgsResource(SynkVaultClient client)
{
    /// <summary>List all organizations accessible to the authenticated credential.</summary>
    public Task<OrgsResponse> ListAsync(CancellationToken cancellationToken = default)
        => client.RequestAsync<OrgsResponse>(
            HttpMethod.Get,
            "/api/v1/orgs",
            new RequestOptions { SkipOrgId = true },
            cancellationToken);

    /// <summary>
    /// List users in an organization. Requires org_owner or org_admin role.
    /// Uses the configured orgId if <paramref name="orgId"/> is not supplied.
    /// </summary>
    public Task<OrgUsersResponse> ListUsersAsync(
        string? orgId = null,
        CancellationToken cancellationToken = default)
    {
        var id = orgId ?? client.GetOrgId();
        return client.RequestAsync<OrgUsersResponse>(
            HttpMethod.Get,
            $"/api/v1/organizations/{Uri.EscapeDataString(id)}/users",
            new RequestOptions { SkipOrgId = true },
            cancellationToken);
    }
}
