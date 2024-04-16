using ipk24chat_server.Client;

namespace ipk24chat_server.Chat;

public class ChatMessage(AbstractChatUser? ignoredUser, string channelId, MsgMessage message)
{
    // Readonly properties make the class immutable
    public AbstractChatUser? IgnoredUser { get; } = ignoredUser;
    public string ChannelId { get; } = channelId;
    public MsgMessage MsgMessage { get; } = message;
    
}
