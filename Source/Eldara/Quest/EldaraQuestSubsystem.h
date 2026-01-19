#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "Eldara/Data/EldaraQuestData.h"
#include "EldaraQuestSubsystem.generated.h"

/**
 * Runtime quest state for tracking progress
 */
USTRUCT(BlueprintType)
struct FEldaraActiveQuest
{
	GENERATED_BODY()

	/** Reference to the quest data asset */
	UPROPERTY(BlueprintReadOnly, Category = "Quest")
	TObjectPtr<UEldaraQuestData> QuestData;

	/** Current progress for each objective (index matches QuestData->Objectives) */
	UPROPERTY(BlueprintReadOnly, Category = "Quest")
	TArray<int32> ObjectiveProgress;

	/** Whether the quest has been completed */
	UPROPERTY(BlueprintReadOnly, Category = "Quest")
	bool bIsCompleted = false;

	/** Whether rewards have been claimed */
	UPROPERTY(BlueprintReadOnly, Category = "Quest")
	bool bRewardsClaimed = false;
};

/**
 * Quest management subsystem
 * Handles active quest tracking and provides Blueprint-accessible quest list
 */
UCLASS()
class ELDARA_API UEldaraQuestSubsystem : public UGameInstanceSubsystem
{
	GENERATED_BODY()

public:
	/** Initialize the subsystem */
	virtual void Initialize(FSubsystemCollectionBase& Collection) override;

	/** Deinitialize the subsystem */
	virtual void Deinitialize() override;

	/** Accept/start a new quest */
	UFUNCTION(BlueprintCallable, Category = "Quest")
	bool AcceptQuest(UEldaraQuestData* QuestData);

	/** Get list of all active quests */
	UFUNCTION(BlueprintCallable, Category = "Quest")
	TArray<FEldaraActiveQuest> GetActiveQuests() const { return ActiveQuests; }

	/** Update progress on a quest objective */
	UFUNCTION(BlueprintCallable, Category = "Quest")
	void UpdateQuestProgress(UEldaraQuestData* QuestData, int32 ObjectiveIndex, int32 Progress);

	/** Complete a quest */
	UFUNCTION(BlueprintCallable, Category = "Quest")
	bool CompleteQuest(UEldaraQuestData* QuestData);

protected:
	/** List of currently active quests */
	UPROPERTY(BlueprintReadOnly, Category = "Quest")
	TArray<FEldaraActiveQuest> ActiveQuests;

	/** Find active quest by quest data */
	FEldaraActiveQuest* FindActiveQuest(UEldaraQuestData* QuestData);
};
