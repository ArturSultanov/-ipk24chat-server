namespace ipk24chat_server.Common;

public abstract class AbstractChatUser
{
    public string ConnectionKey { get; private set; }

    // Constructor
    protected AbstractChatUser(string key)
    {
        ConnectionKey = key;
    }
}