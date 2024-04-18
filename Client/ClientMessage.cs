using ipk24chat_server.Chat;

namespace ipk24chat_server.Client;

// ClientMessage is the base class for all messages that can be sent
public abstract class ClientMessage
{
    public byte Type { get; set; }
}

// ConfirmMessage is a message that confirms that the message was received (Udp only)
public class ConfirmMessage : ClientMessage
{
    public ushort RefMessageId { get; init;}
    public ConfirmMessage(ushort refMessageId)
    {
        Type = ChatProtocol.MessageType.Confirm;
        RefMessageId = refMessageId;
    }

}

// AuthMessage is an initial message that is sent to authenticate the user
public class AuthMessage: ClientMessage
{
    
    public string Username { get; init;}
    public string DisplayName { get; init;}
    public string Secret { get; init;}
    
    public AuthMessage(string username, string displayName, string secret)
    {
        Type = ChatProtocol.MessageType.Auth;
        Username = username;
        DisplayName = displayName;
        Secret = secret;
    }
}

// JoinMessage is a message that is sent when the user joins a channel
public class JoinMessage : ClientMessage
{
    public string ChannelId { get; init;}
    public string DisplayName { get; init;}
    
    public JoinMessage( string channelId, string displayName)
    {
        Type = ChatProtocol.MessageType.Join;
        ChannelId = channelId;
        DisplayName = displayName;
    }
}

// MsgMessage is a message that is sent when the user sends a text message
public class MsgMessage : ClientMessage
{
    public string DisplayName { get; init;}
    public string MessageContent { get; init;}
    
    public MsgMessage(string displayName, string messageContent)
    {
        DisplayName = displayName;
        MessageContent = messageContent;
        Type = ChatProtocol.MessageType.Msg;
    }
}

// ErrMessage is a message that is sent when an error occurs
public class ErrMessage : ClientMessage
{
    public string DisplayName { get; init; }
    public string MessageContent { get; init; }
    
    public ErrMessage(string displayName, string messageContent)
    {
        Type = ChatProtocol.MessageType.Err;
        DisplayName = displayName;
        MessageContent = messageContent;
    }
}

// ByeMessage is a message that is sent when the chat session should be ended
public class ByeMessage : ClientMessage
{
    public ByeMessage()
    {
        Type = ChatProtocol.MessageType.Bye;
    }
}

// ReplyMessage is a message that is sent as a result to any action
public class ReplyMessage : ClientMessage
{
    public byte Result { get; init;}
    public string MessageContent { get; init;}
    
    public ReplyMessage(byte result, string messageContent)
    {
        Result = result;
        MessageContent = messageContent;
        Type = ChatProtocol.MessageType.Reply;
    }
}
