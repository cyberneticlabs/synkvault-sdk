using SynkVault.Sdk.Models.Chat;

namespace SynkVault.Sdk.Resources;

public sealed class ChatResource(SynkVaultClient client)
{
    public IAsyncEnumerable<ChatEvent> RunAsync(
        ChatRunParams @params,
        CancellationToken cancellationToken = default)
    {
        var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(@params.Message), "message");
        formData.Add(new StringContent(@params.Stream ? "true" : "false"), "stream");
        if (@params.SessionId is not null)
            formData.Add(new StringContent(@params.SessionId), "session_id");
        if (@params.UserId is not null)
            formData.Add(new StringContent(@params.UserId), "user_id");

        return client.StreamRequestAsync<ChatEvent>("/api/v1/chat", formData, cancellationToken);
    }
}
