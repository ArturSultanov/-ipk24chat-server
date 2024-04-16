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
                await Task.Run(() => PrintMessageToChat(message), cancellationToken);
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

    private Task PrintMessageToChat(ChatMessage message)
    {
        // Upon user successfully joining to a channel the server is required to "broadcast" a message to all users connected to the channel, with display name Server and content {DisplayName} has joined {ChannelID}..

        
        return Task.CompletedTask;
    }
    
}