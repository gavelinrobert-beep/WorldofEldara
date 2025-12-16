#pragma once

#include "CoreMinimal.h"
#include "AIController.h"
#include "Perception/AIPerceptionTypes.h"
#include "EldaraAIController.generated.h"

// Forward declarations
class UBehaviorTree;
class UBlackboardComponent;

/**
 * AI Controller for NPCs and enemies in World of Eldara
 * Manages behavior trees, perception, and threat/aggro
 */
UCLASS()
class ELDARA_API AEldaraAIController : public AAIController
{
	GENERATED_BODY()

public:
	AEldaraAIController();

protected:
	/** Called when this controller possesses a pawn */
	virtual void OnPossess(APawn* InPawn) override;

	virtual void BeginPlay() override;

public:
	/**
	 * Initialize behavior tree and blackboard
	 * @param BehaviorTreeAsset The behavior tree to run
	 */
	UFUNCTION(BlueprintCallable, Category = "AI")
	void InitializeBehaviorTree(UBehaviorTree* BehaviorTreeAsset);

	/**
	 * Add threat/aggro to a target
	 * @param ThreatSource The actor generating threat
	 * @param ThreatAmount Amount of threat to add
	 */
	UFUNCTION(BlueprintCallable, Category = "AI|Combat")
	void AddThreat(AActor* ThreatSource, float ThreatAmount);

	/**
	 * Get the actor with the highest threat
	 * @return The highest threat target, or nullptr if no threats
	 */
	UFUNCTION(BlueprintCallable, Category = "AI|Combat")
	AActor* GetHighestThreatTarget();

	/**
	 * Clear all threat (evade/reset)
	 */
	UFUNCTION(BlueprintCallable, Category = "AI|Combat")
	void ClearThreat();

protected:
	/** Threat table (Actor -> Threat Value) */
	UPROPERTY()
	TMap<TObjectPtr<AActor>, float> ThreatTable;

	/** Current behavior tree */
	UPROPERTY()
	TObjectPtr<UBehaviorTree> CurrentBehaviorTree;

	/** Perception component callback */
	UFUNCTION()
	void OnTargetPerceptionUpdated(AActor* Actor, FAIStimulus Stimulus);

	/** Update blackboard keys based on pawn state */
	void UpdateBlackboardKeys();
};
