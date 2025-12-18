# Character Creation System

## Overview

The character creation system allows players to create their avatar with race, class, faction, appearance, and name. The system is **data-driven**, **server-validated**, and **lore-constrained**.

## Design Principles

1. **Data-Driven**: Races, classes, and factions are defined in Data Assets, not hardcoded
2. **Server Authority**: All validation happens server-side to prevent invalid characters
3. **Lore Constraints**: Invalid combinations (e.g., Sylvaen Witch-King Acolyte) are rejected
4. **Appearance System**: Modular appearance slots with race-specific constraints
5. **Faction Alignment**: Each class has a primary faction, affecting starting zone and reputation

## Character Creation Flow

```
Player Login → Character List → Create New Character
    ↓
Race Selection (Data-Driven from UEldaraRaceData assets)
    ↓
Class Selection (Filtered by race, from UEldaraClassData assets)
    ↓
Faction Assignment (Automatic based on class's primary faction)
    ↓
Appearance Customization (Race-specific slots and options)
    ↓
Name Entry (Validation: length, profanity filter, uniqueness)
    ↓
Summary & Confirmation
    ↓
Server Validation → Character Spawn in Starter Zone
```

## Data Structures

### UEldaraRaceData (Data Asset)
- `FText DisplayName` (e.g., "Sylvaen")
- `FText Description` (lore description)
- `TArray<TSubclassOf<UEldaraClassData>> AllowedClasses` (e.g., Sylvaen can be Memory Warden, Grove Healer)
- `USkeletalMesh* BaseMesh` (race-specific skeleton)
- `TArray<FAppearanceSlot> AppearanceSlots` (hair, skin color, face shape, etc.)
- `UTexture2D* RaceIcon` (UI display)
- `FVector StartingZoneLocation` (where this race spawns)

### UEldaraClassData (Data Asset)
- `FText DisplayName` (e.g., "Memory Warden")
- `FText Description` (class fantasy and role)
- `EClassRole Role` (Tank, Healer, DPS, Hybrid)
- `UEldaraFactionData* PrimaryFaction` (e.g., The Covenant for Memory Warden)
- `TArray<FStartingAbility> StartingAbilities` (initial abilities)
- `FCharacterStats BaseStats` (starting health, resource, stats)
- `UTexture2D* ClassIcon` (UI display)

### UEldaraFactionData (Data Asset)
- `FText DisplayName` (e.g., "The Covenant")
- `FText Description` (faction philosophy)
- `FLinearColor FactionColor` (UI theming)
- `TArray<UEldaraZoneData*> ControlledZones` (faction territories)
- `int32 StartingReputation` (default rep with this faction)

### FEldaraCharacterCreatePayload (Network Struct)
```cpp
USTRUCT()
struct FAppearanceChoice
{
    UPROPERTY()
    FName SlotName;

    UPROPERTY()
    int32 OptionIndex;
};

USTRUCT()
struct FEldaraCharacterCreatePayload
{
    UPROPERTY()
    FString CharacterName;

    UPROPERTY()
    UEldaraRaceData* RaceData;

    UPROPERTY()
    UEldaraClassData* ClassData;

    UPROPERTY()
    TArray<FAppearanceChoice> AppearanceChoices; // RPC-safe array of slot/option pairs

    UPROPERTY()
    FLinearColor SkinTone;

    UPROPERTY()
    FLinearColor HairColor;
};
```

## UI Flow (Blueprint-Driven)

### Screen 1: Race Selection
- Display all race icons and names
- Show lore description on hover
- Highlight selected race
- Show available classes preview

### Screen 2: Class Selection
- Filter classes by selected race's `AllowedClasses`
- Display class icon, name, role (Tank/Healer/DPS)
- Show class description and faction affiliation
- Preview starting abilities

### Screen 3: Appearance Customization
- For each `AppearanceSlot` in RaceData:
  - Display options (e.g., 10 hairstyles, 8 face shapes)
  - Allow cycling through options
- Color pickers for skin tone, hair color
- Live 3D preview of character

### Screen 4: Name Entry
- Text input field
- Real-time validation feedback (length, characters, profanity)
- "Check Availability" button (optional server query)

### Screen 5: Summary & Confirmation
- Display all choices (race, class, faction, name, appearance)
- "Create Character" button
- Cancel/back button

## Server Validation (C++ in EldaraPlayerController)

### Server_CreateCharacter RPC
```cpp
UFUNCTION(Server, Reliable)
void Server_CreateCharacter(const FEldaraCharacterCreatePayload& Payload); // Validation executed inside the RPC body
```

### Validation Checks:
1. **Name Validation**:
   - Length: 3-20 characters
   - Characters: Alphanumeric + spaces (no special chars)
   - Profanity filter
   - Uniqueness check (query database)

2. **Race/Class Validation**:
   - Verify RaceData and ClassData are valid assets
   - Verify class is in race's `AllowedClasses` list
   - Reject invalid combinations (lore-breaking)

3. **Appearance Validation**:
   - Verify all appearance slot indices are within bounds
   - Verify color values are valid

4. **Faction Assignment**:
   - Auto-assign from ClassData's `PrimaryFaction`
   - Set starting reputation

### Spawning Logic:
```cpp
// 1. Create character data entry in database
// 2. Assign starting stats from ClassData->BaseStats
// 3. Grant starting abilities from ClassData->StartingAbilities
// 4. Spawn player pawn at RaceData->StartingZoneLocation
// 5. Apply appearance choices to character mesh
// 6. Transition to gameplay
```

## Appearance System

### Appearance Slots (Race-Specific):
- **Sylvaen**: Hair, Face, Ears, Bark Patterns, Leaf Accents
- **High Elves**: Hair, Face, Eyes (glow intensity), Runes
- **Humans**: Hair, Face, Body Type, Scars
- **Therakai**: Fur Pattern, Horns, Tail, Face, Teeth
- **Gronnak**: Tusks, Warpaint, Scars, Hair, Body Size
- **Void-Touched**: Phase Intensity, Eye Glow, Skin Shimmer, Face

### Slot Options:
Each slot has 5-15 options (e.g., 10 hairstyles, 8 face shapes)

### Color Options:
- Skin tone: Race-specific palette (e.g., Sylvaen has wood/bark tones)
- Hair color: Natural and fantasy colors
- Eye color: Race-specific (High Elves have glowing eyes)

## Faction & Starting Zone

### Faction Assignment:
- **The Covenant** → Thornveil Enclave (Sylvaen Memory Wardens, Grove Healers)
- **The Dominion** → The Scarred Highlands (Gronnak Witch-King Acolytes)
- **The Wandering Path** → The Untamed Reaches (Therakai Totem Shamans, Berserkers)
- **The Exiled Court** → Temporal Steppes (High Elf Temporal Mages, Chrono Knights)
- **The Free Realms** → Borderkeep (Human Unbound Warriors, Sellswords)
- **The Forsaken** → Voidrift Hollow (Void-Touched Stalkers, Entropy Weavers)

### Starting Zone Features:
- Faction-specific NPCs and architecture
- Intro quest chain (levels 1-3)
- Race-specific lore integration
- Transition to shared zones at level 10+

## Network Flow

1. **Client → Server**: `Server_CreateCharacter(Payload)`
2. **Server Validation**: Check all constraints, query database
3. **Server → Client**: 
   - Success: Spawn character, transition to game world
   - Failure: Return error code and message (e.g., "Name already taken")
4. **Client**: Display error or proceed to gameplay

## Error Handling

### Validation Errors:
- `ERR_NAME_TOO_SHORT`: "Name must be at least 3 characters"
- `ERR_NAME_TOO_LONG`: "Name must be 20 characters or less"
- `ERR_NAME_TAKEN`: "This name is already in use"
- `ERR_NAME_INVALID`: "Name contains invalid characters"
- `ERR_RACE_CLASS_INVALID`: "This race cannot be this class (lore constraint)"
- `ERR_APPEARANCE_INVALID`: "Invalid appearance options"
- `ERR_SERVER_ERROR`: "Character creation failed, please try again"

### Client-Side Pre-Validation:
- Client checks name length and characters *before* sending RPC
- Client filters class list by race
- Reduces server load and improves UX

## Implementation Files

### C++ Files:
- `Source/Eldara/Core/EldaraPlayerController.h/.cpp` (Server_CreateCharacter RPC)
- `Source/Eldara/Data/EldaraRaceData.h` (Race data asset)
- `Source/Eldara/Data/EldaraClassData.h` (Class data asset)
- `Source/Eldara/Data/EldaraFactionData.h` (Faction data asset)
- `Source/Eldara/Data/EldaraCharacterCreatePayload.h` (Payload struct)

### Blueprint Assets:
- `WBP_CharacterCreation` (Main UI widget)
- `WBP_RaceSelection` (Race picker)
- `WBP_ClassSelection` (Class picker)
- `WBP_AppearanceCustomizer` (Appearance editor)
- `WBP_NameEntry` (Name input)
- `WBP_CreationSummary` (Confirmation screen)

### Data Assets (To Be Created):
- `DA_Race_Sylvaen`, `DA_Race_HighElf`, etc. (Race definitions)
- `DA_Class_MemoryWarden`, `DA_Class_TemporalMage`, etc. (Class definitions)
- `DA_Faction_Covenant`, `DA_Faction_Dominion`, etc. (Faction definitions)

## Testing Checklist

- [ ] Valid race/class combinations create successfully
- [ ] Invalid race/class combinations are rejected
- [ ] Duplicate names are rejected
- [ ] Name profanity filter works
- [ ] Appearance choices persist from UI to spawned character
- [ ] Faction is assigned correctly based on class
- [ ] Starting zone spawn location is correct
- [ ] Starting abilities are granted
- [ ] Starting stats are applied
- [ ] Character appears in character list after creation

## Future Enhancements

- **Presets**: Save/load appearance presets
- **Randomize**: Random appearance generation
- **Preview Abilities**: Show ability animations during class selection
- **Intro Cinematic**: Faction-specific intro cutscene after creation
- **Legacy Characters**: Import character appearance from previous servers
