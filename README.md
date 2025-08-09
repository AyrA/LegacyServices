# Legacy Services

This application implements various legacy protocols in a modern manner.
None of these protocols has significant usage potential.

## Safety

Care has been taken to implement the protocols safely, but some protocols are simply unsafe by design. Please review each protocol in the "Docs" folder, and consider disabling all protocols not necessary for operation.

## Protocols

Implemented are (in TCP port order):

- **TCP multiplex**: One of, if not the earliest reverse proxy
- **Echo**: Returns sent data as-isd
- **Discard**: Silently receives data sent to it
- **Users**: Lists logged on users
- **Daytime**: Shows date and time of a system in human readable form
- **Netstat**: Shows socket status
- **Quote of the day**: Sends a random message
- **Message**: Send a message to a user
- **Chargen**: Sends a repeating pattern of characters
- **Time**: Sends time in a computer usable format
- **Pwdgen**: Generates passwords

Each protocol has a document in the "Docs" folder that briefly explains it. This document also shows all possible configuration options.

All protocols are enabled in the TCP multiplex service by default.

## Configuration

Protocols can be individually configured by editing the files in the Config folder.
