# Time Service

- **Full Name**: Time Protocol
- **Port**: 37
- **Security**: Safe
- **Implementation**: Compatible, Extra
- **Usability**: Minimal
- **Type**: Binary
- **RFC**: [868](https://www.rfc-editor.org/rfc/rfc868)

## Description

Service that sends back the current date and time in a format that is easy to handle for computers.
The protocol will stop working at Thu 2036-02-07 06:28:15 UTC

## Protocol usage

No special usage requirements. The value is sent immediately.

## Security considerations

None. Other protocols like HTTP also have the ability to report server time to the user, and this feature is usually enabled by default.

## Modern usage

There are practically no valid usage scenarios for modern times. The protocol does not deal with network latency at all.

## Implementation

Implementation is as per RFC.
If the "UseInt64" option is enabled, the protocol uses 64 bit integers instead of 32 bits, and will continue to work after the original deadline. Legacy implementations are not compatible with this however.

## Configuration

The service is configured in `Time.json`

```js
{
	//Enable or disable the service entirely
	"Enabled": true,
	//Use 64 bit integers instead of 32 bit
	"UseInt64": false
}
```
