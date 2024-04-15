using ipk24chat_server.Common;

namespace ipk24chat_server.Client;

public class ClientMessageEnvelope
{
    private AbstractChatUser _user;
    private string _message = string.Empty;
    
    public AbstractChatUser User
    {
        get { return _user; }
        set { _user = value; }
    }
    
    public string Message
    {
        get { return _message; }
        set { _message = value; }
    }
    
    public ClientMessageEnvelope(AbstractChatUser user, string message)
    {
        User = user;
        Message = message;
    }
    
}