#pragma once

#include "CoreMinimal.h"
#include "EldaraPersistenceProvider.h"
#include "EldaraLocalPersistenceProvider.generated.h"

class UEldaraSaveGame;

/**
 * Default local SaveGame-backed provider.
 * Can be swapped with a DB-backed provider without touching gameplay code.
 * Operations are single-threaded and rewrite the full save payload, so coordinate callers to avoid conflicting writes.
 */
UCLASS()
class ELDARA_API UEldaraLocalPersistenceProvider : public UObject, public IEldaraPersistenceProvider
{
	GENERATED_BODY()

public:
	virtual bool SavePlayerSnapshot(const FString& SlotName, const FEldaraPlayerPersistenceSnapshot& Snapshot) override;
	virtual bool LoadPlayerSnapshot(const FString& SlotName, FEldaraPlayerPersistenceSnapshot& OutSnapshot) override;

	virtual bool SaveInventory(const FString& SlotName, const TArray<FEldaraInventoryItem>& Inventory) override;
	virtual bool LoadInventory(const FString& SlotName, TArray<FEldaraInventoryItem>& OutInventory) override;

	virtual bool SaveQuestProgress(const FString& SlotName, const TArray<FEldaraQuestProgress>& Progress) override;
	virtual bool LoadQuestProgress(const FString& SlotName, TArray<FEldaraQuestProgress>& OutProgress) override;

	virtual bool SaveWorldState(const FString& SlotName, const FEldaraWorldStateSnapshot& WorldState) override;
	virtual bool LoadWorldState(const FString& SlotName, FEldaraWorldStateSnapshot& OutWorldState) override;

private:
	UEldaraSaveGame* LoadExistingSave(const FString& SlotName) const;
	UEldaraSaveGame* LoadOrCreateSave(const FString& SlotName) const;
	bool PersistSave(const FString& SlotName, UEldaraSaveGame* SaveObject) const;
};
