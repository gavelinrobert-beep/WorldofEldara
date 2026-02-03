# BP_ConnectTest Implementation Guide

## Overview
This guide provides step-by-step instructions for creating the BP_ConnectTest Blueprint with automatic character creation flow.

## Prerequisites
The following C++ functions are now implemented in `EldaraNetworkSubsystem`:
- ✅ `SendCharacterListRequest()` - Requests character list from server
- ✅ `SendCreateCharacter()` - Creates a new character
- ✅ `SendSelectCharacter()` - Selects a character to enter world

## Manual Blueprint Creation Instructions

### Step 1: Create the Blueprint
1. Open Unreal Editor
2. Navigate to `Content/WorldofEldara/Blueprints/`
3. Right-click → Blueprint Class → Actor
4. Name it `BP_ConnectTest`

### Step 2: Add Event Graph Nodes

In the Event Graph, create the following node sequence:

#### Initial Connection and Login
1. **Event BeginPlay**
2. **Get Game Instance** → **Cast To EldaraGameInstance**
3. **Get Network Subsystem** (from game instance)
4. **Connect To Server** (IP: "127.0.0.1", Port: 7777)
5. **Delay** (0.5 seconds) - Wait for connection
6. **Send Login** (Username: "testuser", PasswordHash: "testhash")

#### Character Selection Flow (NEW)
After the login sequence, add:

7. **Delay** (2.0 seconds) - Wait for login to complete

8. **Get Game Instance** → **Cast To EldaraGameInstance**

9. **Get Network Subsystem** (from game instance)

10. **Send Character List Request** (call on Network Subsystem)

11. **Delay** (2.0 seconds) - Wait for server response

12. **Make CharacterAppearance** struct with these values:
    - FaceType: 0
    - HairStyle: 0
    - HairColor: 0
    - SkinTone: 0
    - EyeColor: 0
    - Height: 1.0
    - BuildType: 0.5
    - FurPattern: 0
    - FurColor: 0
    - VoidIntensity: 0.0

13. **Send Create Character** (call on Network Subsystem) with:
    - Name: "Hero"
    - Race: Sylvaen (enum value 1)
    - Class: MemoryWarden (enum value 1)
    - Faction: VerdantCircles (enum value 1)
    - TotemSpirit: None (enum value 0)
    - Appearance: (plug in struct from step 12)

14. **Delay** (2.0 seconds) - Wait for character creation

15. **Send Select Character** (call on Network Subsystem)
    - CharacterId: 1

16. **Print String**: "Entering world with character..."
    - Duration: 5.0
    - Text Color: Green

## Expected Flow
1. Player starts game
2. Client connects to server (127.0.0.1:7777)
3. Client sends login with test credentials
4. ✨ **NEW:** Client requests character list
5. ✨ **NEW:** Client creates default character "Hero"
6. ✨ **NEW:** Client selects character ID 1
7. Player enters the game world

## Testing
After creating the Blueprint:

1. Start the C# server (`dotnet run` in Server directory)
2. Place `BP_ConnectTest` actor in a level
3. Play in Unreal Editor
4. Check Output Log for:
   - "EldaraNetworkSubsystem: Connected to 127.0.0.1:7777"
   - "EldaraNetworkSubsystem: Login request sent for user: testuser"
   - "EldaraNetworkSubsystem: Character list request sent"
   - "EldaraNetworkSubsystem: Create character request sent for 'Hero'"
   - "EldaraNetworkSubsystem: Select character request sent (ID: 1)"
   - "Entering world with character..."

## Notes
- The delays are hardcoded for POC - production should wait for actual server responses
- Character ID 1 is assumed for the first character created
- All enum values match the C++ definitions in NetworkTypes.h
- The appearance values are defaults suitable for a Sylvaen character

## Python Automation Script
A Python script `create_bp_connecttest.py` is available to automate this Blueprint creation using Unreal's Python API (must be run from within Unreal Editor).
