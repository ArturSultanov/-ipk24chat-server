using System.Collections.Concurrent;
using ipk24chat_server.Client;

namespace ipk24chat_server.Chat;

public static class ChatMessagesQueue
{
    public static BlockingCollection<ChatMessage> Queue = new BlockingCollection<ChatMessage>(10000);
}