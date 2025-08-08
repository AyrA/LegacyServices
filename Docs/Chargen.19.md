# Chargen Service

- **Full Name**: Character Generator Protocol
- **Port**: 19
- **Security**: Safe
- **Implementation**: Compatible, Extra
- **Usability**: Minimal
- **Type**: Text
- **RFC**: [864](https://www.rfc-editor.org/rfc/rfc864)

## Description

Service that sends back infinite amount of data until the client disconnects.

The exact data is not specified, but is usually a rolling buffer of printable ASCII characters.

## Protocol usage

No special usage requirements. The value is sent immediately.

## Security considerations

None. No sensitive information is sent.
The protocol may be abused to exhaust server bandwidth, but so does repeatedly downloading large files over HTTP. Server admins can set limits on the number of lines as well as introduce a delay between lines to combat this.

## Modern usage

There are practically no valid usage scenarios for modern times. The protocol may be used as a unidirectional speed test mechanism if no limits are set and the implementation is optimized for speed.

## Implementation

Implementation is as per RFC. Extra options to set limits exist. A speed test option also exists

## Configuration

The service is configured in `Chargen.json`

```js
{
	//Enable or disable the service entirely
	"Enabled": true,
	//Limit the number of lines before the connection is closed. Zero indicates no limit
	"LineLimit": 1e6,
	//Delay individual lines by the given number of milliseconds. Zero indicates no delay
	"LineDelay": 100,
	//Apply the delay accross all connected clients rather than individually
	"GlobalDelay": false,
	//Speedtest mode. Sends multiple lines at once. Cannot be used together with LineDelay or GlobalDelay. LineLimit works but will only be accurate to a block boundary
	"SpeedTest": false
}
```
