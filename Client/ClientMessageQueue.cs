using System.Collections.Concurrent;
using ipk24chat_server.Chat;

namespace ipk24chat_server.Client;

public static class ClientMessageQueue
{
    // Represents a queue of all incoming TCP and UDP clients messages, which are to be processed by the message processor.
    public static BlockingCollection<ClientMessageEnvelope> Queue = new BlockingCollection<ClientMessageEnvelope>(10000);
    
    /* This method is used to tag all messages from a user with a specific tag.
     * Used in user disconnect to mark all messages from that user as "DISCONNECTED".
     * This way, the message processor can ignore all messages from that user.
     * More efficient than cleaning message queue in loop when client disconnect.
     */ 
    public static void TagUserMessages(AbstractChatUser user, string tag)
    {
        foreach (var envelope in Queue)
        {
            if (envelope.User == user)
            {
                envelope.Tag = tag;
            }
        }
    }
}