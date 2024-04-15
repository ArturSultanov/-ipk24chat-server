namespace ipk24chat_server.Chat;

public static class ChatProtocol
{
    // public const int DefaultServerPort = 4567;
    // public const string Charset = "us-ascii";

    // Message types represented as byte values for easy parsing and construction
    public static class MessageType
    {
        public const byte Confirm = 0x00;
        public const byte Reply = 0x01;
        public const byte Auth = 0x02;
        public const byte Join = 0x03;
        public const byte Msg = 0x04;
        public const byte Err = 0xFE;
        public const byte Bye = 0xFF;
    }

    // Other protocol-specific values
    public const int MaxUsernameLength = 20;
    public const int MaxChannelIdLength = 20;
    public const int MaxSecretLength = 128;
    public const int MaxDisplayNameLength = 20;
    public const int MaxMessageContentLength = 1400;
}