#pragma once

#include "CoreMinimal.h"
#include "Engine/GameInstance.h"
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

	/** World state version for tracking server-side world changes */
	UPROPERTY(BlueprintReadOnly, Category = "World State")
	int32 WorldStateVersion;

protected:
	/** Initialize world state data */
	void InitializeWorldState();

	/** Resolve player character for save/load helpers */
	class AEldaraCharacterBase* GetPrimaryPlayerCharacter() const;
};
