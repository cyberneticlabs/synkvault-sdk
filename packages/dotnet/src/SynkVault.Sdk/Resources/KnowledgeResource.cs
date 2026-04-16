using SynkVault.Sdk.Internal;
using SynkVault.Sdk.Models.Knowledge;

namespace SynkVault.Sdk.Resources;

/// <summary>Knowledge base querying.</summary>
public sealed class KnowledgeResource(SynkVaultClient client)
{
    /// <summary>Retrieve paginated nodes from the organization's knowledge base.</summary>
    public Task<KnowledgeNodesResponse> ListNodesAsync(
        GetKnowledgeNodesParams @params,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<KnowledgeNodesResponse>(
            HttpMethod.Get,
            "/api/v1/knowledge/nodes",
            new RequestOptions { Params = @params.ToQueryDict() },
            cancellationToken);

    /// <summary>Retrieve a single knowledge base node by record ID.</summary>
    public Task<KnowledgeNodeResponse> GetNodeAsync(
        GetKnowledgeNodeParams @params,
        CancellationToken cancellationToken = default)
        => client.RequestAsync<KnowledgeNodeResponse>(
            HttpMethod.Get,
            "/api/v1/knowledge/node",
            new RequestOptions { Params = @params.ToQueryDict() },
            cancellationToken);
}
