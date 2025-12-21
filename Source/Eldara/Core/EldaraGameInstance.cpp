#include "EldaraGameInstance.h"
#include "EldaraSaveGame.h"
#include "Kismet/GameplayStatics.h"
#include "Engine/World.h"
#include "../Characters/EldaraCharacterBase.h"
#include "Eldara/Networking/EldaraNetworkSubsystem.h"
#include "EldaraLocalPersistenceProvider.h"

UEldaraGameInstance::UEldaraGameInstance()
{
	WorldStateVersion = 0;
	DefaultPersistenceProviderClass = UEldaraLocalPersistenceProvider::StaticClass();
}

void UEldaraGameInstance::Init()
{
	Super::Init();

	EnsurePersistenceProvider();
	
	InitializeWorldState();
	
	// Connect to authoritative server using configured host/port (defaults to localhost:7777)
	FString Host = TEXT("127.0.0.1");
	int32 Port = 7777;
	if (GConfig)
	{
		GConfig->GetString(TEXT("/Script/Eldara.EldaraNetworkSubsystem"), TEXT("Host"), Host, GGameIni);
		GConfig->GetInt(TEXT("/Script/Eldara.EldaraNetworkSubsystem"), TEXT("Port"), Port, GGameIni);
	}

	if (UEldaraNetworkSubsystem* Network = GetSubsystem<UEldaraNetworkSubsystem>())
	{
		Network->ConnectToServer(Host, Port);
	}
	
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

	EnsurePersistenceProvider();
	IEldaraPersistenceProvider* Provider = PersistenceProvider ? PersistenceProvider.GetInterface() : nullptr;
	if (!Provider)
	{
		UE_LOG(LogTemp, Warning, TEXT("SaveCurrentState: No persistence provider available"));
		return false;
	}

	FEldaraPlayerPersistenceSnapshot Snapshot;
	Snapshot.CharacterName = PlayerCharacter->GetCharacterName();
	Snapshot.RaceData = PlayerCharacter->GetRaceData();
	Snapshot.ClassData = PlayerCharacter->GetClassData();
	Snapshot.Level = PlayerCharacter->GetCharacterLevel();
	Snapshot.Experience = PlayerCharacter->GetExperience();
	Snapshot.Health = PlayerCharacter->GetHealth();
	Snapshot.Resource = PlayerCharacter->GetResource();
	Snapshot.Stamina = PlayerCharacter->GetStamina();
	Snapshot.Transform = PlayerCharacter->GetActorTransform();
	Snapshot.WorldState.Version = WorldStateVersion;
	// TODO: Populate Inventory and QuestProgress when those gameplay systems expose persistent data.

	const bool bSaved = Provider->SavePlayerSnapshot(SlotName, Snapshot);
	UE_LOG(LogTemp, Log, TEXT("SaveCurrentState: Slot '%s' save %s"), *SlotName, bSaved ? TEXT("succeeded") : TEXT("failed"));
	return bSaved;
}

bool UEldaraGameInstance::LoadState(const FString& SlotName)
{
	AEldaraCharacterBase* PlayerCharacter = GetPrimaryPlayerCharacter();
	if (!PlayerCharacter)
	{
		UE_LOG(LogTemp, Warning, TEXT("LoadState: No player character to apply state"));
		return false;
	}

	EnsurePersistenceProvider();
	IEldaraPersistenceProvider* Provider = PersistenceProvider ? PersistenceProvider.GetInterface() : nullptr;
	if (!Provider)
	{
		UE_LOG(LogTemp, Warning, TEXT("LoadState: No persistence provider available"));
		return false;
	}

	FEldaraPlayerPersistenceSnapshot Snapshot;
	if (!Provider->LoadPlayerSnapshot(SlotName, Snapshot))
	{
		UE_LOG(LogTemp, Warning, TEXT("LoadState: Slot '%s' not found"), *SlotName);
		return false;
	}

	UEldaraRaceData* RaceAsset = Snapshot.RaceData.Get();
	if (!RaceAsset && !Snapshot.RaceData.IsNull())
	{
		RaceAsset = Snapshot.RaceData.LoadSynchronous();
	}

	UEldaraClassData* ClassAsset = Snapshot.ClassData.Get();
	if (!ClassAsset && !Snapshot.ClassData.IsNull())
	{
		ClassAsset = Snapshot.ClassData.LoadSynchronous();
	}

	PlayerCharacter->SetRaceAndClass(RaceAsset, ClassAsset);
	PlayerCharacter->SetVitals(Snapshot.Health, Snapshot.Resource, Snapshot.Stamina);
	PlayerCharacter->SetCharacterName(Snapshot.CharacterName);
	PlayerCharacter->SetLevel(Snapshot.Level);
	PlayerCharacter->SetExperience(Snapshot.Experience);
	PlayerCharacter->SetActorTransform(Snapshot.Transform);

	WorldStateVersion = Snapshot.WorldState.Version;

	UE_LOG(LogTemp, Log, TEXT("LoadState: Slot '%s' applied to %s (WorldState v%d)"), *SlotName, *PlayerCharacter->GetName(), WorldStateVersion);
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

void UEldaraGameInstance::EnsurePersistenceProvider()
{
	if (PersistenceProvider)
	{
		return;
	}

	UClass* ProviderClass = DefaultPersistenceProviderClass.Get();
	if (!ProviderClass)
	{
		ProviderClass = UEldaraLocalPersistenceProvider::StaticClass();
	}

	UObject* ProviderObject = NewObject<UObject>(this, ProviderClass);
	if (ProviderObject && ProviderObject->GetClass()->ImplementsInterface(UEldaraPersistenceProvider::StaticClass()))
	{
		PersistenceProvider = ProviderObject;
		return;
	}

	UEldaraLocalPersistenceProvider* LocalProvider = NewObject<UEldaraLocalPersistenceProvider>(this);
	PersistenceProvider = LocalProvider;
	if (!ProviderObject)
	{
		UE_LOG(LogTemp, Warning, TEXT("EnsurePersistenceProvider: Failed to create provider, defaulting to local SaveGame provider"));
	}
	else
	{
		UE_LOG(LogTemp, Warning, TEXT("EnsurePersistenceProvider: Provider class did not implement persistence interface, defaulting to local SaveGame provider"));
	}
}
