#pragma once

#include "CoreMinimal.h"
#include "NetworkTypes.generated.h"

// ============================================================================
// ENUMS
// ============================================================================

UENUM(BlueprintType)
enum class EPacketType : uint16
{
	// Authentication (0-9)
	LoginRequest = 0,
	LoginResponse = 1,
	
	// Character Management (2-9)
	CharacterListRequest = 2,
	CharacterListResponse = 3,
	CreateCharacterRequest = 4,
	CreateCharacterResponse = 5,
	SelectCharacterRequest = 6,
	SelectCharacterResponse = 7,
	
	// Movement (10-19)
	MovementInput = 10,
	MovementUpdate = 11,
	PositionCorrection = 12,
	MovementSync = 13,
	
	// Combat (20-29)
	UseAbilityRequest = 20,
	AbilityResult = 21,
	Damage = 22,
	Healing = 23,
	StatusEffect = 24,
	ThreatUpdate = 25,
	CombatEvent = 26,
	
	// Chat (30-39)
	ChatMessage = 30,
	
	// World (100-199)
	EnterWorld = 100,
	LeaveWorld = 101,
	EntitySpawn = 102,
	EntityDespawn = 103,
	EntityUpdate = 104,
	PlayerSpawn = 105,
	NPCStateUpdate = 106,
	
	// Quest (250-299)
	QuestAcceptRequest = 250,
	QuestAcceptResponse = 251,
	QuestProgressUpdate = 252,
	QuestDialogueRequest = 254,
	QuestDialogueResponse = 255,
	QuestLogSnapshot = 256
};

UENUM(BlueprintType)
enum class EResponseCode : uint8
{
	Success = 0,
	Error = 1,
	InvalidRequest = 2,
	NotAuthenticated = 3,
	AlreadyExists = 4,
	NotFound = 5,
	InsufficientPermissions = 6,
	InvalidData = 7,
	ServerError = 8,
	Timeout = 9,
	
	// Character creation specific
	NameTaken = 10,
	InvalidName = 11,
	InvalidRaceClassCombination = 12,
	MaxCharactersReached = 13,
	
	// Combat specific
	NotInRange = 20,
	NotEnoughMana = 21,
	OnCooldown = 22,
	Interrupted = 23,
	InvalidTarget = 24,
	InsufficientResources = 25,
	
	// Lore-specific errors
	LoreInconsistency = 100
};

UENUM(BlueprintType)
enum class EChatChannel : uint8
{
	Say,
	Yell,
	Whisper,
	Party,
	Guild,
	Faction,
	System,
	Combat,
	Emote
};

UENUM(BlueprintType)
enum class EEntityType : uint8
{
	Player,
	NPC,
	Monster,
	Object,
	Vehicle,
	Pet
};

UENUM(BlueprintType)
enum class ENPCState : uint8
{
	Idle = 0,
	Patrol = 1,
	Combat = 2
};

UENUM(BlueprintType)
enum class EMovementState : uint8
{
	Idle,
	Walking,
	Running,
	Jumping,
	Falling,
	Swimming,
	Flying,
	Mounted,
	Stunned,
	Rooted
};

UENUM(BlueprintType)
enum class ENetDamageType : uint8
{
	Physical,
	Nature,
	Radiant,
	Holy,
	Necrotic,
	Arcane,
	Fire,
	Frost,
	Lightning,
	Void,
	Shadow,
	Spirit,
	Temporal
};

// ============================================================================
// PLACEHOLDER ENUMS (TODO: Fill in from C# definitions)
// ============================================================================

UENUM(BlueprintType)
enum class ERace : uint8
{
	None = 0,
	Sylvaen = 1,
	HighElf = 2,
	Human = 3,
	Therakai = 4,
	Gronnak = 5,
	VoidTouched = 6
};

UENUM(BlueprintType)
enum class EClass : uint8
{
	None = 0,
	MemoryWarden = 1,
	TemporalMage = 2,
	UnboundWarrior = 3,
	TotemShaman = 4,
	Berserker = 5,
	WitchKingAcolyte = 6,
	VoidStalker = 7,
	RootDruid = 8,
	ArchonKnight = 9,
	FreeBlade = 10,
	GodSeekerCleric = 11,
	MemoryThief = 12
};

UENUM(BlueprintType)
enum class EFaction : uint8
{
	Neutral = 0,
	VerdantCircles = 1,
	AscendantLeague = 2,
	UnitedKingdoms = 3,
	TotemClansWildborn = 4,
	TotemClansPathbound = 5,
	DominionWarhost = 6,
	VoidCompact = 7
};

UENUM(BlueprintType)
enum class ETotemSpirit : uint8
{
	None = 0,
	Fanged = 1,
	Horned = 2,
	Clawed = 3,
	Winged = 4
};

// ============================================================================
// DATA STRUCTS (Data Transfer Objects)
// ============================================================================

USTRUCT(BlueprintType)
struct FMovementInput
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float Forward = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float Strafe = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bJump = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bSprint = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float LookYaw = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float LookPitch = 0.0f;
};

USTRUCT(BlueprintType)
struct FCharacterAppearance
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 FaceType = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 HairStyle = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 HairColor = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 SkinTone = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 EyeColor = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float Height = 1.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float BuildType = 1.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 FurPattern = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 FurColor = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float VoidIntensity = 0.0f;
};

USTRUCT(BlueprintType)
struct FResourceSnapshot
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 MaxHealth = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 CurrentHealth = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 MaxMana = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 CurrentMana = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 MaxStamina = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 CurrentStamina = 0;
};

USTRUCT(BlueprintType)
struct FAbilitySummary
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 AbilityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Name;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 AbilityType = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 MagicSource = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	ENetDamageType DamageType = ENetDamageType::Physical;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 TargetType = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float Range = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float Cooldown = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 ManaCost = 0;
};

USTRUCT(BlueprintType)
struct FCharacterPosition
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ZoneId;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float X = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float Y = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float Z = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float RotationYaw = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float RotationPitch = 0.0f;
};

USTRUCT(BlueprintType)
struct FCharacterData
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 CharacterId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 AccountId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Name;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	ERace Race = ERace::None;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EClass Class = EClass::None;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EFaction Faction = EFaction::Neutral;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Level = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 ExperiencePoints = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCharacterPosition Position;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCharacterAppearance Appearance;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	ETotemSpirit TotemSpirit = ETotemSpirit::None;
};

USTRUCT(BlueprintType)
struct FCharacterSnapshot
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 CharacterId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Name;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	ERace Race = ERace::None;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EClass Class = EClass::None;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EFaction Faction = EFaction::Neutral;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Level = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FResourceSnapshot Resources;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCharacterPosition Position;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<int32> KnownAbilities;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ZoneId;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Version;
};

USTRUCT(BlueprintType)
struct FNPCData
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 NPCTemplateId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Name;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Level = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EFaction Faction = EFaction::Neutral;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bIsHostile = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bIsQuestGiver = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bIsVendor = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 MaxHealth = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 CurrentHealth = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FResourceSnapshot Resources;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<int32> AbilityIds;
};

USTRUCT(BlueprintType)
struct FQuestObjectiveProgress
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 ObjectiveId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Current = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Target = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bCompleted = false;
};

USTRUCT(BlueprintType)
struct FQuestStateData
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 QuestId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 State = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<FQuestObjectiveProgress> Objectives;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString AcceptedAt;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString CompletedAt;
};

USTRUCT(BlueprintType)
struct FQuestObjectiveDefinition
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 ObjectiveId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 ObjectiveType = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Description;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 TargetCount = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 TargetNpcTemplateId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString TargetTag;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bOptional = false;
};

USTRUCT(BlueprintType)
struct FNetQuestReward
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Experience = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Gold = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EFaction ReputationFaction = EFaction::Neutral;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 ReputationAmount = 0;
};

USTRUCT(BlueprintType)
struct FQuestDefinition
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 QuestId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Title;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Description;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 MinimumLevel = 1;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bIsRepeatable = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bIsMainStory = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<int32> Prerequisites;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<FQuestObjectiveDefinition> Objectives;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FNetQuestReward Rewards;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 GiverNpcTemplateId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 TurnInNpcTemplateId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString AcceptDialogue;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString CompletionDialogue;
};

USTRUCT(BlueprintType)
struct FQuestDialogueOption
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Type = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Text;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 QuestId = 0;
};

USTRUCT(BlueprintType)
struct FCombatEventMetadata
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString EventId;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 ServerTime = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ProtocolVersion;
};
