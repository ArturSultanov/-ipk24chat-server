using ipk24chat_server.Client;

namespace ipk24chat_server.Chat;

public class ChatMessagePrinter
{
    
    public async Task PrintMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Take will block if there are no items in the collection
                var message = ChatMessagesQueue.Queue.Take(cancellationToken);
                await PrintMessageToChat(message, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
            Console.WriteLine("Message processing was canceled.");
        }
        finally
        {
            // Release resources
            ChatMessagesQueue.Queue.Dispose();
        }
    }

    private async Task PrintMessageToChat(ChatMessage message, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();
        foreach (var user in ChatUsers.ConnectedUsers.Values)
        {
            if (user != message.IgnoredUser && user.State == ClientState.State.Open && user.ChannelId == message.ChannelId)
            {
                tasks.Add(SendMessageSafelyAsync(user, message.MsgMessage, cancellationToken));
            }
        }
        await Task.WhenAll(tasks);
    }
    private async Task SendMessageSafelyAsync(AbstractChatUser user, MsgMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await user.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to send message to {user.Username}: {ex.Message}");
            user.State = ClientState.State.Error;
            
            // Disconnect the client, don't need to send bye, because the client is unreachable
            await user.ClientDisconnect(cancellationToken); 
        }
    }
}