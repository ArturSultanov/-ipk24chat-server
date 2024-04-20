using ipk24chat_server.Chat;
using ipk24chat_server.Client;
using ipk24chat_server.System;
using ipk24chat_server.Tcp;
using ipk24chat_server.Udp;

namespace ipk24chat_server;

internal class Program
{
    // public static async Task Main(string[] args)
    // {
    //     try
    //     {
    //         // Parse the command line arguments
    //         ArgumentParser parser = new ArgumentParser();
    //         parser.ParseArguments(args);
    //     }
    //     catch (ArgumentException ex)
    //     {
    //         // Handle argument-related errors
    //         Console.Error.WriteLine($"Error: {ex.Message}");
    //         Console.WriteLine("Use -h for help on how to use the program.");
    //         Environment.Exit(1); // Exit with a non-zero status to indicate an error
    //     }
    //     catch (Exception ex)
    //     {
    //         // Handle other unexpected errors
    //         Console.Error.WriteLine($"Unexpected error: {ex.Message}");
    //         Environment.Exit(99);
    //     }
    //     
    //     // CancellationTokenSource is used to cancel the asynchronous operation.
    //     CancellationTokenSource cts = new ();
    //     Console.CancelKeyPress += (_, e) =>
    //     {
    //         e.Cancel = true;
    //         cts.Cancel();
    //     };
    // }
    
    public static async Task Main(string[] args)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        TcpServer? tcpServer = null;
        UdpServer? udpServer = null;
        try
        {
            ArgumentParser parser = new ArgumentParser();
            parser.ParseArguments(args);

            tcpServer = new TcpServer();
            udpServer = new UdpServer();
            try
            {
                udpServer.ChatConnect();
            }
            catch (Exception)
            {
                throw;
            }

            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Shutdown signal received. Stopping server...");
            };

            Console.WriteLine("Starting TCP server...");
            Task serverTask = tcpServer.StartTcpServerAsync(cts.Token, () => cts.Cancel());
            // Add more tasks here.
            
            await serverTask; // Shoudl wait for all tasks to complete 
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Use -h for help on how to use the program.");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Environment.Exit(99);
        }
        finally
        {
            Console.WriteLine("Server has been stopped.");
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
        Console.WriteLine("Sending 'bye' messages to all connected users...");

        foreach (var user in ChatUsers.ConnectedUsers.Values)
        {
            try
            {
                await user.SendMessageAsync(new ByeMessage());
                await user.ClientDisconnect(cancellationToken); // Assuming this method closes the client connection gracefully
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERR: Failed to send 'bye' message to {user.Username}: {ex.Message}");
            }
        }
    }
}