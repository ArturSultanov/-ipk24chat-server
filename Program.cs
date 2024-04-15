using ipk24chat_server.Common;
using ipk24chat_server.System; // Ensure this namespace matches where ChatArgumentParser is located

namespace ipk24chat_server;

internal class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // Parse the command line arguments
            ArgumentParser parser = new ArgumentParser();
            parser.ParseArguments(args);
        }
        catch (ArgumentException ex)
        {
            // Handle argument-related errors
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Use -h for help on how to use the program.");
            Environment.Exit(1); // Exit with a non-zero status to indicate an error
        }
        catch (Exception ex)
        {
            // Handle other unexpected errors
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Environment.Exit(99);
        }
        
        // CancellationTokenSource is used to cancel the asynchronous operation.
        CancellationTokenSource cts = new ();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        
        
        
        
        
        
        
    }
}