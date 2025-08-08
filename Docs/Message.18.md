# Message Service

- **Full Name**: Message Send Protocol 2
- **Port**: 18
- **Security**: Questionable
- **Implementation**: Compatible
- **Usability**: Minimal
- **Type**: Text
- **RFC**: [1312](https://www.rfc-editor.org/rfc/rfc1312)

## Description

Service that sends a message to a user

## Protocol usage

Client sends a command, and the server replies with a success or error message. Multiple commands can be sent in succession.

## Security considerations

This protocol allows unauthenticated users to send messages to a user. Version "B" adds a mitigation for this in form of the "signature" value but the signature is left implementation defined. Using proper cryptographic signatures by the sender can make this protocol safe, but the server still needs to safeguard against reply attacks.

Unless carefully filtered, the message can mess with the terminal because Linux people are stuck in the 80s and cannot be bothered to separate terminal control codes from text data. This problem is less prominent on Windows, unless the terminal runs in "Virtual Terminal" mode.

## Modern usage

There are practically no valid usage scenarios for modern times. It is a lot safer to use a terminal protocol like SSH and invoke the appropriate command to send messages to locally connected users.

## Implementation

Implementation is as per RFC.
The implementation supports type "A" and "B" simultaneously. Each type can be disabled via configuration.

Messages are shown by printing them to the terminal that runs the service. Control characters (except line breaks and tab) are disallowed.

## Configuration

The service is configured in `Message.json`

```js
{
	//Enable or disable the service entirely
	"Enabled": true,
	//Silently discard valid messages instead of printing to console
	"Discard": false,
	//Enable protocol version A
	"VersionA": true,
	//Enable protocol version B
	"VersionB": true
}
```
