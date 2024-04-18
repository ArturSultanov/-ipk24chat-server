using ipk24chat_server.Chat;

namespace ipk24chat_server.Client;

public class ClientMessageProcessor
{
    public async Task CheckMessageQueueAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ClientMessageEnvelope envelope = await Task.Run(() => ClientMessageQueue.Queue.Take(cancellationToken), cancellationToken);
                await ProcessClientMessageAsync(envelope, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Message processing was canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in message processing: {ex.Message}");
        }
    }

    private async Task ProcessClientMessageAsync(ClientMessageEnvelope envelope, CancellationToken cancellationToken)
    {
        if (!ChatUsers.TryGetUser(envelope.User.ConnectionEndPoint, out var user) || user == null)
        {
            // Just ignored the message if the user is not found. More efficient than cleaning when client disconnect.
            // This could be a case where the user disconnected before the message was processed.
            return;
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
            default:
                await HandleUnknownMessage(user, cancellationToken);
                break;
        }
    }

    // Processing the initial AUTH-messages
    private async Task HandleAuthMessage(AbstractChatUser user, AuthMessage authMessage, CancellationToken cancellationToken)
    {
        // Update the user details
        user.UpdateUserDetails(authMessage.Username, authMessage.DisplayName, authMessage.Secret);
        user.State = ClientState.State.Open;

        // Send the reply message
        ReplyMessage replyMessage = new ReplyMessage(1, "Successfully authenticated", authMessage.MessageId);
        
        try
        {
            await user.SendMessageAsync(replyMessage);
        }
        catch (Exception)
        {
            await user.ClientDisconnect();
            return;
        }

        
        var printMessage = new MsgMessage("Server", $"{user.DisplayName} has joined {user.ChannelId}");
        ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId, printMessage), cancellationToken);
    }

    // // Processing the text MSG-messages
    private Task HandleMsgMessage(AbstractChatUser user, MsgMessage msgMessage, CancellationToken cancellationToken)
    {
        if (msgMessage.DisplayName != user.DisplayName) user.DisplayName = msgMessage.DisplayName;
        ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId, msgMessage), cancellationToken);
        return Task.CompletedTask;
    }

    private async Task HandleJoinMessage(AbstractChatUser user, JoinMessage joinMessage, CancellationToken cancellationToken)
    {
        var leftChannelMessage = new MsgMessage("Server", $"{user.DisplayName} has left {user.ChannelId}");
        ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId, leftChannelMessage), cancellationToken);
                
        user.DisplayName = joinMessage.DisplayName;
        user.ChannelId = joinMessage.ChannelId;

        var joinedChannelMessage = new MsgMessage("Server", $"{user.DisplayName} has joined {user.ChannelId}");
        ChatMessagesQueue.Queue.Add(new ChatMessage(null, user.ChannelId, joinedChannelMessage), cancellationToken);

        ReplyMessage replyMessage = new ReplyMessage(1, $"Successfully joined {user.ChannelId}.", joinMessage.MessageId);
        await user.SendMessageAsync(replyMessage);
    }

    private async Task HandleErrMessage(AbstractChatUser user, CancellationToken cancellationToken)
    {
        await user.SendMessageAsync(new ByeMessage());
        await HandleByeMessage(user, cancellationToken);
    }

    private async Task HandleByeMessage(AbstractChatUser user, CancellationToken cancellationToken)
    {
        var leftChannelMessage = new MsgMessage("Server", $"{user.DisplayName} has left {user.ChannelId}");
        ChatMessagesQueue.Queue.Add(new ChatMessage(user, user.ChannelId, leftChannelMessage), cancellationToken);
        
        await user.ClientDisconnect();
    }

    private async Task HandleUnknownMessage(AbstractChatUser user, CancellationToken cancellationToken)
    {
        var errorMessage = new ErrMessage("Server", "Invalid message");
        await user.SendMessageAsync(errorMessage);
        await HandleByeMessage(user, cancellationToken);
    }
}
