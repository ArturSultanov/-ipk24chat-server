using System.Net.Sockets;
using System.Text;
using ipk24chat_server.Client;
using ipk24chat_server.Chat;
namespace ipk24chat_server.Tcp;

public class TcpChatServer
{
    
    private TcpListener _listener = new TcpListener(ChatSettings.ServerIp, ChatSettings.ServerPort);

    public async Task StartTcpServerAsync(CancellationToken cancellationToken, Action requestCancel)
    {
        _listener.Start();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);
                
                // Create a new user object for the connected client
                var user = new TcpChatUser(GenerateKeyFromTcpClient(tcpClient), tcpClient);
                
                // Add the user to the connected users dictionary
                ChatUsers.ConnectedUsers.TryAdd(user.ConnectionKey, user);
                
                // Handle the connection in a separate task
                _ = Task.Run(() => ListenClientAsync(user, cancellationToken));
                
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
    
    // Listen for messages from the client
    // private async Task ListenClientAsync(TcpChatUser user, CancellationToken cancellationToken)
    // {
    //     try
    //     {
    //         using (var stream = user.TcpClient.GetStream())
    //         {
    //             byte[] buffer = new byte[4096];
    //             StringBuilder messageBuilder = new StringBuilder();
    //             while (!cancellationToken.IsCancellationRequested)
    //             {
    //                 int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
    //                 if (bytesRead > 0)
    //                 {
    //                     // Translate data bytes to a ASCII string.
    //                     string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    //                     // Append the received data to the StringBuilder.
    //                     messageBuilder.Append(receivedData);
    //
    //                     string messageOneLine = messageBuilder.ToString();
    //                     // if the message ends with "\r\n" then it is a full message
    //                     if (messageOneLine.EndsWith("\r\n"))
    //                     {
    //                         string[] messages = messageOneLine.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
    //                         foreach (var message in messages)
    //                         {
    //                             _messageQueue.Enqueue(MessageToEnvelope(user.ConnectionKey, message));
    //                             TcpClientMessageTracker.MessageReceived.TrySetResult(true);
    //                         }
    //                         messageBuilder.Clear();
    //                     }
    //                     else
    //                     {
    //                         // When the message is not full, we need to split it by "\r\n" and process all full messages
    //                         string[] messages = messageOneLine.Split(new[] { "\r\n" }, StringSplitOptions.None);
    //                         if (messages.Length > 1)
    //                         {
    //                             for (int i = 0; i < messages.Length - 1; i++)
    //                             {
    //                                 _messageQueue.Enqueue(MessageToEnvelope(user.ConnectionKey, messages[i]));
    //                                 TcpClientMessageTracker.MessageReceived.TrySetResult(true);
    //                             }
    //                             messageBuilder.Clear();
    //                             messageBuilder.Append(messages[^1]);
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine($"Error in ListenForClientMessagesAsync: {e.Message}");
    //     }
    //     finally
    //     {
    //         user.TcpClient.Close();
    //     }
    //     
    // }
    
    // Check if there is any message in the queue to send to the client
    // Call the ProcessClientMessagesAsync method to process the messages
    
    
    private async Task ListenClientAsync(TcpChatUser user, CancellationToken cancellationToken)
    {
        try
        {
            using (var stream = user.TcpClient.GetStream())
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
        catch (IOException e)
        {
            Console.WriteLine($"Network error in ListenClientAsync: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in ListenClientAsync: {e.Message}");
        }
        finally
        {
            user.TcpClient.Close();
            // Removing the user from the dictionary
            ChatUsers.ConnectedUsers.TryRemove(user.ConnectionKey, out _);
        }
    }

    private Task ProcessReceivedData(StringBuilder messageBuilder, TcpChatUser user)
    {
        string messageData = messageBuilder.ToString();
        int lastNewLineIndex = messageData.LastIndexOf("\r\n");
        
        if (lastNewLineIndex != -1)
        {
            string completeData = messageData.Substring(0, lastNewLineIndex);
            string[] messages = completeData.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var message in messages)
            {
                ClientMessageQueue.Queue.Add(MessageToEnvelope(user, TcpPacker.Unpack(message)));
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
    
    private static string GenerateKeyFromTcpClient(TcpClient tcpClient)
    {
        return tcpClient.Client.RemoteEndPoint.ToString();
    }
    
    private ClientMessageEnvelope MessageToEnvelope(TcpChatUser user, ClientMessage message)
    {
        return new ClientMessageEnvelope(user, message);
    }
    
}