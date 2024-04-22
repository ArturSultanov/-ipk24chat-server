# IPK24-CHAT Server Documentation

IPK Project 2 - IOTA: Chat server.

## Overview

The IPK24-CHAT Server is a robust chat server application built using .NET 8. It is designed to facilitate real-time text communication between clients over the internet. The server is capable of handling custom `IPK24-CHAT` protocol messages and supports both TCP and UDP transport protocols, offering flexibility and reliability in client-server communication.

This application is designed with a focus on networking aspects, ensuring that multiple clients can connect and interact seamlessly in different chat channels. The server adheres to a defined finite state machine (FSM) for managing states and transitions based on client interactions, conforming to the custom protocol specifications.

### Features

- Supports concurrent handling of multiple client connections over TCP and UDP.
- Implements custom message types such as `AUTH`, `JOIN`, `MSG`, `ERR`, and `BYE` as per the `IPK24-CHAT` protocol.
- Ensures graceful session termination with appropriate use of FIN (TCP) and message confirmation (UDP).
- Allows for dynamic chat channels where clients can join, leave, and exchange messages.
- Includes command-line arguments for server configuration, including IP, port, and protocol-specific settings.
- Logs all incoming and outgoing messages in a consistent and readable format to the standard output stream.

### Built With

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0): A free, cross-platform, open-source developer platform for building many different types of applications.

## Getting Started

### Build

The project employs a Makefile for building, showcasing how .NET Core projects can integrate with traditional Unix-like build systems. Compilation is straightforward with the provided make command, adhering to .NET Core's build conventions

The program can be build using the `make` command.

```bash
make
```

### Usage
The following command will start the program:

```bash
./ipk24chat-server [-l <listening Ip>] [-p <listening port>] [-d <timeout>] [-r <retries>]
```

Run the server application using the generated executable. Here is an example of how to start the server with custom IP and port:

```bash
./ipk24chat-server -l 192.168.1.5 -p 4567
```

## Theoretical Background

Understanding the core differences between the Transmission Control Protocol (TCP) and the User Datagram Protocol (UDP) is essential for grasping network communication concepts. Both protocols serve the purpose of sending data over the internet, but they operate differently.

### Transmission Control Protocol (TCP)

TCP is a connection-oriented and reliable protocol. It guarantees the ordered delivery of data as it was sent. This reliability is achieved through a series of mechanisms:

- **Connection Establishment**: TCP establishes a connection using a handshake process, ensuring both parties are ready to communicate.
- **Data Sequencing**: Each byte of data sent over a TCP connection is numbered, which allows the receiver to reassemble data in the correct order.
- **Acknowledgments and Retransmissions**: Receivers send back acknowledgments for data received. If the sender does not receive an acknowledgment, it will retransmit the missing data.
- **Flow Control**: TCP uses windowing and other techniques to control how much data is sent and to prevent overwhelming the receiver.
- **Congestion Control**: Algorithms like TCP congestion control are used to detect and mitigate network congestion.

In TCP, the data stream is a sequence of bytes, which means that a read operation from a TCP socket could return:

- Just a part of the message if the entire message has not been received yet.
- Several messages at once if they are queued in the buffer, requiring the application-level protocol to determine the boundaries of each message.

### User Datagram Protocol (UDP)

In contrast, UDP is a connectionless and unreliable protocol. It does not establish a connection before sending data and does not guarantee message delivery. The key characteristics include:

- **No Connection Overhead**: UDP does not perform a handshake, making it faster for scenarios where speed is prioritized over reliability.
- **No Intrinsic Order**: Messages (datagrams) might arrive out of order, or not at all, and it's up to the application to handle these scenarios.
- **No Acknowledgments**: UDP does not have an acknowledgment mechanism for sent datagrams, and senders are unaware if their messages reach the destination.

UDP handles messages independently from one another:

- Every read from a UDP socket retrieves exactly one message as it was sent. The protocol ensures that messages are isolated; a read operation will never return a partial message or multiple messages at once.
- Because of its stateless nature, UDP is often used in streaming, gaming, and any application where the timely arrival of data is more crucial than its absolute reliability.

### Conclusion

The choice between TCP and UDP depends on the requirements of the application. TCP is suitable for applications that require high reliability, and where it's critical to receive complete and ordered data. In contrast, UDP is suitable for applications that can tolerate some loss of data but require faster and more efficient transmission, such as live audio or video streaming.

In the IPK24-CHAT server application, the implementation of both TCP and UDP allows the server to cater to clients that prioritize reliability (TCP) as well as those that require less overhead and potentially faster communication (UDP).

## Project Structure

The IPK24-CHAT Server project is organized into a set of folders and files that each play a specific role in the functionality of the application. Below is the hierarchy and description of the key components of the project:

### Root Directory

- `ipk24chat-server.csproj`: The C# project file containing build settings, dependencies, and other configurations for the .NET application.
- `LICENSE`: The license file specifying the terms under which the project's source code can be used, modified, and distributed.
- `Makefile`: Defines set of tasks to be executed. Used for building the project, cleaning build artifacts, and other command-line driven actions.
- `Program.cs`: Contains the entry point of the application. It initializes the server, sets up the necessary configurations, and starts the main loop to listen for client connections.
- `README.md`: The markdown file providing an overview, instructions, and documentation for the project.

### Chat Directory

Contains classes related to chat functionality:

- `AbstractChatUser.cs`: Defines an abstract class for chat users, which is the base for different user types such as TCP and UDP users.
- `ChatMessage.cs`: Represents a chat message within the application, containing message details like type, content, and sender information.
- `ChatMessagePrinter.cs`: Handles broadcasting messages to all connected users in a particular chat channel.
- `ChatMessagesQueue.cs`: Implements a queue system for chat messages waiting to be processed and sent.
- `ChatProtocol.cs`: Contains the protocol constants and message types for the IPK24-CHAT protocol.
- `ChatSettings.cs`: Manages settings and configurations related to the chat functionality.
- `ConnectedUsers.cs`: Tracks all the users currently connected to the server.

### Client Directory

Contains classes specific to client message handling:

- `ClientMessage.cs`: Defines the base class for different types of messages from clients.
- `ClientMessageEnvelope.cs`: Encapsulates a client message along with additional data, like the sender's information.
- `ClientMessageProcessor.cs`: Responsible for processing incoming client messages.
- `ClientMessageQueue.cs`: Manages a queue of messages from clients waiting to be processed.
- `ClientState.cs`: Enumerates the possible states a client connection can be in.

### System Directory

Includes utility classes for system-wide operations:

- `ArgumentParser.cs`: Parses and handles command-line arguments passed to the application.
- `Logger.cs`: Provides logging functionality for both incoming and outgoing messages.
- `UserInputHandler.cs`: Deals with user inputs from the console and allows for interactive server control.

### Tcp Directory

Contains the implementation for TCP protocol handling:

- `TcpPacker.cs`: Handles the packing and unpacking of TCP messages for transmission.
- `TcpServer.cs`: Manages TCP connections and the communication lifecycle for TCP clients.
- `TcpUser.cs`: Represents a user connected via TCP, extending the `AbstractChatUser`.

### Udp Directory

Contains the implementation for UDP protocol handling:

- `UdpPacker.cs`: Handles the packing and unpacking of UDP messages for transmission.
- `UdpServer.cs`: Manages UDP connections and communication for UDP clients.
- `UdpUser.cs`: Represents a user connected via UDP, also extending the `AbstractChatUser`.


## Classes and Their Interactions

This section outlines the key classes within the IPK24-CHAT Server application, detailing their responsibilities and how they communicate with each other to facilitate the server's operations.

### Class Diagram

The following diagram illustrates the relationships between the classes in the IPK24-CHAT Server application:

![Class Diagram](./Diagrams/class.svg)



## Testing:

### Telnet Check:
To check the server is running, I used the telnet command.

```bash
[xsulta01@ipk24 ~/test-client/ipk24chat-client]$ telnet 0.0.0.0 4567
Trying 0.0.0.0...
Connected to 0.0.0.0.
Escape character is '^]'.
```
As we can see, the server is running and accepting connections.

### Wireshack Check:
To check the server sends messages in the right `IPK24-CHAT` protocol format, I used the Wireshark tool.  
I tested program using `/ipk24-chat.lua.` lua extension the was provided by Instructors.



### Chat-client from project 1:

Also for testing the server, I used the chat-client from project 1.
In scenarios below I tried to demonstrate the TCP-client, UDP-client, and Server communication.

#### One user TCP Connection user:

The TCP-client input:

```bash
[xsulta01@ipk24 ~/test-client/ipk24chat-client]$ ./ipk24chat-client -t tcp -s localhost
Enter commands:
/auth user1 User1 secret 
Success: Successfully authenticated
Hello World!
/join 1
Success: Successfully joined 1.
Server: secret has joined 1
```

Server logs:

```bash
[xsulta01@ipk24 ~/RiderProjects/ipk24chat-server]$ ./ipk24chat-server 
RECV 127.0.0.1:35716 | AUTH
SENT 127.0.0.1:35716 | REPLY
RECV 127.0.0.1:35716 | MSG
RECV 127.0.0.1:35716 | JOIN
SENT 127.0.0.1:35716 | REPLY
SENT 127.0.0.1:35716 | MSG
```

#### One user UDP Connection user:

The UDP-client input:

```bash
[xsulta01@ipk24 ~/test-client/ipk24chat-client]$ ./ipk24chat-client -t udp -s localhost
Enter commands:
/auth user2 User2 secret 
Success: Successfully authenticated
Hello World!
/join 1
Success: Successfully joined 1.
Server: secret has joined 1
```

Server logs:

```bash
[xsulta01@ipk24 ~/RiderProjects/ipk24chat-server]$ ./ipk24chat-server 
SENT 127.0.0.1:37062 | CONFIRM
RECV 127.0.0.1:37062 | AUTH
SENT 127.0.0.1:37062 | REPLY
RECV 127.0.0.1:37062 | CONFIRM
RECV 127.0.0.1:37062 | MSG
SENT 127.0.0.1:37062 | CONFIRM
RECV 127.0.0.1:37062 | JOIN
SENT 127.0.0.1:37062 | CONFIRM
SENT 127.0.0.1:37062 | REPLY
RECV 127.0.0.1:37062 | CONFIRM
SENT 127.0.0.1:37062 | MSG
RECV 127.0.0.1:37062 | CONFIRM
```

#### TCP and UDP clients communication:

The TCP-client input:

```bash
[xsulta01@ipk24 ~/test-client/ipk24chat-client]$ ./ipk24chat-client -t tcp -s localhost
Enter commands:
/auth user1 password User1_Tom
Success: Successfully authenticated
Server: User2_Sam has joined default
Hi, Sam! How are you doing!
User2_Sam: Hi, Tom! I'am testing chat-server.
```

The UDP-client input:

```bash
[xsulta01@ipk24 ~/test-client/ipk24chat-client]$ ./ipk24chat-client -t udp -s localhost
Enter commands:
/auth user2 12345678 User2_Sam
Success: Successfully authenticated
User1_Tom: Hi, Sam! How are you doing!
Hi, Tom! I'am testing chat-server.      
```

Server logs:

```bash
[xsulta01@ipk24 ~/RiderProjects/ipk24chat-server]$ ./ipk24chat-server 
RECV 127.0.0.1:58976 | AUTH
SENT 127.0.0.1:58976 | REPLY
SENT 127.0.0.1:50161 | CONFIRM
RECV 127.0.0.1:50161 | AUTH
SENT 127.0.0.1:50161 | REPLY
RECV 127.0.0.1:50161 | CONFIRM
SENT 127.0.0.1:58976 | MSG
RECV 127.0.0.1:58976 | MSG
SENT 127.0.0.1:50161 | MSG
RECV 127.0.0.1:50161 | CONFIRM
RECV 127.0.0.1:50161 | MSG
SENT 127.0.0.1:50161 | CONFIRM
SENT 127.0.0.1:58976 | MSG
```

#### TCP and UDP clients communication in diffrent channels:
The TCP-client input:

```bash
/join 1
Success: Successfully joined 1.
Server: User1_Tom has joined 1
Server: User2_Sam has joined 1
User2_Sam: Oh, hi Tom! Did know that you at the channel 1 :)
Yeah, I am here
```
The UDP-client input:

```bash
Server: User1_Tom has left default
Tom, did you leave the chanel?
Okay :(
/join 1
Success: Successfully joined 1.
Server: User2_Sam has joined 1
Oh, hi Tom! Did know that you at the channel 1 :)
User1_Tom: Yeah, I am here
```

Server logs:

```bash
RECV 127.0.0.1:58976 | JOIN
SENT 127.0.0.1:50161 | MSG
SENT 127.0.0.1:58976 | REPLY
RECV 127.0.0.1:50161 | CONFIRM
SENT 127.0.0.1:58976 | MSG
RECV 127.0.0.1:50161 | MSG
SENT 127.0.0.1:50161 | CONFIRM
RECV 127.0.0.1:50161 | MSG
SENT 127.0.0.1:50161 | CONFIRM
RECV 127.0.0.1:50161 | JOIN
SENT 127.0.0.1:50161 | CONFIRM
SENT 127.0.0.1:50161 | MSG
SENT 127.0.0.1:50161 | REPLY
RECV 127.0.0.1:50161 | CONFIRM
SENT 127.0.0.1:50161 | MSG
RECV 127.0.0.1:50161 | CONFIRM
RECV 127.0.0.1:50161 | CONFIRM
SENT 127.0.0.1:50161 | REPLY
SENT 127.0.0.1:58976 | MSG
RECV 127.0.0.1:50161 | CONFIRM
RECV 127.0.0.1:50161 | MSG
SENT 127.0.0.1:50161 | CONFIRM
SENT 127.0.0.1:58976 | MSG
RECV 127.0.0.1:58976 | MSG
SENT 127.0.0.1:50161 | MSG
RECV 127.0.0.1:50161 | CONFIRM
```

#### Handling of the user leaving::
The TCP-client input:

```bash
User2_Sam: Sorry, Sam, have a thing to do... Bye!
Server: User2_Sam has left 1
```
The UDP-client input:

```bash
Sorry, Sam, have a thing to do... Bye!
[xsulta01@ipk24 ~/test-client/ipk24chat-client]$ 
```

Server logs:

```bash
RECV 127.0.0.1:50161 | MSG
SENT 127.0.0.1:50161 | CONFIRM
SENT 127.0.0.1:58976 | MSG
RECV 127.0.0.1:50161 | BYE
SENT 127.0.0.1:50161 | CONFIRM
```

### Chat Namespace

#### `AbstractChatUser`
- **Responsibilities**: Serves as a base class for users connected to the server. It contains common attributes and methods that TCP and UDP users share, such as sending and receiving messages and managing connection endpoints.
- **Communication**: Inherited by `TcpUser` and `UdpUser` to implement protocol-specific communication methods.

#### `ChatMessage`
- **Responsibilities**: Represents the structure of a chat message, holding information like the message type, sender, and content.
- **Communication**: Instances are created and used by `ChatMessagesQueue` for storing pending messages and by `ChatMessagePrinter` for broadcasting messages to users.

#### `ChatMessagePrinter`
- **Responsibilities**: Handles the distribution of chat messages to all connected users within a specific chat channel.
- **Communication**: Retrieves messages from `ChatMessagesQueue` and uses instances of `AbstractChatUser` to send these messages to clients.

#### `ChatMessagesQueue`
- **Responsibilities**: Implements a thread-safe queue to hold chat messages that are waiting to be broadcast to users.
- **Communication**: Provides queued messages to `ChatMessagePrinter` for processing and broadcasting.

#### `ChatProtocol`
- **Responsibilities**: Contains definitions of the `IPK24-CHAT` protocol constants, such as message types and protocol-specific settings.
- **Communication**: Utilized by various components to ensure consistent use of protocol values across the application.

#### `ChatSettings`
- **Responsibilities**: Stores configurable settings related to the chat functionality, such as server IP, port, and timeout values.
- **Communication**: Accessed by server initialization components to configure server behavior.

#### `ConnectedUsers`
- **Responsibilities**: Manages a list of all users currently connected to the chat server.
- **Communication**: Interacts with `TcpServer` and `UdpServer` to add or remove users upon connection or disconnection.

### Client Namespace

#### `ClientMessage`
- **Responsibilities**: Serves as the base class for different types of messages that can be sent by clients.
- **Communication**: Extended by specific message classes that represent different types of client messages.

#### `ClientMessageEnvelope`
- **Responsibilities**: Encapsulates a client message along with the user who sent it, for easier handling and processing.
- **Communication**: Passed between `ClientMessageQueue` and `ClientMessageProcessor` to associate messages with their senders.

#### `ClientMessageProcessor`
- **Responsibilities**: Responsible for dequeuing and processing messages from `ClientMessageQueue`, executing appropriate actions based on the message type.
- **Communication**: Communicates with `ChatMessagesQueue` to forward messages and `ConnectedUsers` to manage user states.

#### `ClientMessageQueue`
- **Responsibilities**: Manages a thread-safe queue of client messages that are awaiting processing.
- **Communication**: Enqueued messages are taken by `ClientMessageProcessor` for processing.

#### `ClientState`
- **Responsibilities**: Defines an enumeration of possible states for a client connection, such as authentication or active communication.
- **Communication**: Used by `TcpUser` and `UdpUser` to track the current state of the client connection.

### System Namespace

#### `ArgumentParser`
- **Responsibilities**: Parses command-line arguments provided at server startup and sets the corresponding `ChatSettings`.
- **Communication**: Interacts with the `Program` class to receive arguments and modifies `ChatSettings` based on the input.

#### `Logger`
- **Responsibilities**: Provides logging functionality, recording details of incoming and outgoing messages.
- **Communication**: Called by various components to log message transactions and server events.

#### `UserInputHandler`
- **Responsibilities**: Listens for and handles console input from the server administrator, allowing for real-time server control.
- **Communication**: Interacts with the main server loop within `Program` to enact user commands, such as shutting down the server.

### Tcp Namespace

#### `TcpPacker`
- **Responsibilities**: Handles the conversion (packing and unpacking) of TCP message data between raw byte streams and structured message objects.
- **Communication**: Used by `TcpServer` and `TcpUser` to serialize and deserialize messages sent over TCP.

#### `TcpServer`
- **Responsibilities**: Manages the lifecycle of TCP client connections, listening for new connections and handling data transmission.
- **Communication**: Creates and manages `TcpUser` instances for each connected TCP client.

#### `TcpUser`
- **Responsibilities**: Represents a TCP-connected client, handling message sending and reception specific to the TCP protocol.
- **Communication**: Inherits from `AbstractChatUser` and uses `TcpPacker` to handle message serialization.

### Udp Namespace

#### `UdpPacker`
- **Responsibilities**: Similar to `TcpPacker`, it is responsible for packing and unpacking UDP message data.
- **Communication**: Utilized by `UdpServer` and `UdpUser` to prepare messages for UDP transmission.

#### `UdpServer`
- **Responsibilities**: Handles UDP client communication, managing stateless message exchanges.
- **Communication**: Manages `UdpUser` instances for each client communicating over UDP.

#### `UdpUser`
- **Responsibilities**: Corresponds to a UDP-connected client, facilitating stateless message sending and reception.
- **Communication**: Inherits from `AbstractChatUser` and interfaces with `UdpPacker` for message handling.

## Bibliography

[NetworkProgramming.NET] "Network programming in .NET - .NET | Microsoft Learn." Microsoft. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/framework/network-programming.

[StevensTCP]: Stevens, W. Richard. *TCP/IP Illustrated, Volume 1: The Protocols*. Addison-Wesley, 1994. [Online]. Available at: https://www.r-5.org/files/books/computers/internals/net/Richard_Stevens-TCP-IP_Illustrated-EN.pdf

[TcpClientTcpListener] "Use TcpClient and TcpListener - .NET | Microsoft Learn." Microsoft. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/framework/network-programming/tcp-client-and-tcp-listener;.

[UsingUDP] "Using UDP Services - .NET Framework | Microsoft Learn." Microsoft. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/framework/network-programming/using-udp-services.

[UdpClientClass] "UdpClient Class (System.Net.Sockets) | Microsoft Learn." Microsoft. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.udpclient?view=netframework-4.8;.

[MicrosoftDocs] Microsoft. TCP/IP Client and Server [online]. Microsoft Docs. [cited 2023-04-30]. Available at: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/tcp-ip-client-and-server

[MicrosoftDocs] Microsoft. Using UDP Services [online]. Microsoft Docs. [cited 2023-04-30]. Available at: https://docs.microsoft.com/en-us/dotnet/framework/network-programming/using-udp-services

[RFC768] Postel, J. User Datagram Protocol [online]. August 1980. [cited 2023-04-30]. DOI: 10.17487/RFC768. Available at: https://datatracker.ietf.org/doc/html/rfc768

[RFC793] Postel, J. Transmission Control Protocol [online]. September 1981. [cited 2023-04-30]. DOI: 10.17487/RFC793. Available at: https://datatracker.ietf.org/doc/html/rfc793

