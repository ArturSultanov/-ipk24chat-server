using ipk24chat_server.Common;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace ipk24chat_server.Tcp
{
    public class TcpChatUser : ChatUser
    {
        
        private readonly object _lock = new object();
        private string _username = string.Empty;
        private string _displayName = string.Empty;
        private string _channelId = string.Empty;
        private ChatState.State _state = ChatState.State.Auth;

        public TcpClient TcpClient { get; private set; }

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
        
        public ChatState.State State
        {
            get { lock (_lock) return _state; }
            set { lock (_lock) _state = value; }
        }

        public TcpChatUser(TcpClient tcpClient)
        {
            this.TcpClient = tcpClient;
        }

        // Constructor with initial user details from the authentication message
        public TcpChatUser(TcpClient tcpClient, string authMessageUsername, string authMessageDisplayName)
        {
            this.TcpClient = tcpClient;
            // Use the property setters to ensure thread safety
            this.Username = authMessageUsername;
            this.DisplayName = authMessageDisplayName;
        }

        // Example: Method to send a message to the user over TCP
        public async Task SendMessageAsync(string message)
        {
            if (TcpClient.Connected)
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
                await TcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }

}
