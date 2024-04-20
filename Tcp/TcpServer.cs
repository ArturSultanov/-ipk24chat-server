using System.Net.Sockets;
using System.Text;
using ipk24chat_server.Client;
using ipk24chat_server.Chat;
using ipk24chat_server.System;
namespace ipk24chat_server.Tcp;

/*
 * Represents a TCP server that handles incoming TCP connections, manages user sessions,
 * and processes incoming data from connected clients. This class encapsulates all aspects
 * of network management, including listening for incoming connections, handling client
 * data reception, and safely closing connections. It utilizes asynchronous programming to
 * manage multiple client connections efficiently and to maintain responsive server operations.
 *
 * Each connected client is handled in a separate asynchronous task, allowing the server to
 * serve multiple clients concurrently. The server operates continuously under a cancellation
 * policy provided by the CancellationToken.
 */
public class TcpServer
{
    
    private TcpListener _listener = new(ChatSettings.ServerIp, ChatSettings.ServerPort);

    /*
     * Starts the TCP server to listen for incoming client connections and handle them asynchronously.
     * Continues to accept and process clients until the cancellation token is requested.
     */
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
                    tcpClient.Close();  // Ensure the client is properly closed to free up resources.
                    continue;  // Skip further processing and wait for the next connection.
                }
                
                // Create a new user object for the connected client
                var user = new TcpUser(tcpClient.Client.RemoteEndPoint, tcpClient, cancellationToken);
                
                // Add the user to the connected users dictionary
                ConnectedUsers.AddUser(user.ConnectionEndPoint, user);
                
                // Handle the connection in a separate task
                _ = Task.Run(() => ListenClientAsync(user, cancellationToken), cancellationToken);
                
            }
        }
        finally
        {
            _listener.Stop();
        }
    }
    
    /*
     * Listens for messages from a connected TCP client and processes received data.
     * Continues to listen and process data until the connection is closed or a cancellation is requested.
     */
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

    /*
     * Processes the data received from a client, extracting and handling complete messages.
     */
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