#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "EldaraPlayerController.generated.h"

// Forward declarations
struct FEldaraCharacterCreatePayload;
class UEldaraQuestData;

/**
 * Player Controller for World of Eldara
 * Handles player input, RPCs, and server communication
 */
UCLASS()
class ELDARA_API AEldaraPlayerController : public APlayerController
{
	GENERATED_BODY()

public:
	AEldaraPlayerController();

protected:
	virtual void BeginPlay() override;
	virtual void PlayerTick(float DeltaTime) override;

public:
	/**
	 * Request to accept a quest
	 * @param QuestData The quest data to accept
	 */
	UFUNCTION(BlueprintCallable, Category = "Quest")
	void RequestQuestAccept(UEldaraQuestData* QuestData);

	/**
	 * Server RPC: Create a new character
	 * @param Payload Character creation data
	 */
	UFUNCTION(Server, Reliable)
	void Server_CreateCharacter(const FEldaraCharacterCreatePayload& Payload);

private:
	/** Validate character creation payload */
	bool ValidateCharacterCreation(const FEldaraCharacterCreatePayload& Payload, FString& OutErrorMessage);

	/** Validate character name */
	bool ValidateName(const FString& Name, FString& OutErrorMessage);

	/** Validate race and class combination */
	bool ValidateRaceClassCombo(const FEldaraCharacterCreatePayload& Payload, FString& OutErrorMessage);

	/** Cached pointer to the network subsystem */
	UPROPERTY()
	class UEldaraNetworkSubsystem* CachedNetwork = nullptr;

	/** Cooldown before retrying subsystem lookup to avoid per-tick overhead */
	float NetworkLookupCooldown = 0.0f;
};
