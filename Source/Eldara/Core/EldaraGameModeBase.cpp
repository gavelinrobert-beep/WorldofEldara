#include "EldaraGameModeBase.h"
#include "../Characters/EldaraCharacterBase.h"

AEldaraGameModeBase::AEldaraGameModeBase()
{
	PlayerControllerClass = nullptr;
	DefaultPawnClass = AEldaraCharacterBase::StaticClass();

	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase constructed"));
}

void AEldaraGameModeBase::BeginPlay()
{
	Super::BeginPlay();
	
	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase: Game started"));
}
