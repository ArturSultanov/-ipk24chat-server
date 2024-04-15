using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using ipk24chat_server.Client;
using ipk24chat_server.Common;

namespace ipk24chat_server.Tcp
{
    // Represents the Tcp chat user
    public class TcpChatUser : Common.AbstractChatUser
    {
        private readonly object _lock = new object();
        public TcpClient TcpClient { get; private set; }
        private string _username = string.Empty;
        private string _displayName = string.Empty;
        private string _channelId = "default";
        private ClientState.State _state = ClientState.State.Start;

        public string Username
        {
            get { lock (_lock) return _username; }
            set { lock (_lock) _username = value; }
        }

        public string DisplayName
        {
            get { lock (_lock) return _displayName; }
            set { lock (_lock) _displayName = value; }
        }

        public string ChannelId
        {
            get { lock (_lock) return _channelId; }
            set { lock (_lock) _channelId = value; }
        }
        
        public ClientState.State State
        {
            get { lock (_lock) return _state; }
            set { lock (_lock) _state = value; }
        }

        // Constructor
        public TcpChatUser(string key, TcpClient tcpClient)
            : base(key) // Passing key to the base class constructor
        {
            TcpClient = tcpClient;
        }

        public async Task SendMessageAsync(string message)
        {
            if (TcpClient.Connected)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await TcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}