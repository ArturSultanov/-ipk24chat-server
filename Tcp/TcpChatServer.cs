using System.Net.Sockets;
using System.Text;
using ipk24chat_server.Common;
namespace ipk24chat_server.Tcp;

public class TcpChatServer
{
    
    private TcpListener _listener = new TcpListener(ChatSettings.ServerIp, ChatSettings.ServerPort);
    
    public async Task StartServerAsync(CancellationToken cancellationToken, Action requestCancel)
    {
        _listener.Start();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);
                // var user = new TcpChatUser(tcpClient);
                
                // Handle the connection in a separate task
                _ = Task.Run(() => HandleClientAsync(tcpClient));
                
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
    
    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[4096];
                StringBuilder messageBuilder = new StringBuilder();
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(receivedData);

                    string currentData = messageBuilder.ToString();
                    int delimiterIndex;

                    // Process all messages that are complete (ending with "\r\n")
                    while ((delimiterIndex = currentData.IndexOf("\r\n")) != -1)
                    {
                        string completeMessage = currentData.Substring(0, delimiterIndex);
                        currentData = currentData.Substring(delimiterIndex + 2);
                        messageBuilder = new StringBuilder(currentData); // reset messageBuilder with the remaining part

                        // Process the complete message
                        TcpMessage message = TcpPacker.Unpack(completeMessage);
                        
                        // User send an authentication message.
                        if (message.Type == ChatProtocol.MessageType.Auth)
                        {
                            TcpAuthMessage authMessage = (TcpAuthMessage)message;
                            string username = authMessage.Username;
                            TcpChatUser newUser = new TcpChatUser(client, authMessage.Username, authMessage.DisplayName);

                            // Try to add the new user only if the username is not already in the dictionary
                            if (!ChatUsers.ConnectedUsers.TryAdd(username, newUser))
                            {
                                Console.WriteLine($"Authentication failed: User {username} is already connected.");
                            }
                            else
                            {
                                Console.WriteLine($"User {username} authenticated successfully and added to connected users.");
                            }
                            
                            continue;
                        }

                        Console.WriteLine($"Processed message: {completeMessage}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error processing client messages: {e.Message}");
        }
        finally
        {
            client.Close();
            //ChatUsers.RemoveUser(user);
        }
    }

}