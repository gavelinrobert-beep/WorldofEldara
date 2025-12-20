#include "EldaraGameInstance.h"
#include "EldaraSaveGame.h"
#include "Kismet/GameplayStatics.h"
#include "Engine/World.h"
#include "../Characters/EldaraCharacterBase.h"

UEldaraGameInstance::UEldaraGameInstance()
{
	WorldStateVersion = 0;
}

void UEldaraGameInstance::Init()
{
	Super::Init();
	
	InitializeWorldState();
	
	UE_LOG(LogTemp, Log, TEXT("EldaraGameInstance initialized. WorldStateVersion: %d"), WorldStateVersion);
}

bool UEldaraGameInstance::SaveCurrentState(const FString& SlotName)
{
	AEldaraCharacterBase* PlayerCharacter = GetPrimaryPlayerCharacter();
	if (!PlayerCharacter)
	{
		UE_LOG(LogTemp, Warning, TEXT("SaveCurrentState: No player character to save"));
		return false;
	}

	UEldaraSaveGame* SaveObject = Cast<UEldaraSaveGame>(UGameplayStatics::CreateSaveGameObject(UEldaraSaveGame::StaticClass()));
	if (!SaveObject)
	{
		return false;
	}

	SaveObject->CharacterName = PlayerCharacter->GetCharacterName();
	SaveObject->RaceData = PlayerCharacter->GetRaceData();
	SaveObject->ClassData = PlayerCharacter->GetClassData();
	SaveObject->Level = PlayerCharacter->GetLevel();
	SaveObject->Experience = PlayerCharacter->GetExperience();
	SaveObject->Health = PlayerCharacter->GetHealth();
	SaveObject->Resource = PlayerCharacter->GetResource();
	SaveObject->Stamina = PlayerCharacter->GetStamina();
	SaveObject->SavedTransform = PlayerCharacter->GetActorTransform();

	const bool bSaved = UGameplayStatics::SaveGameToSlot(SaveObject, SlotName, 0);
	UE_LOG(LogTemp, Log, TEXT("SaveCurrentState: Slot '%s' save %s"), *SlotName, bSaved ? TEXT("succeeded") : TEXT("failed"));
	return bSaved;
}

bool UEldaraGameInstance::LoadState(const FString& SlotName)
{
	if (!UGameplayStatics::DoesSaveGameExist(SlotName, 0))
	{
		UE_LOG(LogTemp, Warning, TEXT("LoadState: Slot '%s' not found"), *SlotName);
		return false;
	}

	UEldaraSaveGame* Loaded = Cast<UEldaraSaveGame>(UGameplayStatics::LoadGameFromSlot(SlotName, 0));
	if (!Loaded)
	{
		return false;
	}

	AEldaraCharacterBase* PlayerCharacter = GetPrimaryPlayerCharacter();
	if (!PlayerCharacter)
	{
		UE_LOG(LogTemp, Warning, TEXT("LoadState: No player character to apply state"));
		return false;
	}

	UEldaraRaceData* RaceAsset = Loaded->RaceData.Get();
	if (!RaceAsset && !Loaded->RaceData.IsNull())
	{
		RaceAsset = Loaded->RaceData.LoadSynchronous();
	}

	UEldaraClassData* ClassAsset = Loaded->ClassData.Get();
	if (!ClassAsset && !Loaded->ClassData.IsNull())
	{
		ClassAsset = Loaded->ClassData.LoadSynchronous();
	}

	PlayerCharacter->SetRaceAndClass(RaceAsset, ClassAsset);
	PlayerCharacter->SetVitals(Loaded->Health, Loaded->Resource, Loaded->Stamina);
	PlayerCharacter->SetCharacterName(Loaded->CharacterName);
	PlayerCharacter->SetLevel(Loaded->Level);
	PlayerCharacter->SetExperience(Loaded->Experience);
	PlayerCharacter->SetActorTransform(Loaded->SavedTransform);

	UE_LOG(LogTemp, Log, TEXT("LoadState: Slot '%s' applied to %s"), *SlotName, *PlayerCharacter->GetName());
	return true;
}

void UEldaraGameInstance::InitializeWorldState()
{
	// TODO: Load world state from server or saved data
	// For now, initialize to default version
	WorldStateVersion = 1;
}

AEldaraCharacterBase* UEldaraGameInstance::GetPrimaryPlayerCharacter() const
{
	if (!GetWorld())
	{
		return nullptr;
	}

	APawn* Pawn = UGameplayStatics::GetPlayerPawn(GetWorld(), 0);
	return Pawn ? Cast<AEldaraCharacterBase>(Pawn) : nullptr;
}
