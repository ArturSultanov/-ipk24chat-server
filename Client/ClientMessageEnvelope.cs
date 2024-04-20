using ipk24chat_server.Chat;
namespace ipk24chat_server.Client;

/*
 * ClientMessageEnvelope is a class that wraps a ClientMessage and the user that sent it.
 * It is used to easily pass the message and user to the ChatMessageParser.
 */
public class ClientMessageEnvelope
{
    private readonly object _lock = new object();
    private AbstractChatUser _user;                 // The user that sent the message
    private ClientMessage _message;                 // The message that was sent
    private string _tag = string.Empty;             // A tag is used for tagging disconnected user messages
    
    public AbstractChatUser User
    {
        get
        {
            lock (_lock)
            {
                return _user;
            }
        }
        set
        {
            lock (_lock)
            {
                _user = value;
            }
        }
    }
    
    public ClientMessage Message
    {
        get
        {
            lock (_lock)
            {
                return _message;
            }
        }
        set
        {
            lock (_lock)
            {
                _message = value;
            }
        }
    }
    
    public string Tag
    {
        get
        {
            lock (_lock)
            {
                return _tag;
            }
        }
        set
        {
            lock (_lock)
            {
                _tag = value;
            }
        }
    }
    
    public ClientMessageEnvelope(AbstractChatUser user, ClientMessage message)
    {
        _user = user;
        _message = message;
    }
}