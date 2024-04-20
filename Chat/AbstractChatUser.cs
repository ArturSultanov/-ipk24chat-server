using System.Net;
using ipk24chat_server.Client;

namespace ipk24chat_server.Chat;

/*
 * Represents the abstract chat user
 * Extend this abstract class in TcpUser and UdpUser classes
 */
public abstract class AbstractChatUser(EndPoint endPoint)
{
    private readonly object _lock = new object();
    public EndPoint ConnectionEndPoint { get; private set; } = endPoint;    // Unique identifier for the user by user Endpoint
    
    private string _username = string.Empty;                    // Login of the user
    private string _secret = string.Empty;                      // Password of the user
    
    private string _displayName = string.Empty;                 // Display name of the user printing at the chat
    private string _channelId = "default";                      // Channel ID of the user
    private ClientState.State _state = ClientState.State.Start; // Current state of the user
    

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

    public string Secret
    {
        get { lock (_lock) return _secret; }
        set { lock (_lock) _secret = value; }
    }
    
    public abstract Task SendMessageAsync(ClientMessage message); // Abstract method to send message to the user
    public abstract Task ClientDisconnect(CancellationToken cancellationToken); // Abstract method for client disconnect
    public abstract ushort? GetMessageIdToSend();    // Abstract method to get the message id (UDP only, null for TCP)

    public void UpdateUserDetails(string username, string displayName, string secret)
    {
        Username = username;
        DisplayName = displayName;
        Secret = secret;
    }
}