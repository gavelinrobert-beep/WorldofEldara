# Character Creation

Character creation should be implemented in the custom client and validated by the authoritative server.

## First Vertical Slice

Start narrow:

- race: Sylvaen
- class: Memory Warden
- faction: Verdant Circles
- starter zone: Verdant Outskirts or Thornveil
- name entry
- basic appearance placeholder

## Client Responsibilities

- show available races/classes
- collect name and appearance choices
- build `CreateCharacterRequest`
- show validation errors from the server
- request character list after successful creation

## Server Responsibilities

- validate login/session state
- validate name length and allowed characters
- validate race/class/faction combinations
- assign starting zone and spawn point
- assign starting stats and abilities
- persist character data once persistence exists

## Protocol

Relevant shared packets:

- `CharacterListRequest`
- `CharacterListResponse`
- `CreateCharacterRequest`
- `CreateCharacterResponse`
- `SelectCharacterRequest`
- `SelectCharacterResponse`
- `PlayerSpawnPacket`
