#pragma once

#include "CoreMinimal.h"
#include "Engine/GameInstance.h"
#include "EldaraPersistenceProvider.h"
#include "EldaraGameInstance.generated.h"

/**
 * Game Instance for World of Eldara
 * Manages persistent game state, world state version, and session data
 */
UCLASS()
class ELDARA_API UEldaraGameInstance : public UGameInstance
{
	GENERATED_BODY()

public:
	UEldaraGameInstance();

	/** Called when the game instance is initialized */
	virtual void Init() override;

	/** Save current state to slot */
	UFUNCTION(BlueprintCallable, Category = "Save")
	bool SaveCurrentState(const FString& SlotName);

	/** Load a state and apply to world */
	UFUNCTION(BlueprintCallable, Category = "Save")
	bool LoadState(const FString& SlotName);

	/** Persist active quest progress (lightweight snapshot for local testing) */
	UFUNCTION(BlueprintCallable, Category = "Quest")
	bool SaveQuestProgressSnapshot(const FString& SlotName);

	/** Load quest progress into the quest subsystem */
	UFUNCTION(BlueprintCallable, Category = "Quest")
	bool LoadQuestProgressSnapshot(const FString& SlotName);

	UFUNCTION(BlueprintCallable, Category = "Quest")
	FString GetDefaultQuestSlotName() const;

	UFUNCTION(BlueprintCallable, Category = "Quest")
	int32 GetQuestSaveVersion() const;

	UFUNCTION(BlueprintCallable, Category = "Quest")
	FString BuildQuestSlotName(const FString& BaseSlot) const;

	/** World state version for tracking server-side world changes */
	UPROPERTY(BlueprintReadOnly, Category = "World State")
	int32 WorldStateVersion;

	/** Replace or retrieve the active persistence provider */
	UFUNCTION(BlueprintCallable, Category = "Persistence")
	void SetPersistenceProvider(TScriptInterface<IEldaraPersistenceProvider> InProvider) { PersistenceProvider = InProvider; }

	UFUNCTION(BlueprintCallable, Category = "Persistence")
	TScriptInterface<IEldaraPersistenceProvider> GetPersistenceProvider() const { return PersistenceProvider; }

protected:
	/** Initialize world state data */
	void InitializeWorldState();

	/** Resolve player character for save/load helpers */
	class AEldaraCharacterBase* GetPrimaryPlayerCharacter() const;

	/** Ensure we have a provider (creates default SaveGame provider) */
	void EnsurePersistenceProvider();

	/** Configurable provider class so backends can be swapped without code changes */
	UPROPERTY(EditDefaultsOnly, Category = "Persistence", meta = (MustImplement = "/Script/Eldara.EldaraPersistenceProvider"))
	TSubclassOf<UObject> DefaultPersistenceProviderClass;

	UPROPERTY()
	TScriptInterface<IEldaraPersistenceProvider> PersistenceProvider;

private:
	static constexpr int32 QuestSaveVersion = 1;
	static const FString DefaultQuestSlot;
};
