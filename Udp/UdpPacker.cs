using System.Text;
using ipk24chat_server.Chat;
using ipk24chat_server.Client;

namespace ipk24chat_server.Udp;

public class UdpPacker
{
    /*
     * Pack a ClientMessage object into bytes to be sent to the client, according to the IPK24-chat protocol.
     * The message is packed according to the message type.
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
    
    /*
     * Unpack a byte array received from the UDP client into a ClientMessage object, according to the IPK24-chat protocol.
     * The message is unpacked according to the message type.
     * If the message is invalid, an UnknownMessage object is returned.
     */
    public static ClientMessage Unpack(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        var type = reader.ReadByte();
        var messageId = ReadMessageId(reader);

        try
        {
            switch (type)
            {
                case ChatProtocol.MessageType.Auth:
                    return ParseAuthMessage(reader, messageId);

                case ChatProtocol.MessageType.Confirm:
                    return new ConfirmMessage(messageId);

                case ChatProtocol.MessageType.Msg:
                    return ParseMsgMessage(reader, messageId);

                case ChatProtocol.MessageType.Err:
                    return ParseErrMessage(reader, messageId);
                
                case ChatProtocol.MessageType.Bye:
                    return ParseByeMessage(messageId);
                
                case ChatProtocol.MessageType.Join:
                    return ParseJoinMessage(reader, messageId);
                
                default:
                    return new UnknownMessage();
                //throw new NotSupportedException($"Unsupported message type: {type}");
            }
        }
        catch (EndOfStreamException)
        {
            return new UnknownMessage();
        }
    }
    
    private static ClientMessage ParseAuthMessage(BinaryReader reader, ushort messageId)
    {
        var username = ReadString(reader, ChatProtocol.MaxUsernameLength);
        var displayName = ReadString(reader, ChatProtocol.MaxDisplayNameLength);
        var secret = ReadString(reader, ChatProtocol.MaxSecretLength);
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(secret))
        {
            return new UnknownMessage(); // Invalid AuthMessage due to missing or excessively long fields
        }
        return new AuthMessage(username, displayName, secret) { MessageId = messageId };
    } 

    private static ClientMessage ParseMsgMessage(BinaryReader reader, ushort messageId)
    {
        var displayName = ReadString(reader, ChatProtocol.MaxDisplayNameLength);
        var messageContent = ReadString(reader, ChatProtocol.MaxMessageContentLength);
        
        if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(messageContent))
        {
            return new UnknownMessage(); // Invalid AuthMessage due to missing or excessively long fields
        }
        return new MsgMessage(displayName, messageContent) { MessageId = messageId };
    }
    
    private static ClientMessage ParseErrMessage(BinaryReader reader, ushort messageId)
    {
        var displayName = ReadString(reader, ChatProtocol.MaxDisplayNameLength);
        var messageContent = ReadString(reader, ChatProtocol.MaxMessageContentLength);
        
        if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(messageContent))
        {
            return new UnknownMessage(); // Invalid AuthMessage due to missing or excessively long fields
        }
        return new ErrMessage(displayName, messageContent) { MessageId = messageId };
    }
    
    private static ClientMessage ParseJoinMessage(BinaryReader reader, ushort messageId)
    {
        var channelId = ReadString(reader, ChatProtocol.MaxChannelIdLength);
        var displayName = ReadString(reader, ChatProtocol.MaxDisplayNameLength);
        if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(displayName))
        {
            return new UnknownMessage(); // Return an error or unknown message type if validation fails
        }
        return new JoinMessage(channelId, displayName) { MessageId = messageId };
    }

    
    private static ClientMessage ParseByeMessage(ushort messageId)
    {
        return new ByeMessage() { MessageId = messageId };
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

    private static string ReadString(BinaryReader reader, int maxLength)
    {
        var stringBytes = new List<byte>();
        byte currentByte;
        while ((currentByte = reader.ReadByte()) != 0)
        {
            stringBytes.Add(currentByte);
            if (stringBytes.Count > maxLength) throw new EndOfStreamException("String exceeds maximum allowed length.");
        }
        return Encoding.ASCII.GetString(stringBytes.ToArray());
    }
}