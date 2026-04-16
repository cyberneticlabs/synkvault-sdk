using SynkVault.Sdk.Internal;
using SynkVault.Sdk.Models.Ontology;

namespace SynkVault.Sdk.Resources;

/// <summary>Ontology browsing and updates.</summary>
public sealed class OntologyResource(SynkVaultClient client)
{
    /// <summary>Retrieve all ontology nodes for the organization.</summary>
    public Task<OntologyResponse> ListAsync(
        GetOntologyParams? @params = null,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<OntologyResponse>(
            HttpMethod.Get,
            "/api/v1/ontology",
            new RequestOptions { Params = @params?.ToQueryDict() },
            cancellationToken);

    /// <summary>Retrieve a single ontology node by ID.</summary>
    public Task<OntologyNodeResponse> GetAsync(
        string nodeId,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<OntologyNodeResponse>(
            HttpMethod.Get,
            $"/api/v1/ontology/{Uri.EscapeDataString(nodeId)}",
            cancellationToken: cancellationToken);

    /// <summary>Retrieve children / descendants of an ontology node.</summary>
    public Task<OntologyChildrenResponse> GetChildrenAsync(
        string nodeId,
        GetOntologyChildrenParams? @params = null,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<OntologyChildrenResponse>(
            HttpMethod.Get,
            $"/api/v1/ontology/{Uri.EscapeDataString(nodeId)}/children",
            new RequestOptions { Params = @params?.ToQueryDict() },
            cancellationToken);

    /// <summary>Update the description of an ontology node.</summary>
    public Task<UpdateOntologyNodeDescriptionResponse> UpdateDescriptionAsync(
        string nodeId,
        UpdateOntologyNodeDescriptionBody body,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<UpdateOntologyNodeDescriptionResponse>(
            HttpMethod.Patch,
            $"/api/v1/ontology/{Uri.EscapeDataString(nodeId)}/description",
            new RequestOptions { Body = body },
            cancellationToken);

    /// <summary>Update the description of a property on an ontology node.</summary>
    public Task<UpdateOntologyPropertyDescriptionResponse> UpdatePropertyDescriptionAsync(
        string nodeId,
        UpdateOntologyPropertyDescriptionBody body,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<UpdateOntologyPropertyDescriptionResponse>(
            HttpMethod.Patch,
            $"/api/v1/ontology/{Uri.EscapeDataString(nodeId)}/properties/description",
            new RequestOptions { Body = body },
            cancellationToken);
}
