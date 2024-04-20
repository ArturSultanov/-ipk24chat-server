using System.Collections.Concurrent;
using System.Net;
namespace ipk24chat_server.Chat;

/*
 * This class is used to store all connected users in a thread-safe way.
 * It uses a ConcurrentDictionary to store the users, where the key is the EndPoint of the user and the value is the user object.
 * The class provides methods to add, remove, and get users from the dictionary.
 * The class is static, so it can be accessed from anywhere in the application.
 */
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
