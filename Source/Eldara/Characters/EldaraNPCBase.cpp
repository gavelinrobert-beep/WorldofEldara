#include "EldaraNPCBase.h"
#include "Eldara/AI/EldaraAIController.h"

AEldaraNPCBase::AEldaraNPCBase()
{
	PrimaryActorTick.bCanEverTick = false;
	AIControllerClass = AEldaraAIController::StaticClass();
	AutoPossessAI = EAutoPossessAI::PlacedInWorldOrSpawned;
}
