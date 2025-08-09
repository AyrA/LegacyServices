# Pwdgen Service

- **Full Name**: Password Generator Protocol
- **Port**: 129
- **Security**: Problematic
- **Implementation**: Compatible, Extra
- **Usability**: Minimal
- **Type**: Text
- **RFC**: [972](https://www.rfc-editor.org/rfc/rfc972)

## Description

Service that generates passwords and sends them to the client

## Protocol usage

No special usage requirements. The value is sent immediately.

## Security considerations

The protocol is unencrypted and thus, it's a bad idea to use it, there is however no danger to the host providing this service.
Passwords should always be generated locally.

## Modern usage

There are practically no valid usage scenarios for modern times.
While not usable for passwords, it may be usable as an entropy source for microcontrollers that lack a cryptographically safe RNG.

## Implementation

Implementation is as per RFC in the sence that the protocol specification is honored.
The RFC suggests generating passwords that are easy to pronounce, the current implementation uses a purely RNG based approach however.
By default the service generates six passwords with eight characters each. This is configurable.

## Configuration

The service is configured in `Pwdgen.json`

```js
{
	//Enable or disable the service entirely
	"Enabled": true,
	//Length of each password in characters
	"Length": 8,
	//Number of passwords
	"Count": 6
}
```
