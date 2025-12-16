#include "EldaraGameInstance.h"

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

void UEldaraGameInstance::InitializeWorldState()
{
	// TODO: Load world state from server or saved data
	// For now, initialize to default version
	WorldStateVersion = 1;
}
