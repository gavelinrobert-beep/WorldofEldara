#include "EldaraPlayerController.h"
#include "../Data/EldaraCharacterCreatePayload.h"
#include "../Data/EldaraQuestData.h"

AEldaraPlayerController::AEldaraPlayerController()
{
}

void AEldaraPlayerController::BeginPlay()
{
	Super::BeginPlay();
	
	UE_LOG(LogTemp, Log, TEXT("EldaraPlayerController: Player controller started"));
}

void AEldaraPlayerController::RequestQuestAccept(UEldaraQuestData* QuestData)
{
	if (!QuestData)
	{
		UE_LOG(LogTemp, Warning, TEXT("RequestQuestAccept: QuestData is null"));
		return;
	}

	// TODO: Send quest accept request to server
	// TODO: Validate quest prerequisites (level, faction, previous quests)
	UE_LOG(LogTemp, Log, TEXT("RequestQuestAccept: %s"), *QuestData->QuestName.ToString());
}

void AEldaraPlayerController::Server_CreateCharacter_Implementation(const FEldaraCharacterCreatePayload& Payload)
{
	FString ErrorMessage;
	
	if (!ValidateCharacterCreation(Payload, ErrorMessage))
	{
		UE_LOG(LogTemp, Warning, TEXT("Server_CreateCharacter: Validation failed - %s"), *ErrorMessage);
		// TODO: Send error message back to client
		return;
	}

	// TODO: Create character data in database
	// TODO: Spawn character in starting zone
	// TODO: Apply appearance and stats
	// TODO: Grant starting abilities
	// TODO: Transition player to gameplay
	
	UE_LOG(LogTemp, Log, TEXT("Server_CreateCharacter: Character '%s' created successfully"), *Payload.CharacterName);
}

bool AEldaraPlayerController::Server_CreateCharacter_Validate(const FEldaraCharacterCreatePayload& Payload)
{
	// Basic validation for RPC
	return Payload.CharacterName.Len() > 0;
}

bool AEldaraPlayerController::ValidateCharacterCreation(const FEldaraCharacterCreatePayload& Payload, FString& OutErrorMessage)
{
	// Validate name
	if (!ValidateName(Payload.CharacterName, OutErrorMessage))
	{
		return false;
	}

	// Validate race/class combination
	if (!ValidateRaceClassCombo(Payload, OutErrorMessage))
	{
		return false;
	}

	// TODO: Validate appearance choices (bounds checking)
	// TODO: Check name uniqueness in database

	return true;
}

bool AEldaraPlayerController::ValidateName(const FString& Name, FString& OutErrorMessage)
{
	// Check length
	if (Name.Len() < 3)
	{
		OutErrorMessage = TEXT("Name must be at least 3 characters");
		return false;
	}

	if (Name.Len() > 20)
	{
		OutErrorMessage = TEXT("Name must be 20 characters or less");
		return false;
	}

	// TODO: Check for invalid characters
	// TODO: Check profanity filter
	// TODO: Check name uniqueness

	return true;
}

bool AEldaraPlayerController::ValidateRaceClassCombo(const FEldaraCharacterCreatePayload& Payload, FString& OutErrorMessage)
{
	if (!Payload.RaceData)
	{
		OutErrorMessage = TEXT("Race data is invalid");
		return false;
	}

	if (!Payload.ClassData)
	{
		OutErrorMessage = TEXT("Class data is invalid");
		return false;
	}

	// TODO: Check if ClassData is in RaceData's AllowedClasses list
	// TODO: Implement lore-based validation

	return true;
}
