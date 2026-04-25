using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SynkVault.Sdk.Internal;
using SynkVault.Sdk.Models.Chat;
using SynkVault.Sdk.Resources;

namespace SynkVault.Sdk;

/// <summary>
/// Official .NET client for the SynkVault Partners API.
/// </summary>
public sealed class SynkVaultClient
{
    private readonly SynkVaultConfig _config;
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ── Public resource properties ─────────────────────────────────────────

    /// <summary>Health check endpoint. No auth required.</summary>
    public HealthResource Health { get; }

    /// <summary>Organization and user management.</summary>
    public OrgsResource Orgs { get; }

    /// <summary>Ontology browsing and updates.</summary>
    public OntologyResource Ontology { get; }

    /// <summary>Knowledge base querying.</summary>
    public KnowledgeResource Knowledge { get; }

    /// <summary>Content ingestion (text, JSON, URL).</summary>
    public IngestResource Ingest { get; }

    /// <summary>Document upload and management.</summary>
    public DocumentsResource Documents { get; }

    /// <summary>Chat / AI agent interaction via SSE streaming.</summary>
    public ChatResource Chat { get; }

    /// <summary>Media proxy for accessing stored files (images, video, etc.).</summary>
    public MediaResource Media { get; }

    // ── Constructors ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="SynkVaultClient"/> with a default <see cref="HttpClient"/>.
    /// </summary>
    public SynkVaultClient(SynkVaultConfig config)
        : this(config, new HttpClient()) { }

    /// <summary>
    /// Creates a new <see cref="SynkVaultClient"/> with an injected <see cref="HttpClient"/>.
    /// Use this constructor for testing or when using <c>IHttpClientFactory</c>.
    /// </summary>
    public SynkVaultClient(SynkVaultConfig config, HttpClient httpClient)
    {
        if (string.IsNullOrEmpty(config.BaseUrl))
            throw new ArgumentException("SynkVaultClient: baseUrl is required", nameof(config));
        if (string.IsNullOrEmpty(config.OrgId))
            throw new ArgumentException("SynkVaultClient: orgId is required", nameof(config));
        if (string.IsNullOrEmpty(config.ApiKey) && string.IsNullOrEmpty(config.Token))
            throw new ArgumentException(
                "SynkVaultClient: either apiKey or token must be provided", nameof(config));

        _config = config;
        _httpClient = httpClient;

        Health = new HealthResource(this);
        Orgs = new OrgsResource(this);
        Ontology = new OntologyResource(this);
        Knowledge = new KnowledgeResource(this);
        Ingest = new IngestResource(this);
        Documents = new DocumentsResource(this);
        Chat = new ChatResource(this);
        Media = new MediaResource(this);
    }

    // ── Internal helpers ────────────────────────────────────────────────────

    /// <summary>Returns the configured organization ID.</summary>
    internal string GetOrgId() => _config.OrgId;

    /// <summary>
    /// Sends an HTTP request and deserializes the response to <typeparamref name="T"/>.
    /// </summary>
    internal async Task<T> RequestAsync<T>(
        HttpMethod method,
        string path,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, options?.Params, options?.SkipOrgId ?? false);

        using var request = new HttpRequestMessage(method, url);
        ApplyAuth(request, options?.SkipAuth ?? false);

        if (options?.Body is not null)
        {
            var json = JsonSerializer.Serialize(options.Body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await SendWithTimeoutAsync(request, cancellationToken).ConfigureAwait(false);
        return await ParseResponseAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a multipart/form-data upload request and deserializes the response to <typeparamref name="T"/>.
    /// </summary>
    internal async Task<T> UploadFileAsync<T>(
        string path,
        MultipartFormDataContent formData,
        Dictionary<string, string?>? queryParams = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, queryParams, skipOrgId: false);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        ApplyAuth(request, skipAuth: false);
        request.Content = formData;

        var response = await SendWithTimeoutAsync(request, cancellationToken).ConfigureAwait(false);
        return await ParseResponseAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a multipart/form-data POST and streams the SSE response as <typeparamref name="T"/> events.
    /// Throws <see cref="SynkVaultException"/> immediately if the server returns a non-2xx status.
    /// </summary>
    internal async IAsyncEnumerable<T> StreamRequestAsync<T>(
        string path,
        MultipartFormDataContent formData,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, extraParams: null, skipOrgId: false);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        ApplyAuth(request, skipAuth: false);
        request.Content = formData;

        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        await ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null) break;
            if (!line.StartsWith("data: ", StringComparison.Ordinal)) continue;
            var json = line[6..];
            T? evt;
            try { evt = JsonSerializer.Deserialize<T>(json, JsonOptions); }
            catch { continue; }
            if (evt is not null) yield return evt;
        }
    }

    /// <summary>
    /// Fetches a binary resource and returns its contents as a <see cref="MemoryStream"/>.
    /// No org_id query param is appended and no auth header is applied.
    /// Throws <see cref="SynkVaultException"/> on non-2xx responses.
    /// </summary>
    internal async Task<Stream> FetchBinaryAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(path, extraParams: null, skipOrgId: true);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        using var response = await SendWithTimeoutAsync(request, cancellationToken).ConfigureAwait(false);
        await ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var bytes = await response.Content
            .ReadAsByteArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new MemoryStream(bytes);
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private string BuildUrl(string path, Dictionary<string, string?>? extraParams, bool skipOrgId)
    {
        var baseUri = new Uri(_config.BaseUrl.TrimEnd('/'));
        var fullUri = new Uri(baseUri, path);

        var queryParts = new List<string>();

        if (!skipOrgId)
            queryParts.Add($"org_id={Uri.EscapeDataString(_config.OrgId)}");

        if (extraParams is not null)
            foreach (var (k, v) in extraParams)
                if (v is not null)
                    queryParts.Add($"{Uri.EscapeDataString(k)}={Uri.EscapeDataString(v)}");

        if (queryParts.Count == 0)
            return fullUri.ToString();

        var separator = fullUri.Query.Length > 0 ? "&" : "?";
        return fullUri + separator + string.Join("&", queryParts);
    }

    private void ApplyAuth(HttpRequestMessage request, bool skipAuth)
    {
        if (skipAuth) return;

        if (!string.IsNullOrEmpty(_config.Token))
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _config.Token);
        else if (!string.IsNullOrEmpty(_config.ApiKey))
            request.Headers.Add("X-Api-Key", _config.ApiKey);
    }

    private async Task<HttpResponseMessage> SendWithTimeoutAsync(
        HttpRequestMessage request,
        CancellationToken callerToken)
    {
        using var timeoutCts = new CancellationTokenSource(_config.Timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutCts.Token, callerToken);

        return await _httpClient.SendAsync(request, linked.Token).ConfigureAwait(false);
    }

    private static async Task<T> ParseResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var raw = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        JsonDocument? doc = null;
        if (!string.IsNullOrEmpty(raw))
        {
            try { doc = JsonDocument.Parse(raw); }
            catch { /* leave null — treat as plain text */ }
        }

        if (!response.IsSuccessStatusCode)
        {
            var msg = ExtractErrorMessage(doc, response.ReasonPhrase);
            var responseData = doc?.RootElement.ValueKind == JsonValueKind.Object
                ? (object?)JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(raw!)
                : null;
            throw new SynkVaultException((int)response.StatusCode, msg, responseData);
        }

        if (string.IsNullOrEmpty(raw))
            return default!;

        return JsonSerializer.Deserialize<T>(raw, JsonOptions)!;
    }

    private static async Task ThrowIfErrorAsync(

        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        JsonDocument? doc = null;
        if (!string.IsNullOrEmpty(raw))
            try { doc = JsonDocument.Parse(raw); } catch { }
        var msg = ExtractErrorMessage(doc, response.ReasonPhrase);
        var data = doc?.RootElement.ValueKind == JsonValueKind.Object
            ? (object?)JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(raw)
            : null;
        throw new SynkVaultException((int)response.StatusCode, msg, data);
    }

    private static string ExtractErrorMessage(JsonDocument? doc, string? fallback)
    {
        if (doc is null) return fallback ?? "Unknown error";

        var root = doc.RootElement;
        foreach (var key in new[] { "statusMessage", "message", "error" })
            if (root.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.String)
                return el.GetString()!;

        return fallback ?? "Unknown error";
    }
}
