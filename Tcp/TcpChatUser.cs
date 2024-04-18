using System.Net;
using System.Text;
using System.Net.Sockets;
using ipk24chat_server.Chat;
using ipk24chat_server.Client;
using ipk24chat_server.System;

namespace ipk24chat_server.Tcp
{
    // Represents the Tcp chat user
    public class TcpChatUser : Chat.AbstractChatUser
    {
        public TcpClient TcpClient { get; private set; }

        // Constructor
        public TcpChatUser(EndPoint endPoint, TcpClient tcpClient) : base(endPoint)
        {
            TcpClient = tcpClient;
        }
        
        public override async Task SendMessageAsync(ClientMessage message)
        {
            if (TcpClient.Connected)
            {
                Logger.LogIo("SENT", ConnectionEndPoint.ToString(), message);
                byte[] byteMessage = TcpPacker.Pack(message);
                await TcpClient.GetStream().WriteAsync(byteMessage, 0, byteMessage.Length);
            }
        }
        public override Task ClientDisconnect()
        {
            try
            {
                // Remove user from connected user list
                ChatUsers.RemoveUser(ConnectionEndPoint);
                
                // Shut down the connection gracefully.
                if (TcpClient.Connected)
                {
                    TcpClient.GetStream().Close(); // Close the network stream
                    TcpClient.Close(); // Close the TcpClient
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error closing TcpClient: {e.Message}");
            }

            return Task.CompletedTask;
        }
    }
}