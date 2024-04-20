using System.Net;
using System.Net.Sockets;
using ipk24chat_server.Chat;
using ipk24chat_server.Client;

namespace ipk24chat_server.Udp;

public class UdpServer
{
    private bool _stopServer; 
    
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
        Task _ = Task.Run(() => HandleCommunicationClientsAsync(cancellationToken));

        await welcomeTask;
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
            
            // Creating a temp user object to check if the user is already connected

            if (ConnectedUsers.TryGetUser(result.RemoteEndPoint, out var user) && user != null)
            {  // Existing user
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
            else  // New user
            {
                user = new UdpUser(result.RemoteEndPoint, _communicationClient, cancellationToken);
                ConnectedUsers.AddUser(user.ConnectionEndPoint, user);
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
            UdpReceiveResult result;
            try
            {
                result = await _communicationClient.ReceiveAsync();
            }
            catch (Exception)
            {
                continue;  // Skip to next iteration upon error
            }

            // Send a confirmation for non-confirm messages
            if (result.Buffer[0] != ChatProtocol.MessageType.Confirm)
            {
                byte[] confirmMessage = { ChatProtocol.MessageType.Confirm, result.Buffer[1], result.Buffer[2] };
                await _communicationClient.SendAsync(confirmMessage, confirmMessage.Length, result.RemoteEndPoint);
            }

            ClientMessage clientMessage = UdpPacker.Unpack(result.Buffer);
            AbstractChatUser? user;

            if (ConnectedUsers.TryGetUser(result.RemoteEndPoint, out user) && user is UdpUser udpUser)
            {
                // Correct user type and user found
                if (clientMessage.Type == ChatProtocol.MessageType.Confirm)
                {
                    udpUser.ConfirmCollection.Add(new ConfirmMessage((ushort)clientMessage.MessageId!));
                }
                else
                {
                    if (udpUser.HasReceivedMessageId(clientMessage.MessageId))
                    {
                        continue;  // Ignore duplicated messages
                    }
                    udpUser.LastReceivedMessageId = (ushort)clientMessage.MessageId!;
                }
            }
            else
            {
                // Handle non-UDP users or users not found
                if (user != null)
                {
                    var errMessage = new ErrMessage("Server", $"Non-UDP user at {result.RemoteEndPoint} sent a UDP message.");
                    await user.SendMessageAsync(errMessage);
                    continue;
                }
                else
                {
                    // User was not found - handle as a new user for error message sending
                    var tempUser = new UdpUser(result.RemoteEndPoint, _communicationClient, cancellationToken);
                    var errMessage = new ErrMessage("Server", "Invalid or unauthenticated UDP user.");
                    await tempUser.SendMessageAsync(errMessage);
                    continue;
                }
            }

            // Further handling for authenticated users
            ClientMessageQueue.Queue.Add(new ClientMessageEnvelope(user, clientMessage));
        }
    }

    public void Stop()
    {
        _stopServer = true;
        _welcomeClient.Close();
        _communicationClient.Close();
    }
}