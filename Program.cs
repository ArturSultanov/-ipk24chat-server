using ipk24chat_server.Chat;
using ipk24chat_server.Client;
using ipk24chat_server.System;
using ipk24chat_server.Tcp;
using ipk24chat_server.Udp;

namespace ipk24chat_server;

internal class Program
{
    public static async Task Main(string[] args)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        TcpServer? tcpServer = null;
        UdpServer? udpServer = null;
        try
        {
            ArgumentParser parser = new ArgumentParser();
            parser.ParseArguments(args);

            var chatMessagePrinter = new ChatMessagePrinter();
            var messageProcessor = new ClientMessageProcessor();
            var userInputHandlernew = new UserInputHandler();
            tcpServer = new TcpServer();
            udpServer = new UdpServer();
            udpServer.ChatConnect(); // Need to bind the ports before starting the server

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Task _ = Task.Run(() => userInputHandlernew.StartListeningForCommandsAsync(cts.Token, () => cts.Cancel()));
            Task messageProcessorTask = messageProcessor.ProcessMessagesAsync(cts.Token);
            Task chatMessagePrinterTask = chatMessagePrinter.PrintMessagesAsync(cts.Token);
            Task tcpServerTask = tcpServer.StartTcpServerAsync(cts.Token, () => cts.Cancel());
            Task udpServerTask = udpServer.StartUdpServerAsync(cts.Token);
            await Task.WhenAll(tcpServerTask, udpServerTask, messageProcessorTask, chatMessagePrinterTask);
        }
        catch (ArgumentException ex)
        {
            await Console.Error.WriteLineAsync($"ERR: {ex.Message}");
            Console.WriteLine("Use -h for help on how to use the program.");
            Environment.Exit(0);
        }
        catch (OperationCanceledException)
        {
            // Ignore the exception based on the requirements of graceful shutdown when using Ctrl+C or EOF
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"ERR: {ex.Message}");
            Environment.Exit(99);
        }
        finally
        {
            await SendByeToAllUsers(cancellationToken: cts.Token);
            
            // Perform final cleanup before exiting
            if (tcpServer != null)
            {
                tcpServer.Stop(); // Assuming a Stop method that closes the listener
            }

            if (udpServer!= null)
            {
                udpServer.Stop();
            }
        }
    }

    private static async Task SendByeToAllUsers(CancellationToken cancellationToken)
    {
        foreach (var user in ConnectedUsers.UsersDict.Values)
        {
            try
            {
                await user.SendMessageAsync(new ByeMessage());
                await user.ClientDisconnect(cancellationToken); // Assuming this method closes the client connection gracefully
            }
            catch (Exception)
            {
                // Ignore exceptions because server is shutting down
            }
        }
    }
}