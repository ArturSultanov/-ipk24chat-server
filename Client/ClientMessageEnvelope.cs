using ipk24chat_server.Common;

namespace ipk24chat_server.Client;

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
        User = user;
        Message = message;
    }
    
}