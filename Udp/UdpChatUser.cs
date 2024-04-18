using System.Net;
using ipk24chat_server.Client;
using ipk24chat_server.Chat;

namespace ipk24chat_server.Udp;

public class UdpChatUser : Chat.AbstractChatUser
{


    // Constructor
    public UdpChatUser(EndPoint endPoint)
        : base(endPoint) // Passing key to the base class constructor
    {
        
    }
    
    override 
    public async Task SendMessageAsync(ClientMessage message)
    {
        throw new NotImplementedException();
    }

    public override async Task ClientDisconnect()
    {
        throw new NotImplementedException();
    }
}