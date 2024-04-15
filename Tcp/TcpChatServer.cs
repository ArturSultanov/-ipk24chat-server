using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using ipk24chat_server.Client;
using ipk24chat_server.Common;
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

    private async Task ProcessReceivedData(StringBuilder messageBuilder, TcpChatUser user)
    {
        string messageData = messageBuilder.ToString();
        int lastNewLineIndex = messageData.LastIndexOf("\r\n");
        
        if (lastNewLineIndex != -1)
        {
            string completeData = messageData.Substring(0, lastNewLineIndex);
            string[] messages = completeData.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var message in messages)
            {
                ClientMessageQueue.Queue.Enqueue(MessageToEnvelope(user, message));
                ClientMessageTracker.MessageReceived.TrySetResult(true);
            }

            // Preserve incomplete message for next read
            messageBuilder.Clear();
            if (lastNewLineIndex + 2 < messageData.Length)
            {
                messageBuilder.Append(messageData.Substring(lastNewLineIndex + 2));
            }
        }
    }


    

    private async Task ProcessClientMessagesAsync(ClientMessageEnvelope envelope, CancellationToken cancellationToken,
        Action requestCancel)
    {
        TcpChatUser user = (TcpChatUser)envelope.User;
        string message = envelope.Message;


        //     try
        //     {
        //         ChatMessage chatMessage = TcpPacker.Unpack(message);
        //         if (chatMessage.Type == ChatProtocol.MessageType.Auth)
        //         {
        //             AuthMessage authMessage = (AuthMessage)chatMessage;
        //             string username = authMessage.Username;
        //             TcpChatUser newUser = new TcpChatUser(user.TcpClient, authMessage.Username);
        //             newUser.DisplayName = authMessage.DisplayName;
        //             ChatUsers.ConnectedUsers.TryAdd(username, newUser);
        //         }
        //         else if (chatMessage.Type == ChatProtocol.MessageType.Msg)
        //         {
        //             MsgMessage msgMessage = (MsgMessage)chatMessage;
        //             string displayName = user.DisplayName;
        //             string messageContent = msgMessage.MessageContent;
        //             string response = $"MSG FROM {displayName} IS {messageContent}\r\n";
        //             await user.SendMessageAsync(response);
        //         }
        //         else if (chatMessage.Type == ChatProtocol.MessageType.Err)
        //         {
        //             ErrMessage errMessage = (ErrMessage)chatMessage;
        //             string displayName = user.DisplayName;
        //             string messageContent = errMessage.MessageContent;
        //             string response = $"ERR FROM {displayName} IS {messageContent}\r\n";
        //             await user.SendMessageAsync(response);
        //         }
        //         else if (chatMessage.Type == ChatProtocol.MessageType.Bye)
        //         {
        //             string response = "BYE\r\n";
        //             await user.SendMessageAsync(response);
        //             user.TcpClient.Close();
        //             ChatUsers.ConnectedUsers.TryRemove(user.ConnectionKey, out _);
        //         }
        //         else
        //         {
        //             // Send error message to the user
        //             string response = "ERR Unknown message type\r\n";
        //             await user.SendMessageAsync(response);
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine($"Error processing message: {e.Message}");
        //     }
        //     
        // }
        //



        // private async Task ListenClientAsync(TcpClient client)
        // {
        //     bool isAuthenticated = false;
        //     
        //     try
        //     {
        //         using (var stream = client.GetStream())
        //         {
        //             byte[] buffer = new byte[4096];
        //             StringBuilder messageBuilder = new StringBuilder();
        //             int bytesRead;
        //
        //             while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //             {
        //                 string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //                 messageBuilder.Append(receivedData);
        //
        //                 string currentData = messageBuilder.ToString();
        //                 int delimiterIndex;
        //
        //                 // Process all messages that are complete (ending with "\r\n")
        //                 while ((delimiterIndex = currentData.IndexOf("\r\n")) != -1)
        //                 {
        //                     string completeMessage = currentData.Substring(0, delimiterIndex);
        //                     currentData = currentData.Substring(delimiterIndex + 2);
        //                     messageBuilder = new StringBuilder(currentData); // reset messageBuilder with the remaining part
        //
        //                     // Process the complete message
        //                     ChatMessage message = TcpPacker.Unpack(completeMessage);
        //
        //
        //                     if (!isAuthenticated)
        //                     {
        //                         // User send an authentication message.
        //                         if (message.Type == ChatProtocol.MessageType.Auth)
        //                         {
        //                             AuthMessage authMessage = (AuthMessage)message;
        //                             string username = authMessage.Username;
        //                             TcpChatUser newUser = new TcpChatUser(client, authMessage.Username, authMessage.DisplayName);
        //
        //                             // Try to add the new user only if the username is not already in the dictionary
        //                             if (!ChatUsers.ConnectedUsers.TryAdd(username, newUser))
        //                             {
        //                                 Console.WriteLine($"Authentication failed: User {username} is already connected.");
        //                                 // Send !REPLY message to the client
        //                                 // Need to comment this in documentation
        //                             }
        //                             else
        //                             {
        //                                 isAuthenticated = true;
        //                                 Console.WriteLine($"User {username} authenticated successfully and added to connected users.");
        //                                 // Send REPLY message to the client
        //                             }
        //                         }
        //                         else
        //                         {
        //                             //send Error
        //                         }
        //                         
        //                     }
        //                     else  // The user is successfully authenticated.
        //                     {
        //                         if (message.Type == ChatProtocol.MessageType.Auth)
        //                         {
        //                             // Send ERR message to the client
        //                             // Close the connection.
        //                             continue;
        //                         }
        //                         else if (message.Type == ChatProtocol.MessageType.Msg)
        //                         {
        //                         
        //                         }
        //                         else if (message.Type == ChatProtocol.MessageType.Join)
        //                         {
        //                         
        //                         }
        //                         else if (message.Type == ChatProtocol.MessageType.Err)
        //                         {
        //                         
        //                         }
        //                         else if (message.Type == ChatProtocol.MessageType.Bye)
        //                         {
        //                         
        //                         }
        //                         else
        //                         {
        //                             // Send error to the user
        //                             // Waiting bye than 
        //                         }   
        //                     }
        //                     Console.WriteLine($"Processed message: {completeMessage}");
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine($"Error processing client messages: {e.Message}");
        //     }
        //     finally
        //     {
        //         client.Close();
        //         //ChatUsers.RemoveUser(user);
        //     }
        // }
        //
        // private async Task ProcessClientMessagesAsync()
        // {
        //     throw new NotImplementedException();
        // }

    }


    // HELPER METHODS
    private static string GenerateKeyFromTcpClient(TcpClient tcpClient)
    {
        return tcpClient.Client.RemoteEndPoint.ToString();
    }
    
    private ClientMessageEnvelope MessageToEnvelope(TcpChatUser user, string message)
    {
        return new ClientMessageEnvelope(user, message);
    }
    


}