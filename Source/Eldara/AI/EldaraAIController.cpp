#include "EldaraAIController.h"
#include "EldaraAIKeys.h"
#include "BehaviorTree/BehaviorTree.h"
#include "BehaviorTree/BlackboardComponent.h"
#include "Perception/AIPerceptionComponent.h"
#include "Perception/AISenseConfig_Sight.h"
#include "Perception/AISenseConfig_Hearing.h"
#include "Perception/AISenseConfig_Damage.h"
#include "../Characters/EldaraCharacterBase.h"

AEldaraAIController::AEldaraAIController()
{
	// Create perception component
	SetPerceptionComponent(*CreateDefaultSubobject<UAIPerceptionComponent>(TEXT("PerceptionComponent")));

	// TODO: Configure perception senses (sight, hearing, damage)
	// This would typically be done in a config or per-enemy basis
}

void AEldaraAIController::BeginPlay()
{
	Super::BeginPlay();
}

void AEldaraAIController::OnPossess(APawn* InPawn)
{
	Super::OnPossess(InPawn);

	UE_LOG(LogTemp, Log, TEXT("EldaraAIController: Possessed %s"), *InPawn->GetName());

	// TODO: Get behavior tree from possessed pawn's data
	// For now, this is a placeholder. In production, each enemy type would
	// have a reference to its specific behavior tree asset.
	
	// Example of how this would work:
	// AEldaraCharacterBase* Character = Cast<AEldaraCharacterBase>(InPawn);
	// if (Character && Character->BehaviorTreeAsset)
	// {
	//     InitializeBehaviorTree(Character->BehaviorTreeAsset);
	// }

	// Initialize blackboard keys
	UpdateBlackboardKeys();
}

void AEldaraAIController::InitializeBehaviorTree(UBehaviorTree* BehaviorTreeAsset)
{
	if (!BehaviorTreeAsset)
	{
		UE_LOG(LogTemp, Warning, TEXT("InitializeBehaviorTree: BehaviorTreeAsset is null"));
		return;
	}

	CurrentBehaviorTree = BehaviorTreeAsset;

	// Run the behavior tree
	if (RunBehaviorTree(BehaviorTreeAsset))
	{
		UE_LOG(LogTemp, Log, TEXT("InitializeBehaviorTree: Successfully started BT %s"), 
			*BehaviorTreeAsset->GetName());
	}
	else
	{
		UE_LOG(LogTemp, Error, TEXT("InitializeBehaviorTree: Failed to start BT %s"), 
			*BehaviorTreeAsset->GetName());
	}
}

void AEldaraAIController::AddThreat(AActor* ThreatSource, float ThreatAmount)
{
	if (!ThreatSource)
	{
		return;
	}

	float* ExistingThreat = ThreatTable.Find(ThreatSource);
	if (ExistingThreat)
	{
		*ExistingThreat += ThreatAmount;
	}
	else
	{
		ThreatTable.Add(ThreatSource, ThreatAmount);
	}

	UE_LOG(LogTemp, Log, TEXT("AddThreat: %s now has %.1f threat from %s"), 
		*GetName(), ThreatTable[ThreatSource], *ThreatSource->GetName());

	// Update blackboard with highest threat target
	AActor* HighestThreat = GetHighestThreatTarget();
	if (HighestThreat && GetBlackboardComponent())
	{
		GetBlackboardComponent()->SetValueAsObject(EldaraAIKeys::TargetActor, HighestThreat);
		GetBlackboardComponent()->SetValueAsBool(EldaraAIKeys::IsInCombat, true);
	}
}

AActor* AEldaraAIController::GetHighestThreatTarget()
{
	AActor* HighestThreatActor = nullptr;
	float HighestThreatValue = 0.0f;

	for (const auto& Entry : ThreatTable)
	{
		if (Entry.Value > HighestThreatValue)
		{
			HighestThreatValue = Entry.Value;
			HighestThreatActor = Entry.Key;
		}
	}

	return HighestThreatActor;
}

void AEldaraAIController::ClearThreat()
{
	ThreatTable.Empty();

	if (GetBlackboardComponent())
	{
		GetBlackboardComponent()->SetValueAsObject(EldaraAIKeys::TargetActor, nullptr);
		GetBlackboardComponent()->SetValueAsBool(EldaraAIKeys::IsInCombat, false);
	}

	UE_LOG(LogTemp, Log, TEXT("ClearThreat: %s threat table cleared"), *GetName());
}

void AEldaraAIController::OnTargetPerceptionUpdated(AActor* Actor, FAIStimulus Stimulus)
{
	// TODO: Implement perception response
	// - Add threat when seeing/hearing player
	// - Update blackboard keys (HasLineOfSight, etc.)
	
	if (Stimulus.WasSuccessfullySensed())
	{
		UE_LOG(LogTemp, Log, TEXT("OnTargetPerceptionUpdated: %s sensed %s"), 
			*GetName(), *Actor->GetName());

		// Add initial threat when first sensing target
		AddThreat(Actor, 10.0f);
	}
}

void AEldaraAIController::UpdateBlackboardKeys()
{
	if (!GetBlackboardComponent())
	{
		return;
	}

	// Update health percentage
	AEldaraCharacterBase* ControlledCharacter = Cast<AEldaraCharacterBase>(GetPawn());
	if (ControlledCharacter)
	{
		float HealthPercent = ControlledCharacter->GetHealth() / ControlledCharacter->GetMaxHealth();
		GetBlackboardComponent()->SetValueAsFloat(EldaraAIKeys::HealthPercent, HealthPercent);
	}

	// TODO: Update other keys (AggroValue, HasLineOfSight, IsCorrupted, etc.)
}
