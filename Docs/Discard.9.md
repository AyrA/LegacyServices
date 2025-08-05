# Echo Service

- **Full Name**: Discard Protocol
- **Port**: 9
- **Security**: Safe
- **Implementation**: Compatible, Extra
- **Usability**: Minimal
- **Type**: Binary
- **RFC**: [863](https://www.rfc-editor.org/rfc/rfc863)

## Description

Service that receives data and discards it.
Comparable to a null stream or null device,
but causes actual network traffic.

## Protocol usage

No special usage requirements

## Security considerations

None

## Modern usage

There are practically no valid usage scenarios for modern times. The protocol may be used to stress test the network in a single direction, or to stress test TCP checksum generation and validation code in network cards.

## Implementation

Base implementation is as per RFC.
In addition, a connection can be limited to a certain number of bytes and idle time

## Configuration

The service is configured in `Discard.json`

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
