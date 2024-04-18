using ipk24chat_server.Client;

namespace ipk24chat_server.System;

public static class Logger
{
    public static void LogIo(string direction, string? endPoint, ClientMessage message)
    {
        string type = message switch
        {
            AuthMessage => "Auth",
            JoinMessage => "Join",
            MsgMessage => "Msg",
            ConfirmMessage => "Confirm",
            _ => "Unknown"
        };
        Console.WriteLine($"{direction} {endPoint} | {type}");
    }
}