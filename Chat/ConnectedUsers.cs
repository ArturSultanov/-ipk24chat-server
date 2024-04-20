using System.Collections.Concurrent;
using System.Net;

namespace ipk24chat_server.Chat;

public static class ConnectedUsers
{
    // Assuming each user has a unique identifier, like a username or GUID
    public static readonly ConcurrentDictionary<EndPoint, AbstractChatUser> UsersDict = new ConcurrentDictionary<EndPoint, AbstractChatUser>();
    
    // Adding a user
    public static void AddUser(EndPoint endPoint, AbstractChatUser user)
    {
        UsersDict.TryAdd(endPoint, user);
    }

    // Removing a user
    public static bool RemoveUser(EndPoint endPoint)
    {
        try
        {
            return UsersDict.TryRemove(endPoint, out _);
        }
        catch (Exception)
        {
            return true;
        }
        
    }

    // Example of how to get a user if needed
    public static bool TryGetUser(EndPoint endPoint, out AbstractChatUser? user)
    {
        return UsersDict.TryGetValue(endPoint, out user);
    }
    
}
