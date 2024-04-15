using ipk24chat_server.Common;
using ipk24chat_server.Tcp;
using ipk24chat_server.Udp;

namespace ipk24chat_server.Client;

public class ClientMessageProcessor
{
    
    public async Task CheckMessageQueueAsync(CancellationToken cancellationToken, Action requestCancel)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ClientMessageEnvelope? envelope;

            while (!ClientMessageQueue.Queue.TryDequeue(out envelope))
            {
                var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);
                var completedTask = await Task.WhenAny(ClientMessageTracker.MessageReceived.Task, cancellationTask);

                if (completedTask == cancellationTask)
                {
                    if (!cancellationToken.IsCancellationRequested) requestCancel();
                    break;
                }

                ClientMessageTracker.ResetMessageReceived();
            }

            if (envelope != null)
            {
                // Processing message from user
                await ProcessClientMessageAsync(envelope, cancellationToken, requestCancel);
            }
        }
    }
    
    public async Task ProcessClientMessageAsync(ClientMessageEnvelope envelope, CancellationToken cancellationToken, Action requestCancel)
    {
        string message = envelope.Message;

        switch (envelope.User)
        {
            case TcpChatUser tcpUser:
                Console.WriteLine("Processing TCP user message");
                await ProcessTcpUserMessage(tcpUser, message, cancellationToken);
                break;

            case UdpChatUser udpUser:
                Console.WriteLine("Processing UDP user message");
                await ProcessUdpUserMessage(udpUser, message, cancellationToken);
                break;

            default:
                requestCancel.Invoke();
                Console.WriteLine("Unknown user type encountered.");
                break;
        }
    }

    private async Task ProcessTcpUserMessage(TcpChatUser user, string message, CancellationToken cancellationToken)
    {
        // Implement TCP-specific message processing logic
        Console.WriteLine($"TCP Message Processed: {message}");
    }

    private async Task ProcessUdpUserMessage(UdpChatUser user, string message, CancellationToken cancellationToken)
    {
        // Implement UDP-specific message processing logic
        Console.WriteLine($"UDP Message Processed: {message}");
    }
}