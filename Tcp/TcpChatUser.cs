using System.Net;
using System.Text;
using System.Net.Sockets;
using ipk24chat_server.Client;

namespace ipk24chat_server.Tcp
{
    // Represents the Tcp chat user
    public class TcpChatUser : Chat.AbstractChatUser
    {
        public TcpClient TcpClient { get; private set; }

        // Constructor
        public TcpChatUser(EndPoint endPoint, TcpClient tcpClient)
            : base(endPoint) // Passing key to the base class constructor
        {
            TcpClient = tcpClient;
        }

        override 
        public async Task SendMessageAsync(ClientMessage message)
        {
            if (TcpClient.Connected)
            {
                byte[] byteMessage = TcpPacker.Pack(message);
                await TcpClient.GetStream().WriteAsync(byteMessage, 0, byteMessage.Length);
            }
        }
    }
}