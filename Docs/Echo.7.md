# Echo Service

- **Full Name**: Echo Protocol
- **Port**: 7
- **Security**: Safe
- **Implementation**: Compatible, Extra
- **Usability**: Minimal
- **Type**: Binary
- **RFC**: [862](https://www.rfc-editor.org/rfc/rfc862)

## Description

Service that simply sends all data it receives back

## Protocol usage

No special usage requirements. The protocol immediately starts in echo mode.

## Security considerations

If not rate limited, the protocol could be used to exhaust service bandwidth.

In the RFC, the service also exists on UDP, which makes it trivial for an attacker to forge the sender IP to have the server send the response packet to an unintended recipient. For example, two servers running an echo service can be made to talk to each other indefinitely.

## Modern usage

There are practically no valid usage scenarios for modern times. ICMP Ping works in a similar fashion.

## Implementation

Base implementation is as per RFC.
In addition, a connection can be limited to a certain number of bytes and idle time

## Configuration

The service is configured in `Echo.json`

```js
{
	//Enable or disable the service entirely
	"Enabled": true,
	//Maximum number of seconds the connection can be idle before it's terminated. Zero or negative disables this feature
	"Timeout": 30,
	//Maximum number of bytes that can be sent before the connection is terminated
	"MaxData": 1e9
}
```
