#include "EldaraGameModeBase.h"

AEldaraGameModeBase::AEldaraGameModeBase()
{
	// TODO: Set default classes when they are created
	// DefaultPawnClass = AEldaraCharacterBase::StaticClass();
	// PlayerControllerClass = AEldaraPlayerController::StaticClass();
	// GameStateClass = AEldaraGameState::StaticClass();
	// PlayerStateClass = AEldaraPlayerState::StaticClass();
	
	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase constructed"));
}

void AEldaraGameModeBase::BeginPlay()
{
	Super::BeginPlay();
	
	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase: Game started"));
}
