# Daytime Service

- **Full Name**: Daytime Protocol
- **Port**: 13
- **Security**: Safe
- **Implementation**: Compatible
- **Usability**: Minimal
- **Type**: Text
- **RFC**: [867](https://www.rfc-editor.org/rfc/rfc867)

## Description

Service that sends back the current date and time

## Protocol usage

No special usage requirements. The value is sent immediately.

## Security considerations

None. Other protocols like HTTP also have the ability to report server time to the user, and this feature is usually enabled by default.

## Modern usage

There are practically no valid usage scenarios for modern times. The format of the answer is left implementation defined, with two formats given as suggestion.

## Implementation

Implementation is as per RFC.

## Configuration

The service is configured in `Daytime.json`

```js
{
	//Enable or disable the service entirely
	"Enabled": true,
	//Use UTC instead of local time
	"UseUtc": false,
	//Format string. The supported formats are as per .NET.
	//If null, assumes "f"
	//Standard formats: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
	//Custom formats: https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
	"Format": "f"
}
```
