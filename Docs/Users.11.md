# User Service

- **Full Name**: Active Users
- **Port**: 11
- **Security**: Questionable
- **Implementation**: Compatible
- **Usability**: Minimal
- **Type**: Text
- **RFC**: [866](https://www.rfc-editor.org/rfc/rfc866)

## Description

Service that sends back a list of users logged into the machine providing this service.

## Protocol usage

No special usage requirements. The user list is sent immediately.

## Security considerations

This protocol allows an unauthenticated user to view user names.

## Modern usage

There are practically no valid usage scenarios for modern times. Modern systems provide safer alternatives to retrieve a user list.

## Implementation

Implementation is as per RFC.
Users are returned as plain user names for local users,
and (on Windows) user@domain for remote users

## Configuration

The service is configured in `Users.json`

```js
{
	//Enable or disable the service entirely
	"Enabled": true,
	//Only return the user that runs this service
	"CurrentOnly": false
}
```
