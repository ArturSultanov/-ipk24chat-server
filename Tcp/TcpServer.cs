using System.Net;
using System.Net.Sockets;
using System.Text;
using ipk24chat_server.Client;
using ipk24chat_server.Chat;
using ipk24chat_server.System;

namespace ipk24chat_server.Tcp;

public class TcpServer()
{
    
    private TcpListener _listener = new TcpListener(ChatSettings.ServerIp, ChatSettings.ServerPort);

    public async Task StartTcpServerAsync(CancellationToken cancellationToken, Action requestCancel)
    {
        _listener.Start();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for a client to connect
                var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);
                
                // Check if the client has a remote endpoint
                if (tcpClient.Client.RemoteEndPoint == null)
                {
                    // Console.WriteLine("Failed to obtain remote endpoint.");
                    tcpClient.Close();  // Ensure the client is properly closed to free up resources.
                    continue;  // Skip further processing and wait for the next connection.
                }
                
                // Create a new user object for the connected client
                var user = new TcpUser(tcpClient.Client.RemoteEndPoint, tcpClient);
                
                // Add the user to the connected users dictionary
                ConnectedUsers.AddUser(user.ConnectionEndPoint, user);
                
                // Handle the connection in a separate task
                _ = Task.Run(() => ListenClientAsync(user, cancellationToken), cancellationToken);
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e); 
        }
        finally
        {
            _listener.Stop();
        }
    }
    
    private async Task ListenClientAsync(TcpUser user, CancellationToken cancellationToken)
    {
        try
        {
            await using (var stream = user.TcpClient.GetStream())
            {
                byte[] buffer = new byte[4096];
                StringBuilder messageBuilder = new StringBuilder();
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0)
                    {
                        // Client has disconnected gracefully
                        break;
                    }

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(receivedData);

                    await ProcessReceivedData(messageBuilder, user);
                }
            }
        }
        finally
        {
            await user.ClientDisconnect(cancellationToken);
        }
    }

    private Task ProcessReceivedData(StringBuilder messageBuilder, TcpUser user)
    {
        string messageData = messageBuilder.ToString();
        int lastNewLineIndex = messageData.LastIndexOf("\r\n", StringComparison.Ordinal);

        
        if (lastNewLineIndex != -1)
        {
            string completeData = messageData.Substring(0, lastNewLineIndex);
            string[] messages = completeData.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var message in messages)
            {
                ClientMessage clientMessage = TcpPacker.Unpack(message);
                Logger.LogIo("RECV", user.ConnectionEndPoint.ToString(), clientMessage);
                // Further processing of received data
                ClientMessageQueue.Queue.Add(MessageToEnvelope(user, clientMessage));
            }

            // Preserve incomplete message for next read
            messageBuilder.Clear();
            if (lastNewLineIndex + 2 < messageData.Length)
            {
                messageBuilder.Append(messageData.Substring(lastNewLineIndex + 2));
            }
        }

        return Task.CompletedTask;
    }
    
    
    private ClientMessageEnvelope MessageToEnvelope(TcpUser user, ClientMessage message)
    {
        return new ClientMessageEnvelope(user, message);
    }

    public void Stop()
    {
        _listener.Stop();
    }
    
}