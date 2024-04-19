using System;
using ipk24chat_server.Chat;

namespace ipk24chat_server.Client;

public class ClientMessageEnvelope
{
    private readonly object _lock = new object();
    private AbstractChatUser _user;
    private ClientMessage _message;
    private string _tag = string.Empty;
    
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