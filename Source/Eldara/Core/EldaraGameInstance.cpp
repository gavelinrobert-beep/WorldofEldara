#include "EldaraGameInstance.h"
#include "EldaraSaveGame.h"
#include "Kismet/GameplayStatics.h"
#include "Engine/World.h"
#include "../Characters/EldaraCharacterBase.h"
#include "Eldara/Networking/EldaraNetworkSubsystem.h"
#include "EldaraLocalPersistenceProvider.h"
#include "Eldara/Quest/EldaraQuestSubsystem.h"

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
		// TODO: Switch to async asset loading to avoid hitching when streaming large data assets.
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

bool UEldaraGameInstance::SaveQuestProgressSnapshot(const FString& SlotName)
{
	EnsurePersistenceProvider();
	IEldaraPersistenceProvider* Provider = PersistenceProvider ? PersistenceProvider.GetInterface() : nullptr;
	if (!Provider)
	{
		UE_LOG(LogTemp, Warning, TEXT("SaveQuestProgressSnapshot: No persistence provider available"));
		return false;
	}

	if (!SlotName.Len())
	{
		UE_LOG(LogTemp, Warning, TEXT("SaveQuestProgressSnapshot: SlotName is empty"));
		return false;
	}

	if (UEldaraQuestSubsystem* QuestSubsystem = GetSubsystem<UEldaraQuestSubsystem>())
	{
		TArray<FEldaraQuestProgress> Progress;
		const TArray<FEldaraActiveQuest>& ActiveQuests = QuestSubsystem->GetActiveQuests();
		Progress.Reserve(ActiveQuests.Num());
		for (const FEldaraActiveQuest& Quest : ActiveQuests)
		{
			if (!Quest.QuestData)
			{
				continue;
			}

			int32 CompletedObjectives = 0;
			for (int32 Index = 0; Index < Quest.ObjectiveProgress.Num(); ++Index)
			{
				if (Quest.QuestData->Objectives.IsValidIndex(Index)
					&& Quest.ObjectiveProgress[Index] >= Quest.QuestData->Objectives[Index].TargetCount)
				{
					++CompletedObjectives;
				}
			}

			FEldaraQuestProgress Entry;
			Entry.QuestId = Quest.QuestData->QuestId;
			Entry.Stage = Quest.bIsCompleted ? Quest.ObjectiveProgress.Num() : CompletedObjectives;
			Entry.bCompleted = Quest.bIsCompleted;
			Progress.Add(Entry);
		}

		const bool bSaved = Provider->SaveQuestProgress(SlotName, Progress);
		UE_LOG(LogTemp, Log, TEXT("SaveQuestProgressSnapshot: Slot '%s' save %s (%d quests)"), *SlotName, bSaved ? TEXT("succeeded") : TEXT("failed"), Progress.Num());
		return bSaved;
	}

	UE_LOG(LogTemp, Warning, TEXT("SaveQuestProgressSnapshot: Quest subsystem unavailable"));
	return false;
}

bool UEldaraGameInstance::LoadQuestProgressSnapshot(const FString& SlotName)
{
	EnsurePersistenceProvider();
	IEldaraPersistenceProvider* Provider = PersistenceProvider ? PersistenceProvider.GetInterface() : nullptr;
	if (!Provider)
	{
		UE_LOG(LogTemp, Warning, TEXT("LoadQuestProgressSnapshot: No persistence provider available"));
		return false;
	}

	if (!SlotName.Len())
	{
		UE_LOG(LogTemp, Warning, TEXT("LoadQuestProgressSnapshot: SlotName is empty"));
		return false;
	}

	TArray<FEldaraQuestProgress> Progress;
	if (!Provider->LoadQuestProgress(SlotName, Progress))
	{
		UE_LOG(LogTemp, Warning, TEXT("LoadQuestProgressSnapshot: Slot '%s' not found"), *SlotName);
		return false;
	}

	UEldaraQuestSubsystem* QuestSubsystem = GetSubsystem<UEldaraQuestSubsystem>();
	if (!QuestSubsystem)
	{
		UE_LOG(LogTemp, Warning, TEXT("LoadQuestProgressSnapshot: Quest subsystem unavailable"));
		return false;
	}

	bool bApplied = false;
	for (const FEldaraQuestProgress& Entry : Progress)
	{
		if (Entry.QuestId.IsNone())
		{
			continue;
		}

		QuestSubsystem->MarkQuestProgressSnapshot(Entry.QuestId, Entry.Stage, Entry.bCompleted);
		bApplied = true;
	}

	UE_LOG(LogTemp, Log, TEXT("LoadQuestProgressSnapshot: Slot '%s' applied (%d entries)"), *SlotName, Progress.Num());
	return bApplied;
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

	UObject* ProviderObject = nullptr;
	if (ProviderClass->ImplementsInterface(UEldaraPersistenceProvider::StaticClass()))
	{
		ProviderObject = NewObject<UObject>(this, ProviderClass);
		if (ProviderObject)
		{
			PersistenceProvider = ProviderObject;
			return;
		}
	}

	UEldaraLocalPersistenceProvider* LocalProvider = NewObject<UEldaraLocalPersistenceProvider>(this);
	PersistenceProvider = LocalProvider;
	UE_LOG(LogTemp, Warning, TEXT("EnsurePersistenceProvider: Provider '%s' missing interface or failed to initialize, defaulting to local SaveGame provider"),
		ProviderClass ? *ProviderClass->GetName() : TEXT("None"));
}
