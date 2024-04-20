using System.Collections.Concurrent;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using ipk24chat_server.Client;
using ipk24chat_server.Chat;
using ipk24chat_server.System;
namespace ipk24chat_server.Udp;

/*
 * UdpUser is a class that represents a user connected to the server via UDP.
 * It is used to handle the user's connection, messages, and disconnection.
 * UdpUser inherits from AbstractChatUser and implements the SendMessageAsync and ClientDisconnect methods.
 * UdpUser uses a UdpClient to send and receive messages.
 * It also uses a BlockingCollection to store confirm messages and a HashSet to track received message IDs.
 */
public class UdpUser : AbstractChatUser
{
    private readonly UdpClient _udpClient;
    public BlockingCollection<ConfirmMessage> ConfirmCollection = new BlockingCollection<ConfirmMessage>(10000);
    private ushort _lastSentMessageId = 0;
    private ushort _lastReceivedMessageId = 0;
    private readonly HashSet<ushort> _receivedMessageIds = new HashSet<ushort>(); // Tracks received message IDs to handle duplicates
    private readonly object _lock = new object();  // Lock object for synchronization
    private CancellationToken _cancellationToken;
    
    // Constructor
    public UdpUser(EndPoint endPoint, UdpClient udpClient, CancellationToken cancellationToken) : base(endPoint)
    {
        _udpClient = udpClient;
        _cancellationToken = cancellationToken;
    }

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

    public override ushort? GetMessageIdToSend()
    {
        lock (_lock)
        {
            return _lastSentMessageId++;  // Ensure thread-safe increment and retrieval
        }
    }

    public override async Task SendMessageAsync(ClientMessage message)
    {
        if (!await SendMessageWithConfirmationAsync(message))
        {
            await ClientDisconnect(cancellationToken: _cancellationToken);
        }
    }

    private async Task<bool> SendMessageWithConfirmationAsync(ClientMessage message)
    {
        var currentMessageId = GetMessageIdToSend();
        byte[] dataToSend = UdpPacker.Pack(message, currentMessageId);
        int attempts = 0;

        while (attempts <= ChatSettings.RetransmissionCount)
        {
            try
            {
                // Assuming ConnectionEndPoint is an IPEndPoint or cast it appropriately
                IPEndPoint? destination = ConnectionEndPoint as IPEndPoint;
                if (destination == null)
                {
                    break;  // Exit loop if endpoint is not correctly specified
                }
                
                await _udpClient.SendAsync(dataToSend, dataToSend.Length, destination);
                Logger.LogIo("SENT", ConnectionEndPoint.ToString(), message);
                
                if (await WaitForConfirmation(currentMessageId))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                await Task.Delay(ChatSettings.ConfirmationTimeout);
            }

            attempts++;
        }

        return false;
    }


    private Task<bool> WaitForConfirmation(ushort? messageId)
    {
        try
        {
            if (ConfirmCollection.TryTake(out var confirmMessage, TimeSpan.FromMilliseconds(ChatSettings.ConfirmationTimeout)))
            {
                if (confirmMessage.MessageId == messageId)
                {
                    return Task.FromResult(true);
                }
            }
        }
        catch (Exception)
        {
            // Ignore exception, just return false
            return Task.FromResult(false);
        }
        return Task.FromResult(false);
    }
    public override Task ClientDisconnect(CancellationToken cancellationToken)
    {
        ConnectedUsers.RemoveUser(ConnectionEndPoint);
        ClientMessageQueue.TagUserMessages(this, "DISCONNECTED");  // Tag messages for cleanup
        
        if (DisplayName != string.Empty && ChannelId != string.Empty && State != ClientState.State.Start && !cancellationToken.IsCancellationRequested)
        {
            var leftChannelMessage = new MsgMessage("Server", $"{DisplayName} has left {ChannelId}");
            ChatMessagesQueue.Queue.Add(new ChatMessage(this, ChannelId, leftChannelMessage), cancellationToken);
        }
        
        return Task.CompletedTask;
    }
}