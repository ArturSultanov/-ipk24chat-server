using System.Net;
using ipk24chat_server.Common;

namespace ipk24chat_server.System
{
    public class ArgumentParser
    {
        public void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    switch (args[i])
                    {
                        case "-l" when i + 1 < args.Length:
                            if (!IPAddress.TryParse(args[++i], out var ip))
                                throw new ArgumentException("Invalid IP address format.");
                            ChatSettings.ServerIp = ip;
                            break;
                        case "-p" when i + 1 < args.Length:
                            if (!int.TryParse(args[++i], out var port) || port <= 0 || port > 65535)
                                throw new ArgumentException("Port must be between 1 and 65535.");
                            ChatSettings.ServerPort = port;
                            break;
                        case "-d" when i + 1 < args.Length:
                            if (!int.TryParse(args[++i], out var timeout) || timeout <= 0)
                                throw new ArgumentException("Timeout must be positive.");
                            ChatSettings.ConfirmationTimeout = timeout;
                            break;
                        case "-r" when i + 1 < args.Length:
                            if (!byte.TryParse(args[++i], out var retries))
                                throw new ArgumentException("Retries must be a non-negative integer.");
                            ChatSettings.RetransmissionCount = retries;
                            break;
                        case "-h":
                            PrintHelp();
                            Environment.Exit(0);
                            break;
                        default:
                            throw new ArgumentException("Invalid or incomplete arguments. Use -h for help.");
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw new ArgumentException("Expected value after " + args[i]);
                }
            }
        }

        private void PrintHelp()
        {
            const string helpMessage =
                "Usage: ipk24chat-server [-l <listening Ip>] [-p <listening port>] [-d <timeout>] [-r <retries>]\n" +
                "-l: Server listening IP address (default 0.0.0.0)\n" +
                "-p: Server port (default 4567)\n" +
                "-d: UDP confirmation timeout in milliseconds (default 250)\n" +
                "-r: Maximum number of UDP retransmissions (default 3)\n" +
                "-h: Displays this help message\n";
            
            Console.Write(helpMessage);
        }
    }
}
