#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "EldaraQuestData.generated.h"

// Forward declarations
class UEldaraFactionData;

/**
 * Quest objective type
 */
UENUM(BlueprintType)
enum class EQuestObjectiveType : uint8
{
	Kill            UMETA(DisplayName = "Kill Enemies"),
	Collect         UMETA(DisplayName = "Collect Items"),
	Interact        UMETA(DisplayName = "Interact with Object"),
	Explore         UMETA(DisplayName = "Discover Location"),
	Escort          UMETA(DisplayName = "Escort NPC"),
	Defend          UMETA(DisplayName = "Defend Location")
};

/**
 * Quest objective definition
 */
USTRUCT(BlueprintType)
struct FQuestObjective
{
	GENERATED_BODY()

	/** Type of objective */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Objective")
	EQuestObjectiveType ObjectiveType;

	/** Description of the objective */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Objective")
	FText Description;

	/** Target count (e.g., kill 10 enemies, collect 5 items) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Objective")
	int32 TargetCount = 1;

	/** Optional target actor class (for kill/interact objectives) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Objective")
	TSubclassOf<AActor> TargetActorClass;
};

/**
 * Quest reward definition
 */
USTRUCT(BlueprintType)
struct FQuestReward
{
	GENERATED_BODY()

	/** Experience points */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Reward")
	int32 ExperiencePoints = 0;

	/** Gold/currency reward */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Reward")
	int32 Gold = 0;

	/** Reputation gain with faction */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Reward")
	TObjectPtr<UEldaraFactionData> ReputationFaction;

	/** Reputation amount */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Reward")
	int32 ReputationAmount = 0;

	// TODO: Add item rewards
};

/**
 * Quest Data Asset
 * Defines a quest with objectives and rewards
 */
UCLASS(BlueprintType)
class ELDARA_API UEldaraQuestData : public UDataAsset
{
	GENERATED_BODY()

public:
	/** Quest name */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Quest")
	FText QuestName;

	/** Quest description/story */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Quest", meta = (MultiLine = "true"))
	FText QuestDescription;

	/** Quest objectives */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Quest")
	TArray<FQuestObjective> Objectives;

	/** Quest rewards */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Quest")
	FQuestReward Rewards;

	/** Minimum level to accept quest */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Quest")
	int32 MinimumLevel = 1;

	/** Recommended level for quest */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Quest")
	int32 RecommendedLevel = 1;

	/** Is this a main story quest? */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Quest")
	bool bIsMainQuest = false;

	/** Prerequisite quests (must be completed before this quest is available) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Quest")
	TArray<TObjectPtr<UEldaraQuestData>> PrerequisiteQuests;
};
