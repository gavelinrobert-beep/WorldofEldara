# FixMap Deserialization Fix - Summary

## Overview
This fix resolves the critical deserialization error `"Cannot skip unknown MessagePack type: 0x80"` that was preventing the Unreal client from successfully deserializing CreateCharacterResponse and SelectCharacterResponse packets.

## Problem Statement
The Unreal client's PacketDeserializer was unable to skip MessagePack FixMap (0x80-0x8f) values, which are used extensively in the C# server's CharacterData structure for nested objects containing Dictionary fields (e.g., FactionStandings in CharacterStats, Resistances in CharacterStats, etc.).

## Root Cause
The original `SkipValue()` function only handled:
- FixInt, FixStr, FixArray
- Explicit integer, float, string, and boolean types
- Array16 and Array32

It did NOT handle:
- FixMap (0x80-0x8f)
- Map16 (0xde)
- Map32 (0xdf)

## Solution Implemented

### 1. Added Map Format Constants (`MessagePackFormat.h`)
```cpp
constexpr uint8 FixMapMask = 0x80;  // 0x80 - 0x8f (0 to 15 key-value pairs)
constexpr uint8 Map16 = 0xde;
constexpr uint8 Map32 = 0xdf;
```

### 2. Enhanced SkipValue Function
Updated to handle:
- **FixMap** (0x80-0x8f): Maps with 0-15 key-value pairs
- **Map16**: Maps with 16-65535 pairs
- **Map32**: Maps with 65536+ pairs

All map types are handled by the new `SkipMap()` helper function.

### 3. Added Helper Functions

#### SkipMap()
```cpp
bool FPacketDeserializer::SkipMap(const TArray<uint8>& InBytes, int32 MapSize)
```
- Safely skips map key-value pairs
- Iterates once per pair, skipping both key and value
- Avoids overflow from multiplication
- Handles nested structures recursively

#### SkipArray()
```cpp
bool FPacketDeserializer::SkipArray(const TArray<uint8>& InBytes, int32 ArraySize)
```
- Extracts array skipping logic into reusable function
- Handles nested structures recursively

#### ReadCharacterData()
```cpp
bool FPacketDeserializer::ReadCharacterData(const TArray<uint8>& InBytes, FCharacterInfo& OutCharacter)
```
- Reads 16-field CharacterData structure from C# server
- Extracts only essential fields: CharacterId, Name, Race, Class, Level
- Skips nested objects: CharacterStats, CharacterPosition, CharacterAppearance, Equipment
- Forward compatible: accepts ≥16 fields, skips extras
- References C# schema location for maintainability

### 4. Updated Response Deserializers

Both `DeserializeCreateCharacterResponse()` and `DeserializeSelectCharacterResponse()` now:
- Use `PeekByte()` to check for Nil without consuming the byte
- Call `ReadCharacterData()` to properly deserialize the full CharacterData structure
- Handle null character data gracefully

## Files Changed
- `Source/Eldara/Networking/MessagePackFormat.h`: +5 lines (added map constants)
- `Source/Eldara/Networking/PacketDeserializer.h`: +11 lines (added declarations)
- `Source/Eldara/Networking/PacketDeserializer.cpp`: +170/-122 lines (core implementation)

## Key Features

### Complete MessagePack Support
✅ Handles all MessagePack map types (FixMap, Map16, Map32)  
✅ Recursive handling of nested structures  
✅ Proper byte consumption patterns

### Minimal Parsing Strategy
✅ Only extracts fields needed by FCharacterInfo  
✅ Efficiently skips unused nested objects  
✅ No unnecessary memory allocations

### Forward Compatibility
✅ Accepts schemas with >16 fields  
✅ Skips unknown future fields  
✅ Won't break if server adds optional data

### Safe Implementation
✅ No integer overflow risks  
✅ Clear, self-documenting constant names  
✅ Comprehensive error logging  
✅ No security vulnerabilities (CodeQL verified)

### Well Documented
✅ References C# schema location  
✅ Clear comments explaining field counts  
✅ Named constants for maintainability

## Testing Instructions

### Prerequisites
1. Rebuild the Unreal project
2. Start the C# server
3. Ensure server is running latest CharacterData schema

### Test Cases

#### Test 1: Character Creation
1. Open Unreal client
2. Login with valid credentials
3. Navigate to character creation screen
4. Create a new character with:
   - Name: "TestHero"
   - Race: Any
   - Class: Any
5. Submit creation request

**Expected Result:**
```
PacketDeserializer: Successfully read CharacterData - ID: 1, Name: TestHero, Race: 0, Class: 0, Level: 1
PacketDeserializer: CreateCharacterResponse - Result: 0, CharacterId: 1, Name: TestHero
```

#### Test 2: Character Selection
1. With characters in database
2. Login with valid credentials
3. View character list
4. Select a character

**Expected Result:**
```
PacketDeserializer: Successfully read CharacterData - ID: 1, Name: Hero, Race: 0, Class: 0, Level: 1
PacketDeserializer: SelectCharacterResponse - Result: 0, CharacterId: 1, Name: Hero
```

#### Test 3: Error Handling
1. Login with valid credentials
2. Try to create character with duplicate name

**Expected Result:**
```
PacketDeserializer: CreateCharacterResponse - Result: 10, Message: Name already taken, Character: null
```

### Verification Checklist
- [ ] No "Cannot skip unknown MessagePack type: 0x80" errors
- [ ] CreateCharacterResponse deserializes successfully
- [ ] SelectCharacterResponse deserializes successfully  
- [ ] Character data (ID, Name, Race, Class, Level) populated correctly
- [ ] No crashes or memory issues
- [ ] Server logs show successful packet delivery

## Performance Impact
- **Minimal**: SkipValue now does ~3 extra checks per call (FixMap, Map16, Map32)
- **Negligible**: Added function call overhead for SkipMap/SkipArray
- **Positive**: More efficient skipping of large nested structures

## Backward Compatibility
✅ **Fully backward compatible**  
- Still handles all previously supported MessagePack types
- No changes to existing packet structures
- Only adds support for map types

## Forward Compatibility
✅ **Forward compatible with server schema changes**  
- Accepts CharacterData with >16 fields
- Automatically skips unknown fields
- Won't break if server adds optional fields

## Schema Coupling
⚠️ **Tightly coupled to C# server schema**  
The deserializer requires exact field ordering and minimum field count (16) from:
```
Shared/WorldofEldara.Shared/Data/Character/CharacterData.cs
```

**If the server schema changes:**
1. Field count changes → Update `MinimumCharacterDataFields` constant
2. Field order changes → Update ReadCharacterData() field parsing
3. New required fields → Add parsing logic for new fields

## Known Limitations
1. **Field Order Dependency**: Fields must be in exact order as C# schema
2. **Minimum Field Count**: Requires at least 16 fields (forward compatible with more)
3. **Essential Fields Only**: Only extracts CharacterId, Name, Race, Class, Level
4. **No Nested Data**: Position, Stats, Appearance, Equipment are skipped

## Future Enhancements
If needed, the following can be added:
1. Parse CharacterPosition for spawn location
2. Parse CharacterStats for health/mana display
3. Parse CharacterAppearance for customization
4. Parse Equipment for visual representation
5. Schema versioning support
6. Dynamic field mapping

## Security Considerations
✅ **CodeQL verified** - No security vulnerabilities detected  
✅ **Safe integer handling** - No overflow risks  
✅ **Bounds checking** - All reads check buffer bounds  
✅ **No memory leaks** - All allocations properly managed  

## References
- MessagePack Specification: https://github.com/msgpack/msgpack/blob/master/spec.md
- C# Schema: `Shared/WorldofEldara.Shared/Data/Character/CharacterData.cs`
- Problem Statement: Issue detailing 0x80 deserialization error
- Code Review: All issues addressed and resolved

## Conclusion
This fix provides complete MessagePack map support, enabling successful deserialization of CharacterData in CreateCharacterResponse and SelectCharacterResponse packets. The implementation is safe, efficient, well-documented, and forward-compatible with server schema evolution.

**Status: ✅ Ready for Testing**
