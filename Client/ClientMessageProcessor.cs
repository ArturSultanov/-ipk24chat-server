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
        // Check user state and type of message before processing
        if (!ChatUsers.TryGetUser(envelope.User.ConnectionEndPoint, out var user)|| user == null)
        {
            Console.WriteLine("User not found."); 
            return; // The user has been disconnected or removed.
        }

        switch (envelope.Message)
        {
            case AuthMessage authMessage when user.State == ClientState.State.Start:
                await HandleAuthMessage(user, authMessage, cancellationToken);
                break;
            case MsgMessage msgMessage when user.State == ClientState.State.Open:
                await HandleMsgMessage(user, msgMessage, cancellationToken);
                break;
            case JoinMessage joinMessage when user.State == ClientState.State.Open:
                await HandleJoinMessage(user, joinMessage, cancellationToken);
                break;
            case ErrMessage:
                await HandleErrMessage(user, cancellationToken);
                break;
            case ByeMessage:
                await HandleByeMessage(user, cancellationToken);
                break;
            // case ConfirmMessage confirmMessage:
            //     await HandleConfirmMessage(user);
            //     break;
            default:
                await HandleUnknownMessage(user, cancellationToken);
                break;
        }
    }

    private Task HandleAuthMessage(AbstractChatUser user, AuthMessage authMessage, CancellationToken cancellationToken)
    {
        user.Username = authMessage.Username;
        user.DisplayName = authMessage.DisplayName;
        user.Secret = authMessage.Secret;
        user.State = ClientState.State.Open;

        // TODO: check already existing users with the same login
        
        // Assuming all these methods are now within the user class
        _ = Task.Run(() => user.SendMessageAsync(new ReplyMessage(1, "Successfully authenticated")));
        // Add the message to the queue to be printed in the chat
        var printMessage = new MsgMessage("Server", $"{user.DisplayName} has joined {user.ChannelId}");
        ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId ,printMessage), cancellationToken);
        return Task.CompletedTask;
    }

    private Task HandleMsgMessage(AbstractChatUser user, MsgMessage msgMessage, CancellationToken cancellationToken)
    {
        ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId, msgMessage), cancellationToken);
        return Task.CompletedTask;
    }
    
    private Task HandleJoinMessage(AbstractChatUser user, JoinMessage joinMessage, CancellationToken cancellationToken)
    {
        // Create the left message to be print into chat channel
        var leftChannelMessage = new MsgMessage("Server", $"{user.DisplayName} has left {user.ChannelId}");
        ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId ,leftChannelMessage), cancellationToken);
                
        // Changing the channel for the user
        user.ChannelId = joinMessage.ChannelId;

        // Create the join message to be print into chat channel
        var joinedChannelMessage = new MsgMessage("Server", $"{user.DisplayName} has joined {user.ChannelId}");
        ChatMessagesQueue.Queue.Add(new ChatMessage(null, user.ChannelId ,joinedChannelMessage), cancellationToken);

        // Send positive reply to the user 
        _ = Task.Run(() => user.SendMessageAsync(new ReplyMessage(1, $"Successfully joined to the {user.ChannelId}.")));
        return Task.CompletedTask;
    }

    private Task HandleByeMessage(AbstractChatUser user, CancellationToken cancellationToken)
    {
        // Create the left message to be print into chat channel
        var leftChannelMessage = new MsgMessage("Server", $"{user.DisplayName} has left {user.ChannelId}");
        ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId ,leftChannelMessage), cancellationToken);
        
        // Disconnect the client from the server.
        _ = Task.Run(user.ClientDisconnect);
        return Task.CompletedTask;
    }
    
    private async Task HandleErrMessage(AbstractChatUser user, CancellationToken cancellationToken)
    {
        // Send Bye message to the user
        _ = Task.Run(() => user.SendMessageAsync(new ByeMessage()));
        
        // Same logic as in the ByeMessage
        await HandleByeMessage(user, cancellationToken);
    }
    
    private async Task HandleUnknownMessage(AbstractChatUser user, CancellationToken cancellationToken)
    {
        // Send error message, send bye and remove the user
        var errorMessage = new ErrMessage("Server", "Invalid message");
        _ = Task.Run(() => user.SendMessageAsync(errorMessage));
        
        await HandleByeMessage(user, cancellationToken);
    }
    
}