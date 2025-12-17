#pragma once

#include "CoreMinimal.h"
#include "EldaraCharacterBase.h"
#include "Data/EldaraFaction.h"
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

	/** Display name for this NPC */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "NPC")
	FText NPCName;

	/** Faction alignment for this NPC */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "NPC")
	EEldaraFaction Faction = EEldaraFaction::Neutral;
};
