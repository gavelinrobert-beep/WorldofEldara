#include "EldaraGameModeBase.h"

AEldaraGameModeBase::AEldaraGameModeBase()
{
	PlayerControllerClass = nullptr;
	DefaultPawnClass = nullptr;

	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase constructed"));
}

void AEldaraGameModeBase::BeginPlay()
{
	Super::BeginPlay();
	
	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase: Game started"));
}
