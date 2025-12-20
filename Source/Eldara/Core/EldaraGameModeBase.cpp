#include "EldaraGameModeBase.h"
#include "../Characters/EldaraCharacterBase.h"
#include "EldaraPlayerController.h"

AEldaraGameModeBase::AEldaraGameModeBase()
{
	PlayerControllerClass = AEldaraPlayerController::StaticClass();
	DefaultPawnClass = AEldaraCharacterBase::StaticClass();

	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase constructed"));
}

void AEldaraGameModeBase::BeginPlay()
{
	Super::BeginPlay();
	
	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase: Game started"));
}
