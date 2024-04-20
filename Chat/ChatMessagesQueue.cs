using System.Collections.Concurrent;
using ipk24chat_server.Client;

namespace ipk24chat_server.Chat;

// Represents a queue of chat messages, which are to be printed in the chat.
public static class ChatMessagesQueue
{
    public static readonly BlockingCollection<ChatMessage> Queue = new BlockingCollection<ChatMessage>(10000);
}