using System.Net;
using System.Net.Sockets;
using ipk24chat_server.Chat;
using ipk24chat_server.Client;
using ipk24chat_server.System;

namespace ipk24chat_server.Udp;

/**
 * The UdpServer class manages UDP connections for a chat server.
 * It handles two main tasks: managing initial connections through the welcome client and
 * managing ongoing communication through the communication client.
 *
 * This server supports handling initial handshake messages to authenticate and register new clients
 * and then switches these clients to a communication-specific handling process that listens for
 * and sends messages continuously until the server stops.
 *
 * It uses separate tasks to handle welcome messages and communication messages to ensure that
 * client interactions are processed in real-time and efficiently, without blocking operations impacting
 * the user experience.
 */
public class UdpServer
{
    private bool _stopServer; 
    
    private UdpClient _welcomeClient = new UdpClient();
    private UdpClient _communicationClient = new UdpClient();
    
    /*
     * Initializes the UDP server by binding the welcome and communication clients to specific endpoints.
     */
    public void ChatConnect()
    {
        // Create UdpClient for welcome messages
        IPEndPoint welcomeEndPoint = new IPEndPoint(ChatSettings.ServerIp, ChatSettings.ServerPort);
        _welcomeClient.Client.Bind(welcomeEndPoint);
            
        // Create UdpClient for communication
        IPEndPoint communicationEndPoint = new IPEndPoint(ChatSettings.ServerIp, 0);
        _communicationClient.Client.Bind(communicationEndPoint);
    }

    /*
     * Starts the UDP server by initiating the handling of welcome and communication clients asynchronously.
     */
    public async Task StartUdpServerAsync(CancellationToken cancellationToken)
    {
        
        Task welcomeTask = HandleWelcomeClientsAsync(cancellationToken);
        Task _ = Task.Run(() => HandleCommunicationClientsAsync(cancellationToken));

        await welcomeTask;
    }
    
    /*
     * Handles incoming messages on the welcome client. Validates and processes new connections or messages from known users
     */
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

            if (result.Buffer[0] != 0x00) // Non-confirm message, then send a confirmation
            {
                byte[] confirmMessage = { 0x00, result.Buffer[1], result.Buffer[2] };
                await _welcomeClient.SendAsync(confirmMessage, confirmMessage.Length, result.RemoteEndPoint);
            }

            ClientMessage clientMessage = UdpPacker.Unpack(result.Buffer);
            Logger.LogIo("RECV", result.RemoteEndPoint.ToString(), clientMessage);

            if (ConnectedUsers.TryGetUser(result.RemoteEndPoint, out var user) && user != null)
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
            else  // User with same EndPoint was not found, create a new user
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
    
    /*
     * Handles communication with existing UDP clients, including processing of confirmations and other messages.
     */
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
                continue;
            }

            // Send a confirmation for non-confirm messages
            if (result.Buffer[0] != ChatProtocol.MessageType.Confirm)
            {
                byte[] confirmMessage = { ChatProtocol.MessageType.Confirm, result.Buffer[1], result.Buffer[2] };
                await _communicationClient.SendAsync(confirmMessage, confirmMessage.Length, result.RemoteEndPoint);
                Logger.LogIo("SENT", result.RemoteEndPoint.ToString(), UdpPacker.Unpack(confirmMessage));
            }

            ClientMessage clientMessage = UdpPacker.Unpack(result.Buffer);
            Logger.LogIo("RECV", result.RemoteEndPoint.ToString(), clientMessage);
            AbstractChatUser? user;

            if (ConnectedUsers.TryGetUser(result.RemoteEndPoint, out user) && user is UdpUser udpUser)
            {
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
                    // User was not found - handle as a temp user for error message sending
                    var tempUser = new UdpUser(result.RemoteEndPoint, _communicationClient, cancellationToken);
                    var errMessage = new ErrMessage("Server", "Invalid or unauthenticated UDP user.");
                    await tempUser.SendMessageAsync(errMessage);
                    await tempUser.ClientDisconnect(cancellationToken);
                    continue;
                }
            }

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