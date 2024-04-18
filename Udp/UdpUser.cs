using System.Net;
using ipk24chat_server.Client;
using ipk24chat_server.Chat;

namespace ipk24chat_server.Udp;

public class UdpUser : AbstractChatUser
{
    // last message id from client
    // last message id from server
    // list of confirm waiting messages

    // Constructor
    public UdpUser(EndPoint endPoint) : base(endPoint){} // Passing key to the base class constructor

    private ushort _lastMessageId = 0;
    
    override 
    public async Task SendMessageAsync(ClientMessage message)
    {
        await Task.Delay(10);
        throw new NotImplementedException();
    }

    public override async Task ClientDisconnect()
    {
        await Task.Delay(10);
        throw new NotImplementedException();
    }

    public override ushort? LastMessageId()
    {
        return _lastMessageId++;
    }
}