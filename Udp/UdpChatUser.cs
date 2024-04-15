using System.Net;
using ipk24chat_server.Client;
using ipk24chat_server.Common;

namespace ipk24chat_server.Udp;

public class UdpChatUser : Common.AbstractChatUser
{
    private readonly object _lock = new object();
    public EndPoint UdpEndPoint { get; private set; }
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
    public UdpChatUser(EndPoint udpEndPoint, string key)
        : base(key) // Passing key to the base class constructor
    {
        UdpEndPoint = udpEndPoint;
    }
}