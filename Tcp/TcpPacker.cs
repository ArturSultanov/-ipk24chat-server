using System.Text;
using ipk24chat_server.Client;
using ipk24chat_server.Chat;
namespace ipk24chat_server.Tcp;

/*
 * TcpPacker is a class that is used to pack and unpack messages from the client.
 * It is used to transform a TcpMessage object into bytes to be sent to the client, according to the IPK24-chat protocol.
 * It is also used to transform a string received from the TCP client into a TcpMessage object, according to the IPK24-chat protocol.
 */
public static class TcpPacker
{
    
    /*
     * Pack a ClientMessage object into bytes to be sent to the client, according to the IPK24-chat protocol.
     * The message is packed according to the message type.
     */
    public static byte[] Pack(ClientMessage message)
    {
        switch (message.Type)
        {
            case ChatProtocol.MessageType.Reply:
                return PackReplyMessage((ReplyMessage)message);
            case ChatProtocol.MessageType.Msg:
                return PackMsgMessage((MsgMessage)message);
            case ChatProtocol.MessageType.Err:
                return PackErrMessage((ErrMessage)message);
            case ChatProtocol.MessageType.Bye:
                return PackByeMessage();
            default:
                return [];
        }
    }
    
    private static byte[] PackReplyMessage(ReplyMessage replyMessage)
    {
        string resultText = replyMessage.Result == 0 ? "NOK" : "OK";
        string message = $"REPLY {resultText} IS {replyMessage.MessageContent}\r\n";
        return Encoding.UTF8.GetBytes(message);
    }
    
    private static byte[] PackMsgMessage(MsgMessage msgMessage)
    {
        string message = $"MSG FROM {msgMessage.DisplayName} IS {msgMessage.MessageContent}\r\n";
        return Encoding.UTF8.GetBytes(message);
    }
    
    private static byte[] PackErrMessage(ErrMessage errMessage)
    {
        string message = $"ERR FROM {errMessage.DisplayName} IS {errMessage.MessageContent}\r\n";
        return Encoding.UTF8.GetBytes(message);
    }
    
    private static byte[] PackByeMessage()
    {
        string message = $"BYE\r\n";
        return Encoding.UTF8.GetBytes(message);
    }
    
    
    /*
     * Unpack a string received from the TCP client into a ClientMessage object, according to the IPK24-chat protocol.
     * The message is unpacked according to the message type.
     */
    public static ClientMessage Unpack(string message)
    {
        if (message.StartsWith("AUTH"))
        {
            return ParseAuthMessage(message);
        }
        else if (message.StartsWith("JOIN"))
        {
            return ParseJoinMessage(message);
        }
        else if (message.StartsWith("MSG FROM"))
        {
            return ParseMsgMessage(message);
        }
        else if (message.StartsWith("ERR FROM"))
        {
            return ParseErrMessage(message);
        }
        else if (message.StartsWith("BYE"))
        {
            return ParseByeMessage(message);
        }
        else
        {
            return new UnknownMessage();
        }
    }

    private static ClientMessage ParseAuthMessage(string message)
    {
        const string commandPrefix = "AUTH ";
        if (!message.StartsWith(commandPrefix)) return new UnknownMessage();

        var parts = message.Substring(commandPrefix.Length).Split(new[] {" AS ", " USING "}, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return new UnknownMessage();

        string username = parts[0].Trim();
        string displayName = parts[1].Trim();
        string secret = parts[2].Trim();

        if (username.Length > ChatProtocol.MaxUsernameLength ||
            displayName.Length > ChatProtocol.MaxDisplayNameLength ||
            secret.Length > ChatProtocol.MaxSecretLength)
        {
            return new UnknownMessage();
        }
        return new AuthMessage(username, displayName, secret);
    }


    private static ClientMessage ParseJoinMessage(string message)
    {
        const string commandPrefix = "JOIN ";
        if (!message.StartsWith(commandPrefix)) return new UnknownMessage();

        var parts = message.Substring(commandPrefix.Length).Split(new[] {" AS "}, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return new UnknownMessage();

        string channelId = parts[0].Trim();
        string displayName = parts[1].Trim();

        if (channelId.Length > ChatProtocol.MaxChannelIdLength || displayName.Length > ChatProtocol.MaxDisplayNameLength)
        {
            return new UnknownMessage();
        }
        return new JoinMessage(channelId, displayName);
    }


    private static ClientMessage ParseMsgMessage(string message)
    {
        const string commandPrefix = "MSG FROM ";
        if (!message.StartsWith(commandPrefix)) return new UnknownMessage();

        var parts = message.Substring(commandPrefix.Length).Split(new[] {" IS "}, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return new UnknownMessage();

        string displayName = parts[0].Trim();
        string messageContent = parts[1].Trim();

        if (displayName.Length > ChatProtocol.MaxDisplayNameLength || messageContent.Length > ChatProtocol.MaxMessageContentLength)
        {
            return new UnknownMessage();
        }
        return new MsgMessage(displayName, messageContent);
    }


    private static ClientMessage ParseErrMessage(string message)
    {
        const string commandPrefix = "ERR FROM ";
        if (!message.StartsWith(commandPrefix)) return new UnknownMessage();

        var parts = message.Substring(commandPrefix.Length).Split(new[] {" IS "}, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return new UnknownMessage();

        string displayName = parts[0].Trim();
        string messageContent = parts[1].Trim();

        if (displayName.Length > ChatProtocol.MaxDisplayNameLength || messageContent.Length > ChatProtocol.MaxMessageContentLength)
        {
            return new UnknownMessage();
        }
        return new ErrMessage(displayName, messageContent);
    }
    
    private static ClientMessage ParseByeMessage(string message)
    {
        const string commandPrefix = "BYE";
        if (message.Trim().Equals(commandPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return new ByeMessage();
        }
        return new UnknownMessage();
    }
}
