#include "EldaraLocalPersistenceProvider.h"

#include "EldaraSaveGame.h"
#include "Kismet/GameplayStatics.h"

UEldaraSaveGame* UEldaraLocalPersistenceProvider::LoadExistingSave(const FString& SlotName) const
{
	if (!UGameplayStatics::DoesSaveGameExist(SlotName, 0))
	{
		return nullptr;
	}

	return Cast<UEldaraSaveGame>(UGameplayStatics::LoadGameFromSlot(SlotName, 0));
}

UEldaraSaveGame* UEldaraLocalPersistenceProvider::LoadOrCreateSave(const FString& SlotName) const
{
	if (UEldaraSaveGame* Existing = LoadExistingSave(SlotName))
	{
		return Existing;
	}

	return Cast<UEldaraSaveGame>(UGameplayStatics::CreateSaveGameObject(UEldaraSaveGame::StaticClass()));
}

bool UEldaraLocalPersistenceProvider::PersistSave(const FString& SlotName, UEldaraSaveGame* SaveObject) const
{
	if (!SaveObject)
	{
		return false;
	}

	return UGameplayStatics::SaveGameToSlot(SaveObject, SlotName, 0);
}

bool UEldaraLocalPersistenceProvider::SavePlayerSnapshot(const FString& SlotName, const FEldaraPlayerPersistenceSnapshot& Snapshot)
{
	UEldaraSaveGame* SaveObject = LoadOrCreateSave(SlotName);
	if (!SaveObject)
	{
		return false;
	}

	SaveObject->ApplySnapshot(Snapshot);
	return PersistSave(SlotName, SaveObject);
}

bool UEldaraLocalPersistenceProvider::LoadPlayerSnapshot(const FString& SlotName, FEldaraPlayerPersistenceSnapshot& OutSnapshot)
{
	if (UEldaraSaveGame* SaveObject = LoadExistingSave(SlotName))
	{
		OutSnapshot = SaveObject->ToSnapshot();
		return true;
	}

	return false;
}

bool UEldaraLocalPersistenceProvider::SaveInventory(const FString& SlotName, const TArray<FEldaraInventoryItem>& Inventory)
{
	UEldaraSaveGame* SaveObject = LoadOrCreateSave(SlotName);
	if (!SaveObject)
	{
		return false;
	}

	SaveObject->Inventory = Inventory;
	return PersistSave(SlotName, SaveObject);
}

bool UEldaraLocalPersistenceProvider::LoadInventory(const FString& SlotName, TArray<FEldaraInventoryItem>& OutInventory)
{
	if (UEldaraSaveGame* SaveObject = LoadExistingSave(SlotName))
	{
		OutInventory = SaveObject->Inventory;
		return true;
	}

	return false;
}

bool UEldaraLocalPersistenceProvider::SaveQuestProgress(const FString& SlotName, const TArray<FEldaraQuestProgress>& Progress)
{
	UEldaraSaveGame* SaveObject = LoadOrCreateSave(SlotName);
	if (!SaveObject)
	{
		return false;
	}

	SaveObject->QuestProgress = Progress;
	return PersistSave(SlotName, SaveObject);
}

bool UEldaraLocalPersistenceProvider::LoadQuestProgress(const FString& SlotName, TArray<FEldaraQuestProgress>& OutProgress)
{
	if (UEldaraSaveGame* SaveObject = LoadExistingSave(SlotName))
	{
		OutProgress = SaveObject->QuestProgress;
		return true;
	}

	return false;
}

bool UEldaraLocalPersistenceProvider::SaveWorldState(const FString& SlotName, const FEldaraWorldStateSnapshot& WorldState)
{
	UEldaraSaveGame* SaveObject = LoadOrCreateSave(SlotName);
	if (!SaveObject)
	{
		return false;
	}

	SaveObject->WorldState = WorldState;
	return PersistSave(SlotName, SaveObject);
}

bool UEldaraLocalPersistenceProvider::LoadWorldState(const FString& SlotName, FEldaraWorldStateSnapshot& OutWorldState)
{
	if (UEldaraSaveGame* SaveObject = LoadExistingSave(SlotName))
	{
		OutWorldState = SaveObject->WorldState;
		return true;
	}

	return false;
}
