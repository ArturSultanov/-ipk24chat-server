using System.Collections.Concurrent;
using ipk24chat_server.Chat;

namespace ipk24chat_server.Client;

public static class ClientMessageQueue
{
    // public static ConcurrentQueue<ClientMessageEnvelope> Queue = new ConcurrentQueue<ClientMessageEnvelope>();
    public static BlockingCollection<ClientMessageEnvelope> Queue = new BlockingCollection<ClientMessageEnvelope>(1000);
    
}