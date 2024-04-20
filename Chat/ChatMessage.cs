using ipk24chat_server.Client;
namespace ipk24chat_server.Chat;

/*
 * Represents a message that can be printed in the chat
 *
 * @param ignoredUser The user to be ignored when sending this message, if any.
 * @param channelId The ID of the channel where this message will be posted.
 * @param message The message content encapsulated in a MsgMessage object.
 */
public class ChatMessage(AbstractChatUser? ignoredUser, string channelId, MsgMessage message)
{
    public AbstractChatUser? IgnoredUser { get; } = ignoredUser;
    public string ChannelId { get; } = channelId;
    public MsgMessage MsgMessage { get; } = message;
    
}
