#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "EldaraPersistenceTypes.h"
#include "EldaraSaveGame.generated.h"

class UEldaraRaceData;
class UEldaraClassData;

/**
 * Basic save game payload for Eldara.
 * Stores player identity, progression, and location so designers can extend in Blueprints.
 */
UCLASS()
class ELDARA_API UEldaraSaveGame : public USaveGame
{
	GENERATED_BODY()

public:
	/** Character name for the slot */
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	FString CharacterName;

	/** Selected race/class for restore (stored as soft references for safe serialization) */
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	TSoftObjectPtr<UEldaraRaceData> RaceData;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	TSoftObjectPtr<UEldaraClassData> ClassData;

	/** Core progression */
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	int32 Level = 1;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	int32 Experience = 0;

	/** Vital state */
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	float Health = 0.0f;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	float Resource = 0.0f;

	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	float Stamina = 0.0f;

	/** Transform in the world */
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	FTransform SavedTransform;

	/** Inventory snapshot (string IDs for now; replace with item definitions when available) */
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	TArray<FEldaraInventoryItem> Inventory;

	/** Quest progress snapshot */
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	TArray<FEldaraQuestProgress> QuestProgress;

	/** World state snapshot */
	UPROPERTY(VisibleAnywhere, BlueprintReadWrite, Category = "Save")
	FEldaraWorldStateSnapshot WorldState;

	/** Copy data from a persistence snapshot into this SaveGame instance */
	void ApplySnapshot(const FEldaraPlayerPersistenceSnapshot& Snapshot);

	/** Produce a persistence snapshot representing this SaveGame instance */
	FEldaraPlayerPersistenceSnapshot ToSnapshot() const;
};
