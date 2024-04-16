using ipk24chat_server.Chat;

namespace ipk24chat_server.Client;


public abstract class ClientMessage
{
    public byte Type { get; set; }
}

public class ConfirmMessage : ClientMessage
{
    public ConfirmMessage()
    {
        Type = ChatProtocol.MessageType.Confirm;
    }
}

public class AuthMessage : ClientMessage
{
    
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    
    public AuthMessage()
    {
        Type = ChatProtocol.MessageType.Auth;
    }
}

public class JoinMessage : ClientMessage
{
    public string ChannelId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    
    public JoinMessage()
    {
        Type = ChatProtocol.MessageType.Join;
    }
}

public class MsgMessage : ClientMessage
{
    public string DisplayName { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    
    public MsgMessage()
    {
        Type = ChatProtocol.MessageType.Msg;
    }
}

public class ErrMessage : ClientMessage
{
    public string DisplayName { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    
    public ErrMessage()
    {
        Type = ChatProtocol.MessageType.Err;
    }
}

public class ByeMessage : ClientMessage
{
    public ByeMessage()
    {
        Type = ChatProtocol.MessageType.Bye;
    }
}

public class ReplyMessage : ClientMessage
{
    public byte Result { get; set; }
    public string MessageContent { get; set; } = string.Empty;
    
    public ReplyMessage()
    {
        Type = ChatProtocol.MessageType.Reply;
    }
}
