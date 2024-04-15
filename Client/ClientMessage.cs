using ipk24chat_server.Chat;

namespace ipk24chat_server.Client;


public abstract class ClientMessage
{
    public byte Type { get; set; }
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

    // public override byte[] Pack()
    // {
    //     string message = $"AUTH {Username} AS {DisplayName} USING {Secret}\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class JoinMessage : ClientMessage
{
    public string ChannelId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    
    public JoinMessage()
    {
        Type = ChatProtocol.MessageType.Join;
    }

    // public override byte[] Pack()
    // {
    //     string message = $"JOIN {ChannelId} AS {DisplayName}\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class MsgMessage : ClientMessage
{
    public string DisplayName { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    
    public MsgMessage()
    {
        Type = ChatProtocol.MessageType.Msg;
    }

    // public override byte[] Pack()
    // {
    //     string message = $"MSG FROM {DisplayName} IS {MessageContent}\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class ErrMessage : ClientMessage
{
    public string DisplayName { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    
    public ErrMessage()
    {
        Type = ChatProtocol.MessageType.Err;
    }

    // public override byte[] Pack()
    // {
    //     string message = $"ERR FROM {DisplayName} IS {MessageContent}\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class ByeMessage : ClientMessage
{
    public ByeMessage()
    {
        Type = ChatProtocol.MessageType.Bye;
    }
    // public override byte[] Pack()
    // {
    //     string message = $"BYE\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class ReplyMessage : ClientMessage
{
    public byte Result { get; set; }
    public string MessageContent { get; set; } = string.Empty;
    
    public ReplyMessage()
    {
        Type = ChatProtocol.MessageType.Reply;
    }

    // public override byte[] Pack()
    // {
    //     string resultText = Result == 0 ? "NOK" : "OK";
    //     string message = $"REPLY {resultText} IS {MessageContent}\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}
