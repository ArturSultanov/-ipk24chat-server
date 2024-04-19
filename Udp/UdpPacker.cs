using System.Text;
using ipk24chat_server.Chat;
using ipk24chat_server.Client;

namespace ipk24chat_server.Udp;

public class UdpPacker
{
    /*
     * Pack a message into a byte array
     * Arguments:
     * - message: The message to be packed
     * - messageId: The message ID used for UDP communication
     */
    public static byte[] Pack(ClientMessage message, ushort? messageId)
    {
        using (var ms = new MemoryStream())
        {
            using (var writer = new BinaryWriter(ms, Encoding.ASCII))
            {
                switch (message)
                {
                    case ConfirmMessage confirmMessage: // The messageId is not used for ConfirmMessage
                        PackConfirmMessage(writer, confirmMessage);
                        break;
                    case ReplyMessage replyMessage:
                        PackReplyMessage(writer, replyMessage, messageId);
                        break;
                    case MsgMessage msgMessage:
                        PackMsgMessage(writer, msgMessage, messageId);
                        break;
                    case ErrMessage errMessage:
                        PackErrMessage(writer, errMessage, messageId);
                        break;
                    case ByeMessage:
                        PackByeMessage(writer, messageId);
                        break;
                } 
            }
            return ms.ToArray();
        }
    }
    
    private static void PackConfirmMessage(BinaryWriter writer, ConfirmMessage message)
    {
        writer.Write(ChatProtocol.MessageType.Confirm); // Write the message type byte (0x00 for CONFIRM)
        WriteMessageId(writer, message.MessageId);      // Write the message ID with endianness conversion
    }
    
    
    private static void PackReplyMessage(BinaryWriter writer, ReplyMessage message, ushort? messageId)
    {
        writer.Write(ChatProtocol.MessageType.Reply);   // Write the message type byte (0x01 for REPLY)
        WriteMessageId(writer, messageId);              // Write the message ID with endianness conversion
        writer.Write(message.Result);                   // Write the result byte
        WriteMessageId(writer, message.RefMessageId);   // Write the ref message ID with endianness conversion
        WriteString(writer, message.MessageContent);    // Write the MessageContent followed by a null terminator
    }

    private static void PackMsgMessage(BinaryWriter writer, MsgMessage message, ushort? messageId)
    {
        writer.Write(ChatProtocol.MessageType.Msg);     // Write the message type byte (0x04 for MSG)
        WriteMessageId(writer, messageId);              // Handle MessageID with endianness conversion
        WriteString(writer, message.DisplayName);       // Write the DisplayName followed by a null terminator
        WriteString(writer, message.MessageContent);    // Write the MessageContent followed by a null terminator
    }

    private static void PackErrMessage(BinaryWriter writer, ErrMessage message, ushort? messageId)
    {
        writer.Write(ChatProtocol.MessageType.Err);     // Write the message type byte (0xFE for ERR)
        WriteMessageId(writer, messageId);              // Write the message ID with endianness conversion
        WriteString(writer, message.DisplayName);       // Write the DisplayName followed by a null terminator
        WriteString(writer, message.MessageContent);    // Write the MessageContent followed by a null terminator
    }

    private static void PackByeMessage(BinaryWriter writer, ushort? messageId)
    {
        writer.Write(ChatProtocol.MessageType.Bye);                 // Write the message type byte (0xFF for BYE)
        WriteMessageId(writer, messageId);  // Write the message ID with endianess conversion
    }
    
    private static void WriteMessageId(BinaryWriter writer, ushort? messageId) // Write the messageID in big-endian format
    {
        byte[] messageIdBytes = BitConverter.GetBytes((ushort)messageId!);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(messageIdBytes); // Convert to big-endian if system is little-endian
        }
        writer.Write(messageIdBytes);
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        writer.Write(Encoding.ASCII.GetBytes(value + '\0'));
    }
    
    
    // Unpack
    public static ClientMessage Unpack(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        var type = reader.ReadByte();
        var messageId = ReadMessageId(reader);

        switch (type)
        {
            case ChatProtocol.MessageType.Auth:
                return ParseAuthMessage(reader,messageId);
            
            case ChatProtocol.MessageType.Confirm:
                return new ConfirmMessage(messageId);
            
            case ChatProtocol.MessageType.Msg:
                return ParseMsgMessage(reader, messageId);
            
            case ChatProtocol.MessageType.Err:
                return ParseErrMessage(reader, messageId);
            
            default:
                return new UnknownMessage();
                //throw new NotSupportedException($"Unsupported message type: {type}");
        }
    }
    
    private static AuthMessage ParseAuthMessage(BinaryReader reader, ushort messageId)
    {
        var username = ReadString(reader);
        var displayName = ReadString(reader);
        var secret = ReadString(reader);
        return new AuthMessage(username, displayName, secret)
        {
            MessageId = messageId
        };
    } 

    private static MsgMessage ParseMsgMessage(BinaryReader reader, ushort messageId)
    {
        var displayName = ReadString(reader);
        var messageContent = ReadString(reader);
        return new MsgMessage(displayName, messageContent)
        {
            MessageId = messageId
        };
    }
    
    private static ErrMessage ParseErrMessage(BinaryReader reader, ushort messageId)
    {
        var displayName = ReadString(reader);
        var messageContent = ReadString(reader);
        return new ErrMessage(displayName, messageContent)
        {
            MessageId = messageId
        };
    }
    
    private static ushort ReadMessageId(BinaryReader reader)
    {
        var messageIdBytes = reader.ReadBytes(2);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(messageIdBytes);
        }
        return BitConverter.ToUInt16(messageIdBytes, 0);
    }

    private static string ReadString(BinaryReader reader)
    {
        var stringBytes = new List<byte>();
        byte currentByte;
        while ((currentByte = reader.ReadByte()) != 0) // Assuming null-terminated strings
        {
            stringBytes.Add(currentByte);
        }
        return Encoding.ASCII.GetString(stringBytes.ToArray());
    }
}