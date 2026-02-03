# BP_ConnectTest Event-Driven Implementation Guide

## Overview
This guide provides step-by-step instructions for updating BP_ConnectTest to use **event-driven network flow** instead of hardcoded delays.

## What Changed?

### ❌ Old Approach (Hardcoded Delays)
```
Connect → Wait 1s → Login → Wait 1s → CharacterList → Wait 2s → CreateCharacter → Wait 3s → SelectCharacter
```
**Problems:**
- Delays are arbitrary guesses
- If server is slow, requests fail
- If server is fast, we waste time
- No error handling

### ✅ New Approach (Event-Driven)
```
Connect → Login → [WAIT FOR RESPONSE] → OnLoginResponse → CharacterList → [WAIT FOR RESPONSE] → OnCharacterListResponse → ...
```
**Benefits:**
- ✅ Reliable - No race conditions
- ✅ Fast - Proceeds as soon as server responds
- ✅ Robust - Handles errors properly
- ✅ Debuggable - Clear logs show exact flow
- ✅ Industry Standard - How real MMOs work

## C++ Prerequisites

The following functions are now available in `EldaraNetworkSubsystem`:

### Response Delegates (BlueprintAssignable)
- `OnLoginResponse` - Fires when login response is received
- `OnCharacterListResponse` - Fires when character list is received
- `OnCreateCharacterResponse` - Fires when character creation completes
- `OnSelectCharacterResponse` - Fires when character selection completes
- `OnMovementUpdateResponse` - Fires when movement update is received

### Helper Functions (BlueprintPure)
- `IsResponseSuccess(EResponseCode)` → bool - Check if response succeeded
- `ResponseCodeToString(EResponseCode)` → FString - Get human-readable error message

### Network Functions (BlueprintCallable)
- `ConnectToServer(FString IpAddress, int32 Port)` → bool
- `SendLogin(FString Username, FString PasswordHash)`
- `SendCharacterListRequest()`
- `SendCreateCharacter(FString Name, ERace, EClass, EFaction, ETotemSpirit, FCharacterAppearance)`
- `SendSelectCharacter(int64 CharacterId)`
- `Disconnect()`

## Blueprint Implementation

### Step 1: Setup Event BeginPlay

Create the following node sequence:

```
Event BeginPlay
  ↓
Get Game Instance → Cast To EldaraGameInstance
  ↓
Get Network Subsystem
  ↓
Store in variable: "NetworkSubsystem" (save as member variable)
  ↓
Bind Event to OnLoginResponse → [Custom Event: Handle_LoginResponse]
Bind Event to OnCharacterListResponse → [Custom Event: Handle_CharacterListResponse]
Bind Event to OnCreateCharacterResponse → [Custom Event: Handle_CreateCharacterResponse]
Bind Event to OnSelectCharacterResponse → [Custom Event: Handle_SelectCharacterResponse]
  ↓
Delay 0.1s (let bindings register)
  ↓
Connect To Server ("127.0.0.1", 7777)
  ↓
Branch: Is Connected?
  - TRUE:
      Print String: "Connected to server!"
      Send Login ("TestUser", "password123")
      Print String: "Login request sent..."
  - FALSE:
      Print String: "Failed to connect to server" (Red)
```

**Key Points:**
- Store NetworkSubsystem reference as a variable so custom events can access it
- Bind all response events BEFORE connecting
- Use short 0.1s delay to ensure bindings register before first request

### Step 2: Create Custom Event - Handle_LoginResponse

```
Custom Event: Handle_LoginResponse
Input Parameter: LoginResponse (FLoginResponse struct)
  ↓
Print String: "Login Response - Result: {Result}, Message: {Message}"
  ↓
IsResponseSuccess(Response.Result)?
  ↓
Branch:
  - TRUE:
      Print String: "Login successful! AccountId: {AccountId}" (Green)
      Get NetworkSubsystem variable
      Send Character List Request
      Print String: "Requesting character list..."
  - FALSE:
      Get ResponseCodeToString(Response.Result)
      Print String: "Login failed: {ErrorMessage}" (Red)
      Get NetworkSubsystem variable
      Disconnect
```

**Key Points:**
- Use `IsResponseSuccess()` helper to check success
- Use `ResponseCodeToString()` helper for readable error messages
- Always disconnect on error to clean up connection
- Store AccountId if needed (though server tracks it via session)

### Step 3: Create Custom Event - Handle_CharacterListResponse

```
Custom Event: Handle_CharacterListResponse
Input Parameter: CharacterListResponse (FCharacterListResponse struct)
  ↓
Print String: "CharacterList Response - Result: {Result}, Count: {Characters.Num()}"
  ↓
IsResponseSuccess(Response.Result)?
  ↓
Branch:
  - TRUE:
      Branch: Characters.Num() > 0?
        - TRUE (Has existing characters):
            Get first character: Characters[0]
            Print String: "Selecting existing character: {Character.Name}"
            Get NetworkSubsystem variable
            Send Select Character Request (Character.CharacterId)
        - FALSE (No characters):
            Print String: "No characters found, creating new character..."
            Make CharacterAppearance (default values)
            Get NetworkSubsystem variable
            Send Create Character Request:
              - Name: "Hero"
              - Race: Sylvaen (1)
              - Class: MemoryWarden (1)
              - Faction: VerdantCircles (1)
              - TotemSpirit: None (0)
              - Appearance: [from Make CharacterAppearance]
            Print String: "Character creation request sent..."
  - FALSE:
      Get ResponseCodeToString(Response.Result)
      Print String: "Failed to get character list: {ErrorMessage}" (Red)
      Get NetworkSubsystem variable
      Disconnect
```

**Key Points:**
- Handle both "has characters" and "no characters" cases
- Use first existing character if available
- Create default character if none exist
- Default character uses lore-appropriate values (Sylvaen MemoryWarden)

### Step 4: Create Custom Event - Handle_CreateCharacterResponse

```
Custom Event: Handle_CreateCharacterResponse
Input Parameter: CreateCharacterResponse (FCreateCharacterResponse struct)
  ↓
Print String: "CreateCharacter Response - Result: {Result}, Message: {Message}"
  ↓
IsResponseSuccess(Response.Result)?
  ↓
Branch:
  - TRUE:
      Print String: "Character created! ID: {Character.CharacterId}, Name: {Character.Name}" (Green)
      Get NetworkSubsystem variable
      Send Select Character Request (Response.Character.CharacterId)
      Print String: "Selecting new character..."
  - FALSE:
      Print String: "Failed to create character: {Message}" (Red)
      Get NetworkSubsystem variable
      Disconnect
```

**Key Points:**
- Extract CharacterId from Response.Character
- Immediately select the newly created character
- Server assigns CharacterId - don't hardcode it

### Step 5: Create Custom Event - Handle_SelectCharacterResponse

```
Custom Event: Handle_SelectCharacterResponse
Input Parameter: SelectCharacterResponse (FSelectCharacterResponse struct)
  ↓
Print String: "SelectCharacter Response - Result: {Result}, Message: {Message}"
  ↓
IsResponseSuccess(Response.Result)?
  ↓
Branch:
  - TRUE:
      Print String: "=== CHARACTER SELECTED SUCCESSFULLY ===" (Cyan)
      Print String: "Character: {Character.Name}, Level {Character.Level}"
      Print String: "Race: {Character.Race}, Class: {Character.Class}"
      Print String: "Ready to enter world!"
      // TODO: Transition to world/gameplay
  - FALSE:
      Print String: "Failed to select character: {Message}" (Red)
      Get NetworkSubsystem variable
      Disconnect
```

**Key Points:**
- This is the final step before entering the game world
- Character is now ready to spawn in world
- Future: Load world, spawn player character, start gameplay

## Complete Flow Diagram

```
Event BeginPlay
  ↓
Setup Event Bindings
  ↓
Connect To Server (127.0.0.1:7777)
  ↓
Send Login Request
  ↓
╔═══════════════════════════════════════╗
║  [WAIT FOR SERVER RESPONSE]           ║
╚═══════════════════════════════════════╝
  ↓
OnLoginResponse Event Fires
  ↓
Handle_LoginResponse
  ↓
Check: Success?
  YES → Send CharacterListRequest
  NO  → Print error, Disconnect, END
  ↓
╔═══════════════════════════════════════╗
║  [WAIT FOR SERVER RESPONSE]           ║
╚═══════════════════════════════════════╝
  ↓
OnCharacterListResponse Event Fires
  ↓
Handle_CharacterListResponse
  ↓
Check: Success?
  YES → Characters.Num() > 0?
         YES → Send SelectCharacterRequest (first character)
         NO  → Send CreateCharacterRequest
  NO  → Print error, Disconnect, END
  ↓
╔═══════════════════════════════════════╗
║  [WAIT FOR SERVER RESPONSE]           ║
╚═══════════════════════════════════════╝
  ↓
OnCreateCharacterResponse Event Fires (if created)
  ↓
Handle_CreateCharacterResponse
  ↓
Check: Success?
  YES → Send SelectCharacterRequest (new CharacterId)
  NO  → Print error, Disconnect, END
  ↓
╔═══════════════════════════════════════╗
║  [WAIT FOR SERVER RESPONSE]           ║
╚═══════════════════════════════════════╝
  ↓
OnSelectCharacterResponse Event Fires
  ↓
Handle_SelectCharacterResponse
  ↓
Check: Success?
  YES → Print success, Ready to enter world!
  NO  → Print error, Disconnect, END
```

## Expected Console Output

### Successful Flow (New Character)
```
LogTemp: EldaraNetworkSubsystem: Connected to 127.0.0.1:7777
Connected to server!
Login request sent...
LogTemp: EldaraNetworkSubsystem: Login request sent for user: TestUser
Login Response - Result: Success, Message: Login successful
Login successful! AccountId: 1001
Requesting character list...
LogTemp: EldaraNetworkSubsystem: Character list request sent
CharacterList Response - Result: Success, Count: 0
No characters found, creating new character...
Character creation request sent...
LogTemp: EldaraNetworkSubsystem: Create character request sent for 'Hero'
CreateCharacter Response - Result: Success, Message: Character created successfully
Character created! ID: 1, Name: Hero
Selecting new character...
LogTemp: EldaraNetworkSubsystem: Select character request sent (ID: 1)
SelectCharacter Response - Result: Success, Message: Character selected
=== CHARACTER SELECTED SUCCESSFULLY ===
Character: Hero, Level 1
Race: Sylvaen, Class: MemoryWarden
Ready to enter world!
```

### Successful Flow (Existing Character)
```
LogTemp: EldaraNetworkSubsystem: Connected to 127.0.0.1:7777
Connected to server!
Login request sent...
LogTemp: EldaraNetworkSubsystem: Login request sent for user: TestUser
Login Response - Result: Success, Message: Login successful
Login successful! AccountId: 1001
Requesting character list...
LogTemp: EldaraNetworkSubsystem: Character list request sent
CharacterList Response - Result: Success, Count: 1
Selecting existing character: Hero
LogTemp: EldaraNetworkSubsystem: Select character request sent (ID: 1)
SelectCharacter Response - Result: Success, Message: Character selected
=== CHARACTER SELECTED SUCCESSFULLY ===
Character: Hero, Level 5
Race: Sylvaen, Class: MemoryWarden
Ready to enter world!
```

### Error Flow (Login Failed)
```
LogTemp: EldaraNetworkSubsystem: Connected to 127.0.0.1:7777
Connected to server!
Login request sent...
LogTemp: EldaraNetworkSubsystem: Login request sent for user: TestUser
Login Response - Result: NotAuthenticated, Message: Invalid credentials
Login failed: Not Authenticated
LogTemp: EldaraNetworkSubsystem: Disconnected
```

## Testing

### Prerequisites
1. C# server is running (`dotnet run` in Server directory)
2. Server is listening on 127.0.0.1:7777
3. BP_ConnectTest has been updated with event-driven flow

### Test Steps
1. Place BP_ConnectTest actor in a level
2. Press Play in Unreal Editor
3. Watch Output Log for the flow sequence
4. Verify no errors occur
5. Verify character is selected successfully

### Troubleshooting

**Problem:** "Failed to connect to server"
- **Solution:** Start the C# server first
- **Check:** Server console shows "Listening on port 7777"

**Problem:** "Login failed: Not Authenticated"
- **Solution:** Check username/password match server expectations
- **Note:** Server may require valid credentials or use test accounts

**Problem:** Event handlers never fire
- **Solution:** Ensure event bindings happen BEFORE first request
- **Check:** Add Print String after each "Bind Event" to confirm setup

**Problem:** "Failed to create character: Name Taken"
- **Solution:** Use a different character name or delete existing character
- **Note:** Server enforces unique character names per account

## Next Steps

After this works:
1. **Enter World Flow** - Deserialize world state, spawn player pawn
2. **Movement Sync** - Bind OnMovementUpdateResponse, implement client prediction
3. **Combat Packets** - Handle ability results, damage, healing
4. **Chat System** - Implement chat message handling
5. **Quest System** - Quest accept/progress/complete flow

## Migration from Old Implementation

If you have an existing BP_ConnectTest with hardcoded delays:

1. **Backup:** Duplicate the blueprint first
2. **Remove:** Delete all Delay nodes between network requests
3. **Add:** Create the 4 custom event handlers (Handle_LoginResponse, etc.)
4. **Bind:** Add event binding nodes in Event BeginPlay
5. **Test:** Verify with server running
6. **Compare:** Old version took 6+ seconds, new version ~1-2 seconds

## Reference: CharacterAppearance Default Values

```
FaceType: 1
HairStyle: 1
HairColor: 1
SkinTone: 1
EyeColor: 1
Height: 1.0
BuildType: 1.0
FurPattern: 0
FurColor: 0
VoidIntensity: 0.0
```

## Reference: Enum Values

### ERace
- None = 0
- Sylvaen = 1
- HighElf = 2
- Human = 3
- Therakai = 4
- Gronnak = 5
- VoidTouched = 6

### EClass
- None = 0
- MemoryWarden = 1
- TemporalMage = 2
- UnboundWarrior = 3
- TotemShaman = 4
- Berserker = 5
- WitchKingAcolyte = 6
- VoidStalker = 7
- RootDruid = 8
- ArchonKnight = 9
- FreeBlade = 10
- GodSeekerCleric = 11
- MemoryThief = 12

### EFaction
- Neutral = 0
- VerdantCircles = 1 (Green Memory - Sylvaen default)
- AscendantLeague = 2 (Radiant Order - High Elf default)
- UnitedKingdoms = 3 (Free Realms - Human default)
- TotemClansWildborn = 4 (Therakai Wildborn)
- TotemClansPathbound = 5 (Therakai Pathbound)
- DominionWarhost = 6 (The Dominion - Gronnak default)
- VoidCompact = 7 (Void Compact - VoidTouched default)

### ETotemSpirit
- None = 0 (Not Therakai)
- Fanged = 1
- Horned = 2
- Clawed = 3
- Winged = 4

## Advanced: Parallel Response Handling

For advanced users, you can bind multiple response handlers and handle responses in parallel:

```
// In Event BeginPlay, also bind:
Bind Event to OnMovementUpdateResponse → Handle_MovementUpdate
Bind Event to OnAbilityResult → Handle_AbilityResult
Bind Event to OnDamageReceived → Handle_Damage
```

This allows the client to react to multiple server events simultaneously, which is essential for real-time gameplay.

---

**This is the foundation for proper MMO networking!** 🎉
