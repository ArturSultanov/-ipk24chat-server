using System.Text;
using ipk24chat_server.Common;

namespace ipk24chat_server.Tcp;

public static class TcpPacker
{
    
    // Transform TcpMessage object into bytes to be sent to client, according to the IPK24-chat protocol.
    public static byte[] Pack(TcpMessage message)
    {
        switch (message.Type)
        {
            case ChatProtocol.MessageType.Reply:
                return PackReplyMessage((TcpReplyMessage)message);
            case ChatProtocol.MessageType.Msg:
                return PackMsgMessage((TcpMsgMessage)message);
            case ChatProtocol.MessageType.Err:
                return PackErrMessage((TcpErrMessage)message);
            case ChatProtocol.MessageType.Bye:
                return PackByeMessage();
            default:
                //return Array.Empty<byte>();
                throw new ArgumentException($"Unknown message type: {message}");
        }
    }
    
    private static byte[] PackReplyMessage(TcpReplyMessage replyMessage)
    {
        string resultText = replyMessage.Result == 0 ? "NOK" : "OK";
        string message = $"REPLY {resultText} IS {replyMessage.MessageContent}\r\n";
        return Encoding.UTF8.GetBytes(message);
    }
    
    private static byte[] PackMsgMessage(TcpMsgMessage msgMessage)
    {
        string message = $"MSG FROM {msgMessage.DisplayName} IS {msgMessage.MessageContent}\r\n";
        return Encoding.UTF8.GetBytes(message);
    }
    
    private static byte[] PackErrMessage(TcpErrMessage errMessage)
    {
        string message = $"ERR FROM {errMessage.DisplayName} IS {errMessage.MessageContent}\r\n";
        return Encoding.UTF8.GetBytes(message);
    }
    
    private static byte[] PackByeMessage()
    {
        string message = $"BYE\r\n";
        return Encoding.UTF8.GetBytes(message);
    }
    
    
    // Transform string received from client into TcpMessage object, according to the IPK24-chat protocol.
    public static TcpMessage Unpack(string message)
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
                return new TcpByeMessage();
            }
            else
            {
                throw new ArgumentException($"Unknown message type: {message}");
            }
        }

        private static TcpAuthMessage ParseAuthMessage(string message)
        {
            var parts = message.Split(' ');
            return new TcpAuthMessage
            {
                Username = parts[1],
                DisplayName = parts[3],
                Secret = parts[5]
            };
        }

        private static TcpJoinMessage ParseJoinMessage(string message)
        {
            var parts = message.Split(' ');
            return new TcpJoinMessage
            {
                ChannelId = parts[1],
                DisplayName = parts[3]
            };
        }

        private static TcpMsgMessage ParseMsgMessage(string message)
        {
            var parts = message.Split(new[] { "MSG FROM", "IS" }, StringSplitOptions.RemoveEmptyEntries);
            return new TcpMsgMessage
            {
                DisplayName = parts[0].Trim(),
                MessageContent = parts[1].Trim()
            };
        }

        private static TcpErrMessage ParseErrMessage(string message)
        {
            var parts = message.Split(new[] { "ERR FROM", "IS" }, StringSplitOptions.RemoveEmptyEntries);
            return new TcpErrMessage
            {
                DisplayName = parts[0].Trim(),
                MessageContent = parts[1].Trim()
            };
        }
}