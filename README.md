# Legacy Services

This application implements various legacy protocols in a modern manner.

## Safety

Care has been taken to implement the protocols safely, but some protocols are simply unsafe by design. Please review each protocol in the "Docs" folder, and consider disabling all protocols not necessary for operation.

## Protocols

Implemented are (in TCP port order):

- TCP multiplex
- Echo
- Discard
- Users
- Daytime
- Netstat
- Quote of the day
- Message
- Chargen
- Time
- Pwdgen

Each protocol has a document in the "Docs" folder that briefly explains it. This document also shows all possible configuration options.

All protocols are enabled in the TCP multiplex service by default.

## Configuration

Protocols can be individually configured by editing the files in the Config folder.
