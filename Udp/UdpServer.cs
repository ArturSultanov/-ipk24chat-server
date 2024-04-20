using System.Net;
using System.Net.Sockets;
using ipk24chat_server.Chat;
using ipk24chat_server.Client;

namespace ipk24chat_server.Udp;

public class UdpServer
{
    private bool _stopServer = false; 
    
    private UdpClient _welcomeClient = new UdpClient();
    private UdpClient _communicationClient = new UdpClient();
    
    public void ChatConnect()
    {
        try
        {
            // Create UdpClient for welcome messages
            IPEndPoint welcomeEndPoint = new IPEndPoint(ChatSettings.ServerIp, ChatSettings.ServerPort);
            _welcomeClient.Client.Bind(welcomeEndPoint);
            
            // Create UdpClient for communication
            IPEndPoint communicationEndPoint = new IPEndPoint(ChatSettings.ServerIp, 0);
            _communicationClient.Client.Bind(communicationEndPoint);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        } 
    }

    public async Task StartUdpServerAsync(CancellationToken cancellationToken)
    {
        
        Task welcomeTask = HandleWelcomeClientsAsync(cancellationToken);
        Task communicationTask = HandleCommunicationClientsAsync(cancellationToken);

        await Task.WhenAll(welcomeTask, communicationTask);
    }
    
    private async Task HandleWelcomeClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            UdpReceiveResult result;
            try
            {
                result = await _welcomeClient.ReceiveAsync(cancellationToken);
            }
            catch (Exception)
            {
                continue;  // Skip to next iteration upon error
            }

            if (result.Buffer[0] != 0x00)  // If not a confirm message
            {
                byte[] confirmMessage = { 0x00, result.Buffer[1], result.Buffer[2] };
                await _welcomeClient.SendAsync(confirmMessage, confirmMessage.Length, result.RemoteEndPoint);
            }

            ClientMessage clientMessage = UdpPacker.Unpack(result.Buffer);
            AbstractChatUser? user;

            if (ChatUsers.TryGetUser(result.RemoteEndPoint, out user) && user != null)
            {
                if (user is UdpUser udpUser)
                {
                    if (udpUser.HasReceivedMessageId(clientMessage.MessageId))
                    {
                        continue;  // Ignore duplicated messages
                    }
                    // Update last received message ID if it's a new message
                    udpUser.LastReceivedMessageId = (ushort)clientMessage.MessageId!;
                }
                else
                {
                    var errMessage = new ErrMessage("Server", $"Non-UDP user at {result.RemoteEndPoint} sent a UDP message.");
                    await user.SendMessageAsync(errMessage);
                    continue;
                }
            }
            else
            {
                user = new UdpUser(result.RemoteEndPoint, _communicationClient, cancellationToken);
                ChatUsers.AddUser(user.ConnectionEndPoint, user);
                if (user is UdpUser newUser)
                {
                    newUser.LastReceivedMessageId = (ushort)clientMessage.MessageId!;
                }
            }

            if (clientMessage.Type != ChatProtocol.MessageType.Auth)
            {
                // Handle error state transition
                user.State = ClientState.State.Error;
            }
            
            ClientMessageQueue.Queue.Add(new ClientMessageEnvelope(user, clientMessage));
        }
    }


    private async Task HandleCommunicationClientsAsync(CancellationToken cancellationToken)
    {
        while (!_stopServer)
        {
            // var result = await _communicationClient.ReceiveAsync();
            // string receivedData = Encoding.UTF8.GetString(result.Buffer);
            // Console.WriteLine($"Received communication message: {receivedData} from {result.RemoteEndPoint}");
            //
            // // Handle message from existing users
            // if (ChatUsers.TryGetUser(result.RemoteEndPoint.ToString(), out var user))
            // {
            //     // Process message accordingly
            //     ClientMessage clientMessage = UdpPacker.Unpack(receivedData);
            //     ClientMessageQueue.Queue.Add(new ClientMessageEnvelope(user, clientMessage));
            // }
        }
    }
    
    public void Stop()
    {
        _stopServer = true;
    }
}