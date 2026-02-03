# BP_ConnectTest Quick Reference Card

## Function Signatures (BlueprintCallable)

### 1. SendCharacterListRequest
```cpp
void SendCharacterListRequest()
```
**No parameters needed** - Server uses session token to identify account.

---

### 2. SendCreateCharacter
```cpp
void SendCreateCharacter(
    FString Name,
    ERace Race,
    EClass Class,
    EFaction Faction,
    ETotemSpirit TotemSpirit,
    FCharacterAppearance Appearance
)
```

**Blueprint Values:**
- **Name**: `"Hero"` (string literal)
- **Race**: `Sylvaen` (dropdown, value = 1)
- **Class**: `MemoryWarden` (dropdown, value = 1)
- **Faction**: `VerdantCircles` (dropdown, value = 1)
- **TotemSpirit**: `None` (dropdown, value = 0)
- **Appearance**: Connect from "Make CharacterAppearance" node

---

### 3. SendSelectCharacter
```cpp
void SendSelectCharacter(int64 CharacterId)
```

**Blueprint Values:**
- **CharacterId**: `1` (integer literal)

---

## CharacterAppearance Struct

### Make CharacterAppearance Node Values
```
FCharacterAppearance
├─ FaceType: 0
├─ HairStyle: 0
├─ HairColor: 0
├─ SkinTone: 0
├─ EyeColor: 0
├─ Height: 1.0
├─ BuildType: 0.5
├─ FurPattern: 0
├─ FurColor: 0
└─ VoidIntensity: 0.0
```

**All fields are integers except:**
- `Height` → Float (1.0)
- `BuildType` → Float (0.5)
- `VoidIntensity` → Float (0.0)

---

## Delay Values

```
After connection:    0.5 seconds
After login:         2.0 seconds
After char list:     2.0 seconds
After char creation: 2.0 seconds
```

---

## Node Connection Pattern

```
[Previous Node] ──Exec──► [Current Node] ──Exec──► [Next Node]
                              │
                              └──Return/Data──► [Input of Another Node]
```

### Example: Getting Network Subsystem
```
[Get Game Instance] ──Exec──► [Cast to EldaraGameInstance]
                                  │
                                  └──As Eldara Game Instance──► [Get Network Subsystem]
                                                                      │
                                                                      └──Return Value──► [SendCharacterListRequest]
```

---

## Print String Node

```
Print String
├─ In String: "Entering world with character..."
├─ Duration: 5.0
└─ Text Color: RGB(0, 255, 0) [Green]
```

---

## Testing Connection String

```
Server IP:   127.0.0.1
Server Port: 7777
Username:    testuser
Password:    testhash
```

---

## Expected Log Messages (In Order)

```
1. EldaraNetworkSubsystem: Connected to 127.0.0.1:7777
2. EldaraNetworkSubsystem: Login request sent for user: testuser
3. EldaraNetworkSubsystem: Character list request sent
4. EldaraNetworkSubsystem: Create character request sent for 'Hero'
5. EldaraNetworkSubsystem: Select character request sent (ID: 1)
6. [Print String] Entering world with character...
```

---

## Common Mistakes to Avoid

❌ **Don't** hardcode AccountId - it's set to 0 in C++ (server uses session token)
❌ **Don't** forget to connect the Appearance struct output to SendCreateCharacter
❌ **Don't** use wrong delay values (must be 0.5s, then 2.0s, 2.0s, 2.0s)
❌ **Don't** forget to cache the Network Subsystem reference after first retrieval
✅ **Do** connect all Exec pins in sequence
✅ **Do** use the exact enum values (Sylvaen=1, MemoryWarden=1, etc.)
✅ **Do** connect CharacterAppearance output to SendCreateCharacter's Appearance pin

---

## Blueprint Node Placement Suggestion

```
X-Axis (horizontal flow):
- Each major step approximately 200 units apart
- Total flow spans ~3200 units

Y-Axis (vertical):
- Main sequence at Y=0
- Data nodes (like MakeStruct) at Y=100 (below)
- Branch/Cast nodes inline with main flow
```

---

## Debugging Tips

1. **Check Connection State**
   - Use IsConnected() node after ConnectToServer
   - Add Print String to show connection status

2. **Verify Subsystem**
   - Add Print String after Get Network Subsystem
   - Check if return value is valid

3. **Log Packet Sends**
   - Each Send* function logs automatically
   - Check Output Log window for messages

4. **Server Side**
   - Run server with verbose logging
   - Watch for packet reception messages
   - Verify packet parsing succeeds

---

## File Locations

- **C++ Header**: `Source/Eldara/Networking/EldaraNetworkSubsystem.h`
- **C++ Implementation**: `Source/Eldara/Networking/EldaraNetworkSubsystem.cpp`
- **Packet Definitions**: `Source/Eldara/Networking/NetworkPackets.h`
- **Enum Definitions**: `Source/Eldara/Networking/NetworkTypes.h`
- **Blueprint Location**: `Content/WorldofEldara/Blueprints/BP_ConnectTest.uasset` (to be created)

---

## Quick Copy-Paste Values

### String Values
```
"Hero"
"testuser"
"testhash"
"127.0.0.1"
"Entering world with character..."
```

### Integer Values
```
7777 (port)
0 (most appearance fields)
1 (CharacterId)
```

### Float Values
```
0.5 (connection delay)
2.0 (other delays)
1.0 (height)
0.5 (build type)
5.0 (print duration)
```

### Enum Selections
```
Sylvaen
MemoryWarden
VerdantCircles
None (totem spirit)
```

---

*For detailed implementation guide, see `BP_ConnectTest_Implementation_Guide.md`*
*For visual flow diagram, see `BLUEPRINT_FLOW_DIAGRAM.md`*
*For JSON specification, see `BP_ConnectTest_Specification.json`*
