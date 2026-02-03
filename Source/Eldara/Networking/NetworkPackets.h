#pragma once

#include "CoreMinimal.h"
#include "NetworkTypes.h"
#include "NetworkPackets.generated.h"

// ============================================================================
// BASE PACKET
// ============================================================================

USTRUCT(BlueprintType)
struct FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 Timestamp = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 SequenceNumber = 0;
};

// ============================================================================
// AUTHENTICATION PACKETS
// ============================================================================

USTRUCT(BlueprintType)
struct FLoginRequest : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Username;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString PasswordHash;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ClientVersion;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ProtocolVersion;
};

// MessagePack deserialization structures
USTRUCT(BlueprintType)
struct FCharacterInfo
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
	int32 Level = 1;
};

USTRUCT(BlueprintType)
struct FLoginResponse : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EResponseCode Result = EResponseCode::Success;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Message;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 AccountId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString SessionToken;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ServerProtocolVersion;
};

// ============================================================================
// CHARACTER MANAGEMENT PACKETS
// ============================================================================

USTRUCT(BlueprintType)
struct FCharacterListRequest : public FPacketBase
{
	GENERATED_BODY()
	
	FCharacterListRequest() { }
};

USTRUCT(BlueprintType)
struct FCharacterListResponse : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EResponseCode Result = EResponseCode::Success;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<FCharacterInfo> Characters;
};

USTRUCT(BlueprintType)
struct FCreateCharacterRequest : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 AccountId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Name;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	ERace Race = ERace::Sylvaen;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EClass Class = EClass::MemoryWarden;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EFaction Faction = EFaction::VerdantCircles;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	ETotemSpirit TotemSpirit = ETotemSpirit::None;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCharacterAppearance Appearance;
};

USTRUCT(BlueprintType)
struct FCreateCharacterResponse : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EResponseCode Result = EResponseCode::Success;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Message;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCharacterInfo Character;
};

USTRUCT(BlueprintType)
struct FSelectCharacterRequest : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 CharacterId = 0;
};

USTRUCT(BlueprintType)
struct FSelectCharacterResponse : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EResponseCode Result = EResponseCode::Success;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Message;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCharacterInfo Character;
};

// ============================================================================
// MOVEMENT PACKETS
// ============================================================================

USTRUCT(BlueprintType)
struct FMovementInputPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 InputSequence = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float DeltaTime = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FMovementInput Input;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector PredictedPosition = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float PredictedRotationYaw = 0.0f;
};

USTRUCT(BlueprintType)
struct FMovementUpdatePacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 EntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector Position = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector Velocity = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float RotationYaw = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float RotationPitch = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EMovementState State = EMovementState::Idle;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 ServerTimestamp = 0;
};

USTRUCT(BlueprintType)
struct FPositionCorrectionPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 LastProcessedInput = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector AuthoritativePosition = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector AuthoritativeVelocity = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float AuthoritativeRotationYaw = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 ServerTimestamp = 0;
};

USTRUCT(BlueprintType)
struct FMovementSyncPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 EntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector Position = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector Velocity = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EMovementState State = EMovementState::Idle;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 ServerTimestamp = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ProtocolVersion;
};

USTRUCT(BlueprintType)
struct FMovementUpdateResponse : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 EntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector Position = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FRotator Rotation = FRotator::ZeroRotator;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector Velocity = FVector::ZeroVector;
};

// ============================================================================
// COMBAT PACKETS
// ============================================================================

USTRUCT(BlueprintType)
struct FUseAbilityRequest : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 AbilityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 TargetEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasTargetEntityId = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector TargetPosition = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasTargetPosition = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 InputSequence = 0;
};

USTRUCT(BlueprintType)
struct FAbilityResultPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EResponseCode Result = EResponseCode::Success;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 CasterEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 AbilityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 InputSequence = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Message;
};

USTRUCT(BlueprintType)
struct FDamagePacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 SourceEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 TargetEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 AbilityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	ENetDamageType DamageType = ENetDamageType::Physical;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Amount = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bIsCritical = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 RemainingHealth = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bIsFatal = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCombatEventMetadata Metadata;
};

USTRUCT(BlueprintType)
struct FHealingPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 SourceEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 TargetEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 AbilityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 Amount = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bIsCritical = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 RemainingHealth = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCombatEventMetadata Metadata;
};

USTRUCT(BlueprintType)
struct FStatusEffectPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 TargetEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 EffectId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bApplied = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 StackCount = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCombatEventMetadata Metadata;
};

USTRUCT(BlueprintType)
struct FThreatUpdatePacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 NPCEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 CurrentTargetId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TMap<int64, int32> ThreatTable;
};

USTRUCT(BlueprintType)
struct FCombatEventPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 EventType = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCombatEventMetadata Metadata;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FDamagePacket Damage;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasDamage = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FHealingPacket Healing;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasHealing = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FStatusEffectPacket StatusEffect;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasStatusEffect = false;
};

// ============================================================================
// CHAT PACKETS
// ============================================================================

USTRUCT(BlueprintType)
struct FChatMessagePacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EChatChannel Channel = EChatChannel::Say;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 SenderEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString SenderName;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Message;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 TargetEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasTargetEntityId = false;
};

// ============================================================================
// WORLD PACKETS
// ============================================================================

USTRUCT(BlueprintType)
struct FEnterWorldPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCharacterData Character;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ZoneId;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 ServerTime = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ProtocolVersion;
};

USTRUCT(BlueprintType)
struct FPlayerSpawnPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCharacterSnapshot Character;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FResourceSnapshot Resources;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ZoneId;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 ServerTime = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<FAbilitySummary> Abilities;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString ProtocolVersion;
};

USTRUCT(BlueprintType)
struct FLeaveWorldPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 EntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Reason;
};

USTRUCT(BlueprintType)
struct FEntitySpawnPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 EntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EEntityType Type = EEntityType::Player;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Name;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FVector Position = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	float RotationYaw = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FCharacterData CharacterData;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasCharacterData = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FNPCData NPCData;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasNPCData = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FResourceSnapshot Resources;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasResources = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<int32> AbilityIds;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasAbilityIds = false;
};

USTRUCT(BlueprintType)
struct FEntityDespawnPacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 EntityId = 0;
};

USTRUCT(BlueprintType)
struct FNPCStateUpdatePacket : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 EntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	ENPCState State = ENPCState::Idle;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 TargetEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasTargetEntityId = false;
};

// ============================================================================
// QUEST PACKETS
// ============================================================================

USTRUCT(BlueprintType)
struct FQuestAcceptRequest : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 QuestId = 0;
};

USTRUCT(BlueprintType)
struct FQuestAcceptResponse : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	EResponseCode Result = EResponseCode::Success;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString Message;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FQuestStateData State;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasState = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FQuestDefinition Definition;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasDefinition = false;
};

USTRUCT(BlueprintType)
struct FQuestLogSnapshot : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<FQuestDefinition> Definitions;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<FQuestStateData> States;
};

USTRUCT(BlueprintType)
struct FQuestProgressUpdate : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FQuestStateData State;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FQuestDefinition Definition;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasDefinition = false;
};

USTRUCT(BlueprintType)
struct FQuestDialogueRequest : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 NpcEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int32 NpcTemplateId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasNpcTemplateId = false;
};

USTRUCT(BlueprintType)
struct FQuestDialogueResponse : public FPacketBase
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	int64 NpcEntityId = 0;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	bool bHasNpcEntityId = false;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	FString NpcName;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<FQuestDialogueOption> Options;

	UPROPERTY(BlueprintReadWrite, Category = "Network")
	TArray<FQuestStateData> UpdatedStates;
};
