#pragma once

#include "CoreMinimal.h"
#include "EldaraCharacterBase.h"
#include "Eldara/Data/EldaraFaction.h"
#include "Eldara/Networking/EldaraProtocolTypes.h"
#include "EldaraNPCBase.generated.h"

/**
 * Base class for all NPCs in Eldara
 */
UCLASS(Blueprintable)
class ELDARA_API AEldaraNPCBase : public AEldaraCharacterBase
{
	GENERATED_BODY()

public:
	AEldaraNPCBase();

	/** Unique identifier for this NPC (for quest tracking, dialogue, etc.) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "NPC")
	FName NPCId;

	/** Display name for this NPC */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "NPC")
	FText NPCName;

	/** Faction alignment for this NPC */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "NPC")
	EEldaraFaction Faction = EEldaraFaction::Neutral;

	/** Server-driven state for visualization */
	UPROPERTY(BlueprintReadOnly, Category = "NPC")
	EEldaraNPCServerState ServerState = EEldaraNPCServerState::Idle;

	/** Apply authoritative state update from server */
	void ApplyServerState(EEldaraNPCServerState NewState) { ServerState = NewState; }
};
