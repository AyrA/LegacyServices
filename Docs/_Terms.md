# Terms used in these documents

This document briefly explains the terms used in other documents.

## Introduction

The introduction for each service contains a list as follows:

### Full Name

The full service name as per RFC

### Port

The TCP port number this service is intended to run at

### Security

Protocol security in regards to modern expectations:

- **Dangerous**: Protocol is dangerous (by design) and should not be exposed at all
- **Problematic**: Protocol lacks important security features and should not be used
- **Questionable**: Protocol is generally safe but may have questionable features that need attention before deploying
- **Undefined**: Difficult to evaluate
- **Safe**: Protocol is safe to use if properly deployed (and potentially limited)

### Implementation

The implementation of the service in this project

- **Compatible**: Protocol implementation should be fully compatible with clients adhering to the used RFCs
- **Limited**: Protocol is implemented but will lilely not provide full compatibility with the original specification. This may be due to missing or altered features
- **Incompatible**: Protocol is not implemented in an RFC compliant manner
- **Extra**: The protocol has been extended with modern features
- **Unspecified**: The protocol lacks formal definition, or the definition is not accurate enough to implement it in a faithful manner

More details will be available in the "Implementation" chapter of the relevant protocol.

### Usability

How usable the protocol is in modern times.

- **Minimal**: Next to not benefit, or the benefits to not outweigh the security considerations
- **Moderate**: Protocol may still be useful to some degree in niche applications
- **Useful**: Protocol may still be useful or is outright still commonly in use today.

### Type

Type of protocol. See [_Types.md](_Types.md) for a list.
This is the type the protocol starts in, but protocols may switch type at runtime

### RFC

The primary RFC used as implementation guide
