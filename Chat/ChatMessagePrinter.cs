using ipk24chat_server.Client;

namespace ipk24chat_server.Common;

public class ChatMessagePrinter
{
    public void PrintMessageToChat(ClientMessageEnvelope envelope)
    {
        // Upon user successfully joining to a channel the server is required to "broadcast" a message to all users connected to the channel, with display name Server and content {DisplayName} has joined {ChannelID}..
        throw new NotImplementedException();
    }
    
    private void PrintJoinMessage(ClientMessageEnvelope envelope)
    {
        // Upon user successfully joining to a channel the server is required to "broadcast" a message to all users connected to the channel, with display name Server and content {DisplayName} has joined {ChannelID}..
        throw new NotImplementedException();
    }
}