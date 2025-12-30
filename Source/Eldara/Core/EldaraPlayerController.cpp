#include "EldaraPlayerController.h"
#include "Eldara/Data/EldaraCharacterCreatePayload.h"
#include "Eldara/Data/EldaraQuestData.h"
#include "Eldara/Networking/EldaraNetworkSubsystem.h"
#include "Eldara/UI/WorldHUDWidget.h"
#include "Eldara/Characters/EldaraCharacterBase.h"
#include "Internationalization/Text.h"

#define LOCTEXT_NAMESPACE "AEldaraPlayerController"

namespace
{
constexpr float NetworkLookupInterval = 1.0f;
constexpr float MinimapLocationTolerance = 10.f;

TArray<FText> BuildDefaultActionLabels()
{
	TArray<FText> Labels;
	Labels.Reserve(6);
	Labels.Add(LOCTEXT("Action1", "1: Attack"));
	Labels.Add(LOCTEXT("Action2", "2: Block"));
	Labels.Add(LOCTEXT("Action3", "3: Interrupt"));
	Labels.Add(LOCTEXT("Action4", "4: Potion"));
	Labels.Add(LOCTEXT("Action5", "5: Mount"));
	Labels.Add(LOCTEXT("Action6", "6: Map"));
	return Labels;
}
}

AEldaraPlayerController::AEldaraPlayerController()
{
}

void AEldaraPlayerController::BeginPlay()
{
	Super::BeginPlay();
	
	CachedNetwork = GetGameInstance()
		? GetGameInstance()->GetSubsystem<UEldaraNetworkSubsystem>()
		: nullptr;

	EnsureHUD();

	UE_LOG(LogTemp, Log, TEXT("EldaraPlayerController: Player controller started"));
}

void AEldaraPlayerController::PlayerTick(float DeltaTime)
{
	Super::PlayerTick(DeltaTime);

	if (!HUDWidget)
	{
		EnsureHUD();
	}
	UpdateHUD();

	if (!CachedNetwork)
	{
		NetworkLookupCooldown -= DeltaTime;
		if (NetworkLookupCooldown <= 0.f)
		{
			CachedNetwork = GetGameInstance()
				? GetGameInstance()->GetSubsystem<UEldaraNetworkSubsystem>()
				: nullptr;
			NetworkLookupCooldown = NetworkLookupInterval;
		}
	}

	UEldaraNetworkSubsystem* Network = CachedNetwork;
	if (!Network)
	{
		return;
	}

	APawn* ControlledPawn = GetPawn();
	if (!ControlledPawn)
	{
		return;
	}

	const FVector InputVector = ControlledPawn->GetLastMovementInputVector();
	const FVector PawnLocation = ControlledPawn->GetActorLocation();
	const FRotator ControlRot = GetControlRotation();
	Network->SendMovementInput(FVector2D(InputVector.X, InputVector.Y), ControlRot, DeltaTime, PawnLocation);
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

void AEldaraPlayerController::EnsureHUD()
{
	if (HUDWidget)
	{
		return;
	}

	HUDWidget = CreateWidget<UWorldHUDWidget>(this, UWorldHUDWidget::StaticClass());
	if (HUDWidget)
	{
		HUDWidget->AddToViewport();

		HUDWidget->SetActionBarLabels(BuildDefaultActionLabels());
	}
}

void AEldaraPlayerController::UpdateHUD()
{
	if (!HUDWidget)
	{
		return;
	}

	AEldaraCharacterBase* Character = Cast<AEldaraCharacterBase>(GetPawn());
	if (!Character)
	{
		return;
	}

	const float CurrentHealth = Character->GetHealth();
	const float CurrentMaxHealth = Character->GetMaxHealth();
	const float CurrentResource = Character->GetResource();
	const float CurrentMaxResource = Character->GetMaxResource();
	const bool bVitalsChanged =
		!bHasCachedVitals ||
		!FMath::IsNearlyEqual(CurrentHealth, LastHealth) ||
		!FMath::IsNearlyEqual(CurrentMaxHealth, LastMaxHealth) ||
		!FMath::IsNearlyEqual(CurrentResource, LastResource) ||
		!FMath::IsNearlyEqual(CurrentMaxResource, LastMaxResource);

	if (bVitalsChanged)
	{
		HUDWidget->UpdateVitals(CurrentHealth, CurrentMaxHealth, CurrentResource, CurrentMaxResource);
		LastHealth = CurrentHealth;
		LastMaxHealth = CurrentMaxHealth;
		LastResource = CurrentResource;
		LastMaxResource = CurrentMaxResource;
		bHasCachedVitals = true;
	}

	const FVector CurrentLocation = Character->GetActorLocation();
	const bool bLocationChanged = !bHasCachedLocation || !CurrentLocation.Equals(LastLocation, MinimapLocationTolerance);
	if (bLocationChanged)
	{
		HUDWidget->UpdateMinimapLocation(CurrentLocation);
		LastLocation = CurrentLocation;
		bHasCachedLocation = true;
	}
}

#undef LOCTEXT_NAMESPACE
