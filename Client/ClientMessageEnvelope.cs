using ipk24chat_server.Chat;

namespace ipk24chat_server.Client;

// ClientMessageEnvelope is a class that wraps a user and a message together for easier handling in the server.
public class ClientMessageEnvelope
{
    private AbstractChatUser _user;
    private ClientMessage _message;
    
    public AbstractChatUser User
    {
        get { return _user; }
        set { _user = value; }
    }
    
    public ClientMessage Message
    {
        get { return _message; }
        set { _message = value; }
    }
    
    public ClientMessageEnvelope(AbstractChatUser user, ClientMessage message)
    {
        _user = user;
        _message = message;
    }
    
}