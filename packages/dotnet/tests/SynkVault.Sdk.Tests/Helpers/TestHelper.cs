namespace SynkVault.Sdk.Tests.Helpers;

internal static class TestHelper
{
    internal const string BaseUrl = "https://api.example.com";
    internal const string OrgId = "org-123";
    internal const string ApiKey = "svk_test_key";

    internal static (SynkVaultClient Client, MockHttpMessageHandler Handler) MakeClient(
        string? apiKey = ApiKey,
        string? token = null)
    {
        var handler = new MockHttpMessageHandler();
        var config = new SynkVaultConfig
        {
            BaseUrl = BaseUrl,
            OrgId = OrgId,
            ApiKey = apiKey,
            Token = token,
        };
        var client = new SynkVaultClient(config, new HttpClient(handler));
        return (client, handler);
    }
}
