#pragma once

#include "CoreMinimal.h"
#include "UObject/Interface.h"
#include "EldaraPersistenceTypes.h"
#include "EldaraPersistenceProvider.generated.h"

UINTERFACE(Blueprintable)
class ELDARA_API UEldaraPersistenceProvider : public UInterface
{
	GENERATED_BODY()
};

/**
 * Interface to persist Eldara runtime state.
 * Implementations can target SaveGame, PostgreSQL, or any other durable store.
 */
class ELDARA_API IEldaraPersistenceProvider
{
	GENERATED_BODY()

public:
	/** Persist full player snapshot including character state, inventory, quests, and world state */
	virtual bool SavePlayerSnapshot(const FString& SlotName, const FEldaraPlayerPersistenceSnapshot& Snapshot) = 0;

	/** Load full player snapshot */
	virtual bool LoadPlayerSnapshot(const FString& SlotName, FEldaraPlayerPersistenceSnapshot& OutSnapshot) = 0;

	/** Persist inventory independently */
	virtual bool SaveInventory(const FString& SlotName, const TArray<FEldaraInventoryItem>& Inventory) = 0;
	virtual bool LoadInventory(const FString& SlotName, TArray<FEldaraInventoryItem>& OutInventory) = 0;

	/** Persist quest progress independently */
	virtual bool SaveQuestProgress(const FString& SlotName, const TArray<FEldaraQuestProgress>& Progress) = 0;
	virtual bool LoadQuestProgress(const FString& SlotName, TArray<FEldaraQuestProgress>& OutProgress) = 0;

	/** Persist world state independently */
	virtual bool SaveWorldState(const FString& SlotName, const FEldaraWorldStateSnapshot& WorldState) = 0;
	virtual bool LoadWorldState(const FString& SlotName, FEldaraWorldStateSnapshot& OutWorldState) = 0;
};
