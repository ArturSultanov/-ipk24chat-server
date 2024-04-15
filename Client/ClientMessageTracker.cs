namespace ipk24chat_server.Client;

public class ClientMessageTracker
{
    private static TaskCompletionSource<bool> _messageReceived = new TaskCompletionSource<bool>();

    public static TaskCompletionSource<bool> MessageReceived
    {
        get => _messageReceived;
        set => _messageReceived = value;
    }

    // A method to reset the TaskCompletionSource for a new wait
    
    public static void ResetMessageReceived()
    {
        _messageReceived = new TaskCompletionSource<bool>();
    }
}