namespace ipk24chat_server.Client;

public static class ClientState
{
    public enum State
    {
        Start,  // Waiting for user's /auth command, send to server --> WaitConfirm --> Auth
        Auth,   // Authentication has been sent, waiting for server answer
        Open,   // Authenticated, waiting for user's message
        Bye     // User has sent /bye command, waiting for server confirmation
    }
}