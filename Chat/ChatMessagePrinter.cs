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
                var envelope = ChatMessagesQueue.Queue.Take(cancellationToken);
                await Task.Run(() => PrintMessageToChat(envelope), cancellationToken);
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

    private Task PrintMessageToChat(ClientMessageEnvelope envelope)
    {
        // Upon user successfully joining to a channel the server is required to "broadcast" a message to all users connected to the channel, with display name Server and content {DisplayName} has joined {ChannelID}..
        var message = envelope.Message;
        throw new NotImplementedException();
        
    }
    private void PrintAuthMessage(ClientMessageEnvelope envelope) // All
    {
        // Upon user successfully joining to a channel the server is required to "broadcast" a message to all users connected to the channel, with display name Server and content {DisplayName} has joined {ChannelID}..
        throw new NotImplementedException();
    }
    
    private void PrintJoinMessage(ClientMessageEnvelope envelope) // All
    {
        // Upon user successfully joining to a channel the server is required to "broadcast" a message to all users connected to the channel, with display name Server and content {DisplayName} has joined {ChannelID}..
        throw new NotImplementedException();
    }
    
    private void PrintTextMessage(ClientMessageEnvelope envelope) // Except for the original sender
    {
        // Upon user sending a message to a channel the server is required to "broadcast" a message to all users connected to the channel, with display name {DisplayName} and content {Content}..
        throw new NotImplementedException();
    }
    
    private void PrintLeaveMessage(ClientMessageEnvelope envelope) // Except for the original sender
    {
        // Upon user successfully leaving a channel the server is required to "broadcast" a message to all users connected to the channel, with display name Server and content {DisplayName} has left {ChannelID}..
        throw new NotImplementedException();
    }
    
}