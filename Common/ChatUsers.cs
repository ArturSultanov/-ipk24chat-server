using System.Collections.Concurrent;

namespace ipk24chat_server.Common;
// public static class ChatUsers
// {
//     public static ConcurrentBag<ChatUser> ConnectedUsers = new ConcurrentBag<ChatUser>();
// }

public static class ChatUsers
{
    // Assuming each user has a unique identifier, like a username or GUID
    public static ConcurrentDictionary<string, ChatUser> ConnectedUsers = new ConcurrentDictionary<string, ChatUser>();

    // Adding a user
    public static void AddUser(string key, ChatUser user)
    {
        ConnectedUsers.TryAdd(key, user);
    }

    // Removing a user
    public static bool RemoveUser(string key)
    {
        return ConnectedUsers.TryRemove(key, out _);
    }

    // Example of how to get a user if needed
    public static bool TryGetUser(string key, out ChatUser user)
    {
        return ConnectedUsers.TryGetValue(key, out user);
    }
}
