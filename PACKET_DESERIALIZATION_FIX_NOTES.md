# Packet Deserialization Fix - Breaking Changes

## Overview
This fix updates the Unreal client's packet deserializers to match the C# server's actual packet structure. This resolves errors like:
- `Error: PacketDeserializer: LoginResponse expected 4 fields, got 5`
- `Error: PacketDeserializer: CharacterListResponse expected 1 field, got 2`
- `Error: PacketDeserializer: Invalid bool byte: 0x00`

## Changes to Packet Structures

### 1. FLoginResponse
**OLD:**
```cpp
bool Success
FString Message
int64 AccountId
FString SessionToken
```

**NEW:**
```cpp
EResponseCode Result  // Enum instead of bool
FString Message
int64 AccountId
FString SessionToken
FString ServerProtocolVersion  // New field
```

### 2. FCharacterListResponse
**OLD:**
```cpp
TArray<FCharacterInfo> Characters
```

**NEW:**
```cpp
EResponseCode Result  // New field
TArray<FCharacterInfo> Characters
```

### 3. FCreateCharacterResponse
**OLD:**
```cpp
bool Success
FString Message
int64 CharacterId
```

**NEW:**
```cpp
EResponseCode Result  // Enum instead of bool
FString Message
FCharacterInfo Character  // Full character object instead of just ID
```

### 4. FSelectCharacterResponse
**OLD:**
```cpp
bool Success
FString Message
```

**NEW:**
```cpp
EResponseCode Result  // Enum instead of bool
FString Message
FCharacterInfo Character  // New field
```

## EResponseCode Enum
The `EResponseCode` enum already exists in `NetworkTypes.h` with values:
- `Success = 0` - Operation succeeded
- `Error = 1` - Generic error
- `InvalidRequest = 2` - Request was invalid
- `NotAuthenticated = 3` - User not authenticated
- `NameTaken = 10` - Character name already taken
- `InvalidName = 11` - Character name invalid
- `InvalidRaceClassCombination = 12` - Race/class combination not allowed
- And more...

## Blueprint Migration Guide

### For LoginResponse handlers:
**OLD:**
```blueprint
if (LoginResponse.Success == true)
    // Handle success
```

**NEW:**
```blueprint
if (LoginResponse.Result == EResponseCode::Success)
    // Handle success
```

### For CharacterListResponse handlers:
**OLD:**
```blueprint
// Just access Characters array directly
ForEach Character in Response.Characters
```

**NEW:**
```blueprint
// Check result code first
if (Response.Result == EResponseCode::Success)
    ForEach Character in Response.Characters
```

### For CreateCharacterResponse handlers:
**OLD:**
```blueprint
if (Response.Success)
    CharacterId = Response.CharacterId
```

**NEW:**
```blueprint
if (Response.Result == EResponseCode::Success)
    Character = Response.Character  // Full character info now available
    CharacterId = Response.Character.CharacterId
```

### For SelectCharacterResponse handlers:
**OLD:**
```blueprint
if (Response.Success)
    // Character selected
```

**NEW:**
```blueprint
if (Response.Result == EResponseCode::Success)
    Character = Response.Character  // Character data now included
```

## What You Need to Update

### In Blueprints (Unreal Editor):
1. Open all Blueprint widgets/actors that handle network responses
2. Update any nodes that access `Success` to use `Result` instead
3. Update comparison checks from `== true` to `== EResponseCode::Success`
4. For CreateCharacterResponse, change `CharacterId` access to `Character.CharacterId`
5. For SelectCharacterResponse, add handling for the new `Character` field
6. For CharacterListResponse, add result checking before accessing `Characters`

### In C++ Code (if any custom handlers):
1. Replace `Response.Success` with `Response.Result`
2. Check `Result == EResponseCode::Success` instead of `Success == true`
3. Access character data from `Response.Character` instead of `Response.CharacterId`

## Error Handling Examples

### Check for specific errors:
```cpp
switch (Response.Result)
{
    case EResponseCode::Success:
        // Handle success
        break;
    case EResponseCode::NameTaken:
        // Show "Name already taken" error
        break;
    case EResponseCode::InvalidRaceClassCombination:
        // Show "Invalid race/class combo" error
        break;
    default:
        // Show generic error message
        break;
}
```

## Testing
After updating your Blueprints:
1. Rebuild the Unreal project
2. Start the C# server
3. Connect from the Unreal client
4. Test login, character list, create character, and select character flows
5. Verify logs show successful deserialization with no errors

## Deserialization Details
The deserializer now properly handles:
- Reading `ResponseCode` as int instead of bool
- Skipping unused fields in CharacterData (only reads needed fields)
- Handling nullable Character fields (null when operation fails)
- Reading the new ServerProtocolVersion field in LoginResponse
