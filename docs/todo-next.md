# TODO: Next Steps

This document outlines the prioritized backlog for implementing the foundation systems after the C++ scaffolding and documentation are complete.

## Phase 1: Core Framework Setup (Week 1-2)

### GameMode & Player Controller
- [ ] Wire up `AEldaraGameModeBase` default classes
  - Set default pawn class to `AEldaraCharacterBase`
  - Set default player controller to `AEldaraPlayerController`
  - Set default game state and player state classes (to be created)
- [ ] Implement `AEldaraPlayerController::Server_CreateCharacter` validation
  - Add name validation (length, characters, uniqueness)
  - Add race/class lore validation (check AllowedClasses)
  - Add appearance validation (bounds checking)
  - Return error codes for invalid payloads
- [ ] Implement character spawning logic in GameMode
  - Parse `FEldaraCharacterCreatePayload`
  - Spawn `AEldaraCharacterBase` at starting zone location
  - Apply RaceData and ClassData to character
  - Apply appearance choices to character mesh

### Game Instance
- [ ] Add world state persistence to `UEldaraGameInstance`
  - Implement save/load for `WorldStateVersion`
  - Add server connection state tracking
  - Add player session data (account ID, character list)

## Phase 2: Data Assets (Week 2-3)

### Create Initial Data Assets
- [ ] **Races**: Create one race for vertical slice testing
  - `DA_Race_Sylvaen` (Whispering Canopy starter zone)
  - Define allowed classes: Memory Warden, Grove Healer
  - Define appearance slots: Hair, Face, Ears, Bark Patterns
  - Set base mesh and starting zone location
- [ ] **Classes**: Create one class for vertical slice
  - `DA_Class_MemoryWarden` (Tank role, Covenant faction)
  - Define base stats (health, mana, armor)
  - Define starting abilities (Shield Slam, Thorn Strike)
- [ ] **Factions**: Create Covenant faction
  - `DA_Faction_Covenant` (preservation philosophy)
  - Define starting reputation and controlled zones
- [ ] **Zones**: Create Whispering Canopy zone data
  - `DA_Zone_WhisperingCanopy` (Sylvaen starter zone)
  - Define level range (1-10), biome, faction ownership
- [ ] **Quests**: Create first quest for testing
  - `DA_Quest_TheWorldrootsPain` (tutorial quest)
  - Define objectives, rewards, quest text

### Data Asset Validation
- [ ] Add editor validation for data assets
  - Ensure all required fields are filled
  - Check for invalid references (e.g., null abilities)
  - Warn if race/class combinations violate lore

## Phase 3: Character Creation Flow (Week 3-4)

### UI Implementation (Blueprint)
- [ ] Create `WBP_CharacterCreation` main widget
- [ ] Create `WBP_RaceSelection` screen
  - Display race icons and names
  - Show race description on hover
  - Filter to available races (for vertical slice, just Sylvaen)
- [ ] Create `WBP_ClassSelection` screen
  - Filter classes by selected race
  - Display class icon, name, role (Tank/Healer/DPS)
  - Show class description and faction
- [ ] Create `WBP_AppearanceCustomizer` screen
  - Display appearance slot options (hair, face, etc.)
  - Add color pickers for skin tone and hair color
  - Show 3D character preview
- [ ] Create `WBP_NameEntry` screen
  - Add text input with validation feedback
  - Check name length and characters
- [ ] Create `WBP_CreationSummary` screen
  - Display all choices for confirmation
  - Add "Create Character" button

### Client-Server Integration
- [ ] Connect UI to `Server_CreateCharacter` RPC
- [ ] Build `FEldaraCharacterCreatePayload` from UI selections
- [ ] Handle server response (success or error)
- [ ] Transition to gameplay on success
- [ ] Display error message on failure

## Phase 4: Character & Combat Foundation (Week 4-6)

### Character Base Implementation
- [ ] Implement `AEldaraCharacterBase` stats system
  - Add replicated properties for Health, MaxHealth, Resource, MaxResource
  - Implement stat calculation from RaceData and ClassData
  - Add death state handling
- [ ] Apply RaceData to character
  - Set skeletal mesh from RaceData
  - Apply appearance choices to mesh
- [ ] Apply ClassData to character
  - Set base stats from ClassData
  - Grant starting abilities

### Combat Component
- [ ] Implement `UEldaraCombatComponent::CanActivateAbility`
  - Check cooldown status
  - Check resource availability
  - Check target range and line-of-sight
  - Check crowd control status
- [ ] Implement `UEldaraCombatComponent::Server_ActivateAbility`
  - Validate ability activation
  - Deduct resource cost
  - Trigger cooldown
  - Apply effects to target(s)
- [ ] Implement `UEldaraCombatComponent::ApplyEffect`
  - Parse effect data from `UEldaraEffect`
  - Apply damage, healing, buffs, debuffs
  - Handle DoT/HoT ticking
  - Track active effects and durations

### Ability System
- [ ] Create first test ability
  - `DA_Ability_ShieldSlam` (Memory Warden starting ability)
  - Single-target, instant, melee range
  - Low cooldown (5s), low mana cost
  - Applies damage effect
- [ ] Create first test effect
  - `DA_Effect_PhysicalDamage` (instant damage)
  - Base magnitude: 50 damage
  - No duration (instant application)

### Combat Testing
- [ ] Create test dummy NPC
  - Stationary, does not attack back
  - High health for sustained testing
  - Displays damage taken
- [ ] Test ability activation flow
  - Client sends RPC
  - Server validates
  - Server applies damage
  - Client receives damage feedback

## Phase 5: AI Foundation (Week 6-7)

### AI Controller Setup
- [ ] Implement `AEldaraAIController::OnPossess`
  - Initialize Behavior Tree and Blackboard
  - Set up perception component
  - Initialize threat table
- [ ] Create `BB_EnemyBasic` Blackboard asset
  - Add keys from `EldaraAIKeys.h`
  - Configure default values
- [ ] Create `BT_EnemyBasic` Behavior Tree
  - Implement patrol logic (idle state)
  - Implement chase logic (pursue target)
  - Implement attack logic (basic attack)

### First Enemy NPC
- [ ] Create test enemy character
  - Derives from `AEldaraCharacterBase`
  - Low health, basic stats
  - Uses `AEldaraAIController`
- [ ] Configure perception
  - Sight range: 1500 cm
  - Hearing range: 1000 cm
  - Can sense players
- [ ] Test basic AI
  - Patrols when no target
  - Chases player when perceived
  - Attacks player when in range
  - Returns to patrol when player leaves

### Combat AI
- [ ] Add basic attack behavior
  - AI uses melee attack on player
  - AI tracks cooldown
- [ ] Add ability usage
  - AI selects ability if available
  - AI casts ability on player

## Phase 6: Starter Zone - Whispering Canopy (Week 7-12)

### Environment Art (Greybox First)
- [ ] Block out terrain (ground level, root pathways, canopy)
- [ ] Place key landmarks (Great Root Arch, Corrupted Wound, Verdant Shrine)
- [ ] Add lighting (bioluminescent flora, ambient lighting)
- [ ] Add patrol paths for enemies

### Quest Implementation
- [ ] Create NPC: Elder Tharivol
  - Quest giver for main quest chain
  - Dialogue system integration
- [ ] Implement Quest 1: "Awakening"
  - Objective: Speak to Elder Tharivol
  - Reward: Basic weapon
- [ ] Implement Quest 2: "The Worldroot's Pain"
  - Objective: Kill 8 Corrupted Squirrels
  - Reward: XP, basic armor piece
- [ ] Implement quest tracking UI
  - Display active quests
  - Show objectives and progress
  - Mark quest locations on map

### Enemy Population
- [ ] Create enemy: Corrupted Squirrel
  - Low health, low damage
  - Basic AI (chase and attack)
  - Drops: Quest item (Corruption Sample)
- [ ] Place enemies in zone
  - Spread out for solo questing
  - Respawn time: 30 seconds

### Boss Implementation (Phase 2 of Zone)
- [ ] Create Sorrow Stag boss
  - High health, tank-buster attacks
  - Phase transitions at 60% and 25% health
  - Telegraphed AoE attacks
- [ ] Implement boss arena
  - Verdant Shrine location
  - Phase transition visual effects
- [ ] Test boss mechanics
  - Phase 1: Basic charge and swipe
  - Phase 2: Summon adds, ground AoE
  - Phase 3: Enrage, Corruption aura

## Phase 7: Polish & Testing (Week 12-14)

### Performance Optimization
- [ ] Profile frame rate in Whispering Canopy
- [ ] Optimize AI tick rate
- [ ] Optimize particle effects
- [ ] Add LOD for distant meshes

### Balance Tuning
- [ ] Tune enemy health and damage for solo play
- [ ] Tune ability cooldowns and costs
- [ ] Tune quest XP to reach level 10 by zone completion
- [ ] Tune boss difficulty

### Bug Fixing
- [ ] Fix any combat bugs (abilities not activating, incorrect damage)
- [ ] Fix any AI bugs (enemies stuck, not pathing correctly)
- [ ] Fix any quest bugs (objectives not tracking, rewards not granted)

### Playtesting
- [ ] Internal playtest with fresh accounts
- [ ] Collect feedback on difficulty, clarity, engagement
- [ ] Iterate based on feedback

## Phase 8: Code Review & Security (Week 14-15)

### Code Review
- [ ] Run code review tool on all C++ changes
- [ ] Address feedback from code review
- [ ] Refactor any unclear or complex code

### Security Scan
- [ ] Run CodeQL security scanner
- [ ] Fix any security vulnerabilities discovered
- [ ] Ensure all RPCs have validation
- [ ] Ensure no client-authoritative logic

### Documentation
- [ ] Update architecture.md with any changes
- [ ] Document any new systems added
- [ ] Add inline code comments for complex logic

## Phase 9: Milestone Delivery (Week 15)

### Deliverables
- [ ] Fully functional character creation (Sylvaen + Memory Warden only)
- [ ] Working combat system (basic abilities, damage, healing)
- [ ] Functional AI (patrol, aggro, attack)
- [ ] Whispering Canopy starter zone (levels 1-10)
- [ ] First boss (Sorrow Stag)
- [ ] Documentation complete (all markdown files)

### Success Criteria
- [ ] Player can create a Sylvaen Memory Warden character
- [ ] Player can complete main quest chain in Whispering Canopy
- [ ] Player can reach level 10
- [ ] Player can defeat Sorrow Stag boss
- [ ] No critical bugs in main path
- [ ] 60 FPS on target hardware

## Future Phases (Post-Milestone)

### Expand Character Creation
- [ ] Add remaining races (High Elf, Human, Therakai, Gronnak, Void-Touched)
- [ ] Add remaining classes (12 total)
- [ ] Create starter zones for each race

### Expand Combat
- [ ] Add combo system
- [ ] Add interrupt mechanics
- [ ] Add reflect/absorb effects
- [ ] Add PvP system

### Expand AI
- [ ] Add squad AI coordination
- [ ] Add environmental awareness
- [ ] Add dynamic difficulty scaling

### Expand World
- [ ] Create mid-level zones (10-20, 20-30, etc.)
- [ ] Create capital cities
- [ ] Create first raid dungeon

### Add Systems
- [ ] Inventory system
- [ ] Equipment system
- [ ] Crafting system
- [ ] Guild system
- [ ] Auction house
- [ ] Mail system

---

**Priority:** Focus on Phase 1-4 first to get core systems working, then move to Phase 5-6 for content. Phases 7-9 are polish and delivery.

**Timeline:** 15 weeks to first vertical slice milestone (Whispering Canopy fully playable with one race/class).

**Team Size Estimate:** This is a large scope. For a solo developer, expect 6-12 months. For a small team (3-5), expect 3-6 months.
