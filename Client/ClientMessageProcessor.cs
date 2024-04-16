using ipk24chat_server.Chat;
using ipk24chat_server.Tcp;
using ipk24chat_server.Udp;

namespace ipk24chat_server.Client;

public class ClientMessageProcessor
{
    
    // public async Task CheckMessageQueueAsync(CancellationToken cancellationToken, Action requestCancel)
    // {
    //     while (!cancellationToken.IsCancellationRequested)
    //     {
    //         ClientMessageEnvelope? envelope;
    //
    //         while (!ClientMessageQueue.Queue.TryDequeue(out envelope))
    //         {
    //             var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);
    //             var completedTask = await Task.WhenAny(ClientMessageTracker.MessageReceived.Task, cancellationTask);
    //
    //             if (completedTask == cancellationTask)
    //             {
    //                 if (!cancellationToken.IsCancellationRequested) requestCancel();
    //                 break;
    //             }
    //
    //             ClientMessageTracker.ResetMessageReceived();
    //         }
    //
    //         if (envelope != null)
    //         {
    //             // Processing message from user
    //             await ProcessClientMessageAsync(envelope, cancellationToken, requestCancel);
    //         }
    //     }
    // }
    
    public async Task CheckMessageQueueAsync(CancellationToken cancellationToken, Action requestCancel)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // This will block until an item is available or the cancellation is triggered
                    ClientMessageEnvelope envelope = ClientMessageQueue.Queue.Take(cancellationToken);
                    await ProcessClientMessageAsync(envelope, cancellationToken, requestCancel);
                }
                catch (OperationCanceledException)
                {
                    // Handle the cancellation of the blocking call gracefully
                    if (!cancellationToken.IsCancellationRequested) requestCancel(); // Invoke requestCancel to handle cleanup or final operations
                    Console.WriteLine("Message processing was canceled.");
                    break; // Exit the loop on cancellation
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in message processing: {ex.Message}");
            // Handle or log the exception as needed
        }
    }
    
    
    public async Task ProcessClientMessageAsync(ClientMessageEnvelope envelope, CancellationToken cancellationToken, Action requestCancel)
    {
        switch (envelope.User)
        {
            case TcpChatUser tcpUser:
                Console.WriteLine("Processing TCP user message");
                await ProcessTcpUserMessage(tcpUser, envelope.Message, cancellationToken);
                break;

            case UdpChatUser udpUser:
                Console.WriteLine("Processing UDP user message");
                await ProcessUdpUserMessage(udpUser, envelope.Message, cancellationToken);
                break;

            default:
                requestCancel.Invoke();
                Console.WriteLine("Unknown user type encountered.");
                break;
        }
    }

    private async Task ProcessTcpUserMessage(TcpChatUser user, ClientMessage message, CancellationToken cancellationToken)
    {
        if (ChatUsers.TryGetUser(user.ConnectionEndPoint, out _))
        {
            if (message is AuthMessage authMessage && user.State == ClientState.State.Start)
            {
                // TODO: check already existing users with the same login
                
                // Update the user info
                user.Username = authMessage.Username;
                user.DisplayName = authMessage.DisplayName;
                user.Secret = authMessage.Secret;
                user.State = ClientState.State.Open;
                
                // By default, any combination of a valid Username, DisplayName and Secret will be authenticated successfully.
                var positiveReply = new ReplyMessage
                {
                    Result = 1,
                    MessageContent = "Successfully authenticated"
                };
                _ = Task.Run(() => user.SendMessageAsync(positiveReply), cancellationToken);
                
                // Add the message to the queue to be printed in the chat
                var printMessage = new MsgMessage
                {
                    DisplayName = "Server",
                    MessageContent = $"{user.DisplayName} has joined {user.ChannelId}"
                };
                ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId ,printMessage), cancellationToken);
                
            }
            else if (message is MsgMessage msgMessage && user.State != ClientState.State.Start)
            {
                ChatMessagesQueue.Queue.Add(new ChatMessage(null, user.ChannelId ,msgMessage), cancellationToken);
            }
            else if(message is JoinMessage joinMessage && user.State != ClientState.State.Start)
            {
                var leftChannelMessage = new MsgMessage
                {
                    DisplayName = "Server",
                    MessageContent = $"{user.DisplayName} has left {user.ChannelId}"
                };
                ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId ,leftChannelMessage), cancellationToken);
                
                user.ChannelId = joinMessage.ChannelId;
                
                var joinedChannelMessage = new MsgMessage
                {
                    DisplayName = "Server",
                    MessageContent = $"{user.DisplayName} has joined {user.ChannelId}"
                };
                ChatMessagesQueue.Queue.Add(new ChatMessage(null, user.ChannelId ,joinedChannelMessage), cancellationToken);
                
                var positiveReply = new ReplyMessage
                {
                    Result = 1,
                    MessageContent = "Successfully authenticated"
                };
                _ = Task.Run(() => user.SendMessageAsync(positiveReply), cancellationToken);
            }
            else if (message is ErrMessage)
            {
                // user.State = ClientState.State.Error;
                _ = Task.Run(() => user.SendMessageAsync(new ByeMessage()), cancellationToken);
                ChatUsers.RemoveUser(user.ConnectionEndPoint);
                
                var leftChannelMessage = new MsgMessage
                {
                    DisplayName = "Server",
                    MessageContent = $"{user.DisplayName} has left {user.ChannelId}"
                };
                ChatMessagesQueue.Queue.Add(new ChatMessage(null, user.ChannelId ,leftChannelMessage), cancellationToken);
                user.TcpClient.Close();
            }
            else if (message is ByeMessage)
            {
                ChatUsers.RemoveUser(user.ConnectionEndPoint);
                var leftChannelMessage = new MsgMessage
                {
                    DisplayName = "Server",
                    MessageContent = $"{user.DisplayName} has left {user.ChannelId}"
                };
                ChatMessagesQueue.Queue.Add(new ChatMessage(null, user.ChannelId ,leftChannelMessage), cancellationToken);
                user.TcpClient.Close();
            }
            else
            {
                // Send error message, send bye and remove the user
                var errorMessage = new ErrMessage
                {
                    DisplayName = "Server",
                    MessageContent = "Invalid message"
                };
                _ = Task.Run(() => user.SendMessageAsync(errorMessage), cancellationToken);
                if (user.State != ClientState.State.Start)
                {
                    _ = Task.Run(() => user.SendMessageAsync(new ByeMessage()), cancellationToken);
                    ChatUsers.RemoveUser(user.ConnectionEndPoint);
                    
                    var leftChannelMessage = new MsgMessage
                    {
                        DisplayName = "Server",
                        MessageContent = $"{user.DisplayName} has left {user.ChannelId}"
                    };
                    ChatMessagesQueue.Queue.Add(new ChatMessage(null, user.ChannelId ,leftChannelMessage), cancellationToken);
                    
                }
                
                user.TcpClient.Close();
            }
            
        }
        else
        {
            
            Console.WriteLine("User not found in the connected users list.");
        }
        Console.WriteLine($"TCP Message Processed: {message}");
    }

    private async Task ProcessUdpUserMessage(UdpChatUser user, ClientMessage message, CancellationToken cancellationToken)
    {
        // Implement UDP-specific message processing logic
        Console.WriteLine($"UDP Message Processed: {message}");
    }
}