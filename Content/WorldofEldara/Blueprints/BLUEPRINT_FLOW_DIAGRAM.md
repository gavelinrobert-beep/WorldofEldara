# BP_ConnectTest Blueprint Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         BP_ConnectTest Event Graph                          │
└─────────────────────────────────────────────────────────────────────────────┘

Event BeginPlay
     │
     ├──► Get Game Instance ──► Cast to EldaraGameInstance
     │                               │
     │                               ├──► Get Network Subsystem
     │                                         │
     ├────────────────────────────────────────┤
     │
     ├──► Connect To Server
     │    • IP: "127.0.0.1"
     │    • Port: 7777
     │
     ├──► Delay (0.5s) ──────────────────────┐ Wait for connection
     │                                        │
     ├────────────────────────────────────────┤
     │
     ├──► Send Login
     │    • Username: "testuser"
     │    • PasswordHash: "testhash"
     │
     │
┌────┴─────────────────────────────────────────────────────────────────────────┐
│                    NEW: Character Selection Flow                             │
└──────────────────────────────────────────────────────────────────────────────┘
     │
     ├──► Delay (2.0s) ──────────────────────┐ Wait for login response
     │                                        │
     ├────────────────────────────────────────┤
     │
     ├──► Get Game Instance ──► Cast to EldaraGameInstance
     │                               │
     │                               ├──► Get Network Subsystem
     │                                         │
     ├────────────────────────────────────────┤
     │
     ├──► Send Character List Request  ◄──── NEW FUNCTION
     │
     ├──► Delay (2.0s) ──────────────────────┐ Wait for character list
     │                                        │ (assume empty list)
     ├────────────────────────────────────────┤
     │
     ├──► Make CharacterAppearance ──────────┐ Create struct
     │    • FaceType: 0                      │
     │    • HairStyle: 0                     │
     │    • HairColor: 0                     │
     │    • SkinTone: 0                      │
     │    • EyeColor: 0                      │
     │    • Height: 1.0                      │
     │    • BuildType: 0.5                   │
     │    • FurPattern: 0                    │
     │    • FurColor: 0                      │
     │    • VoidIntensity: 0.0               │
     │                                        │
     │    ┌───────────────────────────────────┘
     │    │
     ├──► Send Create Character  ◄──── NEW FUNCTION
     │    • Name: "Hero"
     │    • Race: Sylvaen (1)
     │    • Class: MemoryWarden (1)
     │    • Faction: VerdantCircles (1)
     │    • TotemSpirit: None (0)
     │    • Appearance: (from struct above)
     │
     ├──► Delay (2.0s) ──────────────────────┐ Wait for character creation
     │                                        │
     ├────────────────────────────────────────┤
     │
     ├──► Send Select Character  ◄──── NEW FUNCTION
     │    • CharacterId: 1
     │
     ├──► Print String
     │    • Text: "Entering world with character..."
     │    • Duration: 5.0
     │    • Color: Green (0, 255, 0)
     │
     └──► End

┌─────────────────────────────────────────────────────────────────────────────┐
│                            Expected Log Output                              │
└─────────────────────────────────────────────────────────────────────────────┘

LogTemp: EldaraNetworkSubsystem: Connected to 127.0.0.1:7777
LogTemp: EldaraNetworkSubsystem: Login request sent for user: testuser
LogTemp: EldaraNetworkSubsystem: Character list request sent         ◄── NEW
LogTemp: EldaraNetworkSubsystem: Create character request sent for 'Hero' ◄── NEW
LogTemp: EldaraNetworkSubsystem: Select character request sent (ID: 1)    ◄── NEW
[Screen]: Entering world with character...

┌─────────────────────────────────────────────────────────────────────────────┐
│                          Network Packet Flow                                │
└─────────────────────────────────────────────────────────────────────────────┘

Client                           Server
  │                                 │
  ├──► LoginRequest ───────────────►│
  │                                 │
  │◄─── LoginResponse ──────────────┤
  │                                 │
  ├──► CharacterListRequest ───────►│  ◄── NEW
  │                                 │
  │◄─── CharacterListResponse ──────┤  (empty list)
  │                                 │
  ├──► CreateCharacterRequest ─────►│  ◄── NEW ("Hero")
  │                                 │
  │◄─── CreateCharacterResponse ────┤  (CharacterId: 1)
  │                                 │
  ├──► SelectCharacterRequest ─────►│  ◄── NEW (ID: 1)
  │                                 │
  │◄─── SelectCharacterResponse ────┤
  │◄─── EnterWorld/PlayerSpawn ─────┤
  │                                 │

┌─────────────────────────────────────────────────────────────────────────────┐
│                        Implementation Checklist                             │
└─────────────────────────────────────────────────────────────────────────────┘

 ✅ C++ Functions Implemented
    ✅ SendCharacterListRequest()
    ✅ SendCreateCharacter()
    ✅ SendSelectCharacter()

 ⚠️  Blueprint Creation (Manual)
    ☐ Create BP_ConnectTest Blueprint asset in Unreal Editor
    ☐ Add Event BeginPlay node
    ☐ Add connection and login sequence (existing)
    ☐ Add character list request (NEW)
    ☐ Add Make CharacterAppearance struct (NEW)
    ☐ Add create character call (NEW)
    ☐ Add select character call (NEW)
    ☐ Add confirmation print string (NEW)
    ☐ Connect all nodes in sequence
    ☐ Set all parameter values correctly

 ⚠️  Testing (Requires Unreal + Server)
    ☐ Start C# server
    ☐ Place BP_ConnectTest in level
    ☐ Play in Editor
    ☐ Verify log output
    ☐ Verify server receives all packets
    ☐ Verify character is created and selected
```

## Key Points

### Timing
- **0.5s** after connection: Send login
- **2.0s** after login: Request character list
- **2.0s** after list: Create character
- **2.0s** after creation: Select character

### Enum Values (from NetworkTypes.h)
- `ERace::Sylvaen = 1`
- `EClass::MemoryWarden = 1`
- `EFaction::VerdantCircles = 1`
- `ETotemSpirit::None = 0`

### Character Appearance Defaults
All appearance values set to 0 or default values:
- Numeric fields (FaceType, HairStyle, etc.): 0
- Height: 1.0 (normal height)
- BuildType: 0.5 (average build)
- VoidIntensity: 0.0 (no void corruption)

### Network Architecture
- **Protocol**: TCP with MessagePack serialization
- **Port**: 7777 (game server)
- **Packet Format**: 4-byte length prefix + MessagePack payload
- **Max Packet Size**: 8KB
