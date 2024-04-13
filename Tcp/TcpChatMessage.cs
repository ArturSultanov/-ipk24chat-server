using System.Text;
using ipk24chat_server.Common;

namespace ipk24chat_server.Tcp;


public abstract class TcpMessage
{
    public byte Type { get; set; }
}

public class TcpAuthMessage : TcpMessage
{
    
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    
    public TcpAuthMessage()
    {
        Type = ChatProtocol.MessageType.Auth;
    }

    // public override byte[] Pack()
    // {
    //     string message = $"AUTH {Username} AS {DisplayName} USING {Secret}\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class TcpJoinMessage : TcpMessage
{
    public string ChannelId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    
    public TcpJoinMessage()
    {
        Type = ChatProtocol.MessageType.Join;
    }

    // public override byte[] Pack()
    // {
    //     string message = $"JOIN {ChannelId} AS {DisplayName}\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class TcpMsgMessage : TcpMessage
{
    public string DisplayName { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    
    public TcpMsgMessage()
    {
        Type = ChatProtocol.MessageType.Msg;
    }

    // public override byte[] Pack()
    // {
    //     string message = $"MSG FROM {DisplayName} IS {MessageContent}\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class TcpErrMessage : TcpMessage
{
    public string DisplayName { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    
    public TcpErrMessage()
    {
        Type = ChatProtocol.MessageType.Err;
    }

    // public override byte[] Pack()
    // {
    //     string message = $"ERR FROM {DisplayName} IS {MessageContent}\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class TcpByeMessage : TcpMessage
{
    public TcpByeMessage()
    {
        Type = ChatProtocol.MessageType.Bye;
    }
    // public override byte[] Pack()
    // {
    //     string message = $"BYE\r\n";
    //     return Encoding.UTF8.GetBytes(message);
    // }
}

public class TcpReplyMessage : TcpMessage
{
    public byte Result { get; set; }
    public string MessageContent { get; set; } = string.Empty;
    
    public TcpReplyMessage()
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
