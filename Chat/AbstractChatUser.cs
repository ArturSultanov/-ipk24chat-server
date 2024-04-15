using ipk24chat_server.Client;

namespace ipk24chat_server.Chat;

public abstract class AbstractChatUser(string key)
{
    public string ConnectionKey { get; private set; } = key;
    public abstract Task SendMessageAsync(ClientMessage message);
}