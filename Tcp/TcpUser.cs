using System.Net;
using System.Net.Sockets;
using ipk24chat_server.Chat;
using ipk24chat_server.Client;
using ipk24chat_server.System;
namespace ipk24chat_server.Tcp;

/**
 * Represents a TCP chat user in the chat server environment.
 * This class encapsulates the functionality needed to manage a user's TCP connection,
 * handle message sending and receiving, and properly close the connection when needed.
 *
 * The class extends the AbstractChatUser, providing implementations for sending messages,
 * disconnecting the user, and additional utilities specific to TCP communication.
 *
 * TcpUser manages the network stream associated with a TCP client, ensuring that messages
 * are sent and that the connection is closed cleanly upon user disconnection. It also logs
 * message sending actions and handles exceptions that may occur during network operations.
 */
public class TcpUser : AbstractChatUser
{
    private CancellationToken _cancellationToken;
    public TcpClient TcpClient { get; private set; }

    // Constructor
    public TcpUser(EndPoint endPoint, TcpClient tcpClient, CancellationToken cancellationToken) : base(endPoint)
    {
        TcpClient = tcpClient;
        _cancellationToken = cancellationToken;
    }
    
    public override async Task SendMessageAsync(ClientMessage message)
    {
        try
        {
            if (TcpClient.Connected)
            {
                byte[] byteMessage = TcpPacker.Pack(message);
                await TcpClient.GetStream().WriteAsync(byteMessage, 0, byteMessage.Length);
                Logger.LogIo("SENT", ConnectionEndPoint.ToString(), message);
            }
        }
        catch (Exception)
        {
            await ClientDisconnect(cancellationToken: _cancellationToken);
        }
    }
    public override Task ClientDisconnect(CancellationToken cancellationToken)
    {
        try
        {
            if (TcpClient.Connected)
            {
                TcpClient.GetStream().Close();
                TcpClient.Close();
            }
            ConnectedUsers.RemoveUser(ConnectionEndPoint);
        }
        catch (ArgumentNullException )
        {
            // Exception can be causes by RemoveUser when the user is already removed, or had not been added.
            // Ignore exception if the client is already disconnected.
        }
        
        // Tag the user messages with "DISCONNECTED", so they can be ignored in the message queue.
        ClientMessageQueue.TagUserMessages(this, "DISCONNECTED");
        
        if (DisplayName != string.Empty && ChannelId != string.Empty && State != ClientState.State.Start && !cancellationToken.IsCancellationRequested)
        {
            var leftChannelMessage = new MsgMessage("Server", $"{DisplayName} has left {ChannelId}");
            ChatMessagesQueue.Queue.Add(new ChatMessage(this, ChannelId, leftChannelMessage), cancellationToken);
        }
        
        return Task.CompletedTask;
    }

    // TCP does not require message IDs, so this method always returns null.
    // The method is implemented to safe compatibility with the UDP messages in queue.
    public override ushort? GetMessageIdToSend()
    {
        return null;
    }
}
