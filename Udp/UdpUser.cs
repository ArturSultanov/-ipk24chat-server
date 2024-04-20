using System.Net;
using System.Collections.Generic;
using ipk24chat_server.Client;
using ipk24chat_server.Chat;

namespace ipk24chat_server.Udp;

public class UdpUser : AbstractChatUser
{
    private ushort _lastSentMessageId = 0;
    private ushort _lastReceivedMessageId = 0;
    private readonly HashSet<ushort> _receivedMessageIds = new HashSet<ushort>(); // Tracks received message IDs to handle duplicates
    private readonly object _lock = new object();  // Lock object for synchronization

    // Constructor
    public UdpUser(EndPoint endPoint) : base(endPoint){} // Passing key to the base class constructor

    public ushort LastReceivedMessageId
    {
        get
        {
            lock (_lock)
            {
                return _lastReceivedMessageId;
            }
        }
        set
        {
            lock (_lock)
            {
                _lastReceivedMessageId = value;
                _receivedMessageIds.Add(value);  // Add to received IDs set
            }
        }
    }

    public bool HasReceivedMessageId(ushort? messageId)
    {
        if (messageId != null)
        {
            lock (_lock)
            {
                return _receivedMessageIds.Contains((ushort)messageId);
            }
        }
        return false;

    }

    public override ushort? LastSentMessageId()
    {
        lock (_lock)
        {
            return ++_lastSentMessageId;  // Ensure thread-safe increment and retrieval
        }
    }

    public override async Task SendMessageAsync(ClientMessage message)
    {
        await Task.Delay(10);  // Simulate some operation
        // Implement message sending logic here
        throw new NotImplementedException();
    }

    public override async Task ClientDisconnect()
    {
        ChatUsers.RemoveUser(ConnectionEndPoint);
        ClientMessageQueue.TagUserMessages(this, "DISCONNECTED");  // Tag messages for cleanup
        
        await Task.Delay(10);  // Simulate some cleanup operation
    }
}