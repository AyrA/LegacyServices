# QOTD Service

- **Full Name**: Quote of the Day Protocol
- **Port**: 17
- **Security**: Safe
- **Implementation**: Compatible
- **Usability**: Minimal
- **Type**: Text
- **RFC**: [865](https://www.rfc-editor.org/rfc/rfc865)

## Description

Service that sends back a single entry from a list of previously defined quotes.

## Protocol usage

No special usage requirements. The value is sent immediately.

## Security considerations

None.

## Modern usage

There are practically no valid usage scenarios for modern times.

## Implementation

Implementation is as per RFC.

## Configuration

The service is configured in `Qotd.json`

```js
{
	//Enable or disable the service entirely
	"Enabled": true,
	//List of quotes. A predefined set is used if not defined or null. The list cannot be empty, and quotes cannot be null, blank, or whitespace only
	"Quotes": ["Quote 1","Quote 2","..."],
	//Allow quotes exceeding 511 characters. The RFC recommends quotes to be shorter than that due to early limits in UDP packet sizes without causing fragmentation
	"AllowExcessiveLength": false
}
```
