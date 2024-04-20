namespace ipk24chat_server.Client;

public static class ClientState
{
    public enum State
    {
        Start,  // Client is waiting to be authenticated
        Open,   // Client is authenticated, can send a user's message
        Error,  // Error has occurred, Error message has been sent to the client
        End     // Client is disconnected
    }
}