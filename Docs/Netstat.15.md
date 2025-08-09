# Netstat Service

- **Full Name**: Netstat Service
- **Port**: 15
- **Security**: Dangerous, Problematic
- **Implementation**: Unspecified
- **Usability**: Minimal
- **Type**: Text
- **RFC**: None (Unofficial assignment)

## Description

Service that sends back the result of the `netstat` command

## Protocol usage

No special usage requirements. The value is sent immediately.

## Security considerations

- Allows enumeration of IP address and port combination regardless of whether the service is blocked by a firewall or not.
- Allows to enumerate services bound to other interfaces such as loopback.
- Allows to enumerate connected clients.

## Modern usage

There are no valid usage scenarios for modern times. The command is trivially accessible from any remote shell protocol

## Implementation

Implementation is the output of `netstat -an`,
but by default, it only shows listening endpoints, not connected or closing endpoints because this would list IP addresses of remote systems.

## Configuration

The service is configured in `Netstat.json`

```js
{
	//Enable or disable the service entirely
	"Enabled": true,
	//Show all lines, otherwise only those in "LISTEN(ING)" state are shown
	"All": false
}
```
