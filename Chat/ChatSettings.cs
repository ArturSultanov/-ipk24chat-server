using System.Net;

namespace ipk24chat_server.Common;

public static class ChatSettings
{
    // -l	0.0.0.0	IP address	Server listening IP address for welcome sockets
    // -p	4567	uint16	Server listening port for welcome sockets
    // -d	250	uint16	UDP confirmation timeout
    // -r	3	uint8	Maximum number of UDP retransmissions
    
    private static readonly object Lock = new object();
    
    private static int _confirmationTimeout = 250;
    private static byte _retransmissionCount = 3;
    private static int _serverPort = 4567;
    private static IPAddress _serverIp = IPAddress.Parse("0.0.0.0");
    
    // Properties for Server IP
    public static IPAddress ServerIp
    {
        get
        {
            lock (Lock)
            {
                return _serverIp;
            }
        }
        set
        {
            lock (Lock)
            {
                _serverIp = value;
            }
        }
    }

    // Properties for Server Port
    public static int ServerPort
    {
        get
        {
            lock (Lock)
            {
                return _serverPort;
            }
        }
        set
        {
            lock (Lock)
            {
                _serverPort = value;
            }
        }
    }

    // Properties for Confirmation Timeout
    public static int ConfirmationTimeout
    {
        get
        {
            lock (Lock)
            {
                return _confirmationTimeout;
            }
        }
        set
        {
            lock (Lock)
            {
                _confirmationTimeout = value;
            }
        }
    }

    // Properties for Retransmission Count
    public static byte RetransmissionCount
    {
        get
        {
            lock (Lock)
            {
                return _retransmissionCount;
            }
        }
        set
        {
            lock (Lock)
            {
                _retransmissionCount = value;
            }
        }
    }
}