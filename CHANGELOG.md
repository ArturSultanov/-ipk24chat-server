# Implemented Functionality and Known Limitations

## Implemented Functionality

### Protocol Handling
- **TCP**: Full TCP protocol support, including connection establishment, message sequencing, and reliable data transmission.
- **UDP**: Support for the connectionless UDP protocol, enabling faster data transmission with support for message confirmation.

### Messaging
- **Chat Messaging**: Implementation of a messaging system that allows authenticated users to send and receive messages in real-time.
- **Broadcast**: Functionality to broadcast messages to all users in a channel when a user joins or leaves.

### User Management
- **Authentication**: Support for user authentication using a custom `AUTH` message.
- **Channel Management**: Ability for users to join and switch channels with proper acknowledgment using `JOIN` and `REPLY` messages.

### Server Operations
- **Concurrent Connections**: Capability to handle multiple client connections simultaneously, maintaining separate sessions for each.
- **State Machine Compliance**: Adherence to the specified finite state machine (FSM) for managing the server's responses and transitions based on client messages.

### Logging and Monitoring
- **Activity Logging**: Logging of all incoming and outgoing messages, ensuring transparency and aiding in troubleshooting.

### Command-Line Interface
- **Server Configuration**: Command-line arguments for server configuration, such as specifying the IP address and port for server operations.

## Known Limitations

### Protocol Features
- **Error Handling**: While basic error handling is implemented, certain edge cases may not be fully covered, potentially leading to unexpected behavior under specific circumstances.

### Rapid Re-authentication
- **Connection Persistence on Rapid Re-authentication**: In cases where a user logs out and then attempts to re-authenticate immediately (sending a second `AUTH` message shortly after a `BYE` message), the server may not process the disconnection quickly enough before receiving the subsequent `AUTH` message. Consequently, the server will erroneously disconnect the user upon processing the disconnection from the first session, and as a result, the second `AUTH` message that was intended for the new session is discarded. This issue stems from a timing overlap between the server's user disconnection routine and the initiation of a new connection.

### Performance
- **UDP Confirmation Overhead**: The need for message confirmations in UDP may introduce latency, affecting performance, especially in high-throughput scenarios.
