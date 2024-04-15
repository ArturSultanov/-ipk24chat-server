using System.Collections.Concurrent;

namespace ipk24chat_server.Client;

public static class ClientMessageQueue
{
    // public static ConcurrentQueue<ClientMessageEnvelope> Queue = new ConcurrentQueue<ClientMessageEnvelope>();
    public static BlockingCollection<ClientMessageEnvelope> Queue = new BlockingCollection<ClientMessageEnvelope>();
}