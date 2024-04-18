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
            // receive welcome message from client
            var result = await _welcomeClient.ReceiveAsync(cancellationToken);
            

            // Check if user already exists
            if (!ChatUsers.TryGetUser(result.RemoteEndPoint, out var existingUser))
            {
                // Create a new user and add to connected users
                var newUser = new UdpUser(result.RemoteEndPoint);
                ChatUsers.AddUser(newUser.ConnectionEndPoint, newUser);
            }

            // Add the client message to the ClientMessageQueue
            ClientMessage clientMessage = UdpPacker.Unpack(receivedData); // Assuming TCP Packer works here, or you might need UDP-specific unpacking
            ClientMessageQueue.Queue.Add(new ClientMessageEnvelope(existingUser ?? newUser, clientMessage));
        }
    }

    private async Task HandleCommunicationClientsAsync(CancellationToken cancellationToken)
    {
        while (!_stopServer)
        {
            var result = await _communicationClient.ReceiveAsync();
            string receivedData = Encoding.UTF8.GetString(result.Buffer);
            Console.WriteLine($"Received communication message: {receivedData} from {result.RemoteEndPoint}");

            // Handle message from existing users
            if (ChatUsers.TryGetUser(result.RemoteEndPoint.ToString(), out var user))
            {
                // Process message accordingly
                ClientMessage clientMessage = UdpPacker.Unpack(receivedData);
                ClientMessageQueue.Queue.Add(new ClientMessageEnvelope(user, clientMessage));
            }
        }
    }
    
    
}