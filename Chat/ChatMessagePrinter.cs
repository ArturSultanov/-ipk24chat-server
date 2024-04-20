using ipk24chat_server.Client;

namespace ipk24chat_server.Chat;

/*
 * Represents a message printer, which prints messages to the particular Channel in the chat.
 * The printer is responsible for sending messages to all connected users in the channel.
 */
public class ChatMessagePrinter
{
    
    // Print messages from the queue to the chat.
    public async Task PrintMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = ChatMessagesQueue.Queue.Take(cancellationToken);
                await PrintMessageToChat(message);
            }
        }
        finally
        {
            // Release resources
            ChatMessagesQueue.Queue.Dispose();
        }
    }

    private async Task PrintMessageToChat(ChatMessage message)
    {
        // Create a list of tasks to send the message to all connected users in the channel
        var tasks = new List<Task>();
        foreach (var user in ConnectedUsers.UsersDict.Values)
        {
            if (user != message.IgnoredUser && user.State == ClientState.State.Open && user.ChannelId == message.ChannelId)
            {
                tasks.Add(user.SendMessageAsync(message.MsgMessage));
            }
        }
        await Task.WhenAll(tasks);
    }
}