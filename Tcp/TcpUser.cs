using System.Net;
using System.Net.Sockets;
using ipk24chat_server.Chat;
using ipk24chat_server.Client;
using ipk24chat_server.System;

namespace ipk24chat_server.Tcp
{
    // Represents the Tcp chat user
    public class TcpUser : AbstractChatUser
    {
        public TcpClient TcpClient { get; private set; }

        // Constructor
        public TcpUser(EndPoint endPoint, TcpClient tcpClient) : base(endPoint)
        {
            TcpClient = tcpClient;
        }
        
        public override async Task SendMessageAsync(ClientMessage message)
        {
            try
            {
                if (TcpClient.Connected)
                {
                    Logger.LogIo("SENT", ConnectionEndPoint.ToString(), message);
                    byte[] byteMessage = TcpPacker.Pack(message);
                    await TcpClient.GetStream().WriteAsync(byteMessage, 0, byteMessage.Length);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error sending message to {ConnectionEndPoint}: {e.Message}");
                throw;
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
            
            if (DisplayName != string.Empty && ChannelId != string.Empty && !cancellationToken.IsCancellationRequested)
            {
                var leftChannelMessage = new MsgMessage("Server", $"{DisplayName} has left {ChannelId}");
                ChatMessagesQueue.Queue.Add(new ChatMessage(this, ChannelId, leftChannelMessage), cancellationToken);
            }
            
            return Task.CompletedTask;
        }

        
        public override ushort? GetMessageIdToSend()
        {
            return null;
        }
    }
}