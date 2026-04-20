namespace SynkVault.Sdk;

/// <summary>
/// Thrown when the SynkVault API returns a non-2xx response.
/// </summary>
public sealed class SynkVaultException : Exception
{
    /// <summary>HTTP status code returned by the API.</summary>
    public int StatusCode { get; }

    /// <summary>
    /// Raw deserialized response body, if available.
    /// Named <c>ResponseData</c> to avoid shadowing <see cref="Exception.Data"/>.
    /// </summary>
    public object? ResponseData { get; }

    public SynkVaultException(int statusCode, string message, object? responseData = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseData = responseData;
    }
}
