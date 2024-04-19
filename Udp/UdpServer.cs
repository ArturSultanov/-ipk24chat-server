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
            // Receive the initial AUTH-message from client
            UdpReceiveResult result;
            try
            {
                result =  await _welcomeClient.ReceiveAsync(cancellationToken);
            }
            catch (Exception)
            {
                continue;
            }
            
            // Send the conformation message to the client
            if (result.Buffer[0] != 0x00) // If it's not confirm message
            {
                // Construct and send confirm response
                byte[] confirmMessage = new byte[3];
                confirmMessage[0] = 0x00; // Confirm message type
                confirmMessage[1] = result.Buffer[1]; // Copy the second byte from the received message
                confirmMessage[2] = result.Buffer[2]; // Copy the third byte from the received message
                    
                _welcomeClient.Client.SendTo(confirmMessage, SocketFlags.None, result.RemoteEndPoint);
            }

            // Unpack the received message, convert it to the ClientMessage object
            ClientMessage clientMessage = UdpPacker.Unpack(result.Buffer);
            
            // Check if the user already has been connected:
            if (ChatUsers.TryGetUser(result.RemoteEndPoint, out AbstractChatUser? existingUser))
            {
                if (existingUser != null)
                {
                    // Check if the type of the received message is AUTH
                    if (clientMessage.Type == ChatProtocol.MessageType.Auth)
                    {
                        if (existingUser is UdpUser udpUser)
                        {
                            // If the client did not receive the previous COMMIT message.
                            if (udpUser.HasReceivedMessageId(clientMessage.MessageId))
                            {
                                continue;
                            }
                            else // User tried to send the AUTH message again.
                            {
                                existingUser.State = ClientState.State.Error;
                                // Create and send the error message to the client
                                var errMessage = new ErrMessage("Server", "User has been already connected.");
                                await udpUser.SendMessageAsync(errMessage);
                                // Send the BYE message
                                await udpUser.SendMessageAsync(new ByeMessage());
                                // Disconnect the user
                                await udpUser.ClientDisconnect();
                                continue;
                            }
                        }
                        else
                        {
                            // Edge case, when some TcpUser exists at the EndPoint, but server has received the UDP-message.
                            var errMessage = new ErrMessage("Server", $"User from {result.RemoteEndPoint} is TCP connected, but send the UDP message.");
                            await existingUser.SendMessageAsync(errMessage);
                            continue;
                        } 
                    }
                    else // The received message is not AUTH
                    {
                        // User tried to send the AUTH message again.
                        existingUser.State = ClientState.State.Error;
                        // Create and send the error message to the client
                        var errMessage = new ErrMessage("Server", "User is already connected.");
                        await existingUser.SendMessageAsync(errMessage);
                        // Send the BYE message
                        await existingUser.SendMessageAsync(new ByeMessage());
                        // Disconnect the user
                        await existingUser.ClientDisconnect();
                        continue;
                    }
                }
                else
                {
                    // Remove the null-user from the connected users
                    ChatUsers.RemoveUser(result.RemoteEndPoint);
                }
            }
            
            // Create a new user and add to connected users
            UdpUser newUser = new UdpUser(result.RemoteEndPoint);
            // Add the user to the connected users
            ChatUsers.AddUser(newUser.ConnectionEndPoint, newUser);
            // Update the Received MessageId hashset
            newUser.LastReceivedMessageId = (ushort)clientMessage.MessageId!;
            // Add the message to the message queue
            ClientMessageQueue.Queue.Add(new ClientMessageEnvelope(newUser, clientMessage));
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
    
    
}