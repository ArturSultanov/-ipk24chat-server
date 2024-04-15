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
        if (ChatUsers.TryGetUser(user.ConnectionKey, out _))
        {
            if (message is AuthMessage)
            {
                
            }
            else if (message is MsgMessage)
            {
                
            }
            else if (message is ErrMessage)
            {
                
            }
            else if (message is ByeMessage)
            {
                ChatUsers.RemoveUser(user.ConnectionKey);
                
            }
            else
            {
                // Send error message to the user
                string response = "ERR Unknown message type\r\n";
                await user.SendMessageAsync(new ErrMessage { DisplayName = "Server", MessageContent = response });
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