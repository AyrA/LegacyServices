# TCP Multiplexing Service

- **Full Name**: TCP Port Service Multiplexer (TCPMUX)
- **Port**: 1
- **Security**: Problematic
- **Implementation**: Compatible, Extra
- **Usability**: Minimal
- **Type**: Text, CI
- **RFC**: [1087](https://www.rfc-editor.org/rfc/rfc1078)

## Description

This is a service that permits access to other services (usually but not necessarily) on the same host.

Created back when "well known ports" did not exceed 255,
it was thought to be a useful service to allow a virtually unlimited other services to run behind it.

Being a TCP multiplex service, only TCP services can be tunneled over it.

## Protocol usage

The protocol has two modes: "connection", and "query"

### Connection

1. Client connects to server
2. Client sends a line consisting only of the requested service name
3. Server responds with a single line starting with `+` or `-`, followed by an optional message
4. If `-`, the server immediately disconnects.
5. If `+`, the protocol switches to binary mode, and all future traffic is forwarded between client and the connected service until either end terminates the connection.

### Query

1. Client connects to server
2. Client sends a line consisting of "HELP"
3. Server sends the service list (one per line) to the client
4. Server disconnects

The RFC suggests using the list of well known service names for known services, but allows for arbitrary names.

## Security considerations

The service allows an attacker to easily enumerate all provided services if the "HELP" command is not disabled.
This is comparable to a port scan but without the guesswork as to what a service is.

Since all traffic is tunneled over a single port,
it makes traditional network rules hard to enforce.

The protocol lacks any form of authentication.

The protocol provides only minimal benefit over direct port exposure of backend services in that it can hide certain services from the HELP list. In that case, the service name can function as a sort of authentication cookie. Being unencrypted, this is of limited use.

## Modern usage

There are practically no valid usage scenarios for modern times. In general, the 65535 possible TCP ports are sufficient.

In a sufficiently secure environment, the protocol could be used as a sort of service discovery.

Although not mentioned in the RFC, the requested service doesn't has to be on the same machine. This allows the machine to effectively be turned into a reverse proxy, or a single point of entry into a network.

## Implementation

Base implementation is as per RFC.
In addition, (and provided enabled) "STARTTLS" can be used to initiate a TLS connection. Sending that line will immediately begin TLS authentication without any cleartext response beforehand. mTLS (client certificates) is not implemented.
If TLS is enforced (not implemented), the protocol can be used to enable TLS support for any arbitrary protocol and allow for secure usage of hidden service names as authentication cookie (see security considerations)

## Configuration

The service is configured in `TcpMultiplex.json`

```json
{
	// Enable or disable the service entirely
    "Enabled": true,
	// Enable STARTTLS support
    "StartTls": true,
	// Allow HELP command. The command is always enabled but the list will be empty if set to "false"
    "Help": true,
	// TLS certificate. This section is optional. If not specified (or set to null), a self-signed certificate will be used
    "Certificate": {
		// Certificate file. PEM or binary. Incomplete paths are resolved relative to the config directory.
        "Public": "path/to/cert",
		// Key file. PEM or binary. Incomplete paths are resolved relative to the config directory.
        "Private": "path/to/key"
    },
	// Service list. See below for service object
    "Services": []
}
// Service
{
	// Name as shown in HELP command
	"Name": "HTTP",
	// Endpoint where to connect to
	"Endpoint": "127.0.0.1:80",
	// true to report it in HELP, false to hide
	"Public": true
}
```
