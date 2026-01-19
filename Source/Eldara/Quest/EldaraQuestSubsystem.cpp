#include "EldaraQuestSubsystem.h"

void UEldaraQuestSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);
	UE_LOG(LogTemp, Log, TEXT("EldaraQuestSubsystem initialized"));
}

void UEldaraQuestSubsystem::Deinitialize()
{
	ActiveQuests.Empty();
	Super::Deinitialize();
}

bool UEldaraQuestSubsystem::AcceptQuest(UEldaraQuestData* QuestData)
{
	if (!QuestData)
	{
		return false;
	}

	// Check if already active
	if (FindActiveQuest(QuestData))
	{
		return false;
	}

	// Create new active quest entry
	FEldaraActiveQuest NewQuest;
	NewQuest.QuestData = QuestData;
	NewQuest.bIsCompleted = false;
	NewQuest.bRewardsClaimed = false;

	// Initialize objective progress
	NewQuest.ObjectiveProgress.SetNum(QuestData->Objectives.Num());
	for (int32& Progress : NewQuest.ObjectiveProgress)
	{
		Progress = 0;
	}

	ActiveQuests.Add(NewQuest);
	return true;
}

void UEldaraQuestSubsystem::UpdateQuestProgress(UEldaraQuestData* QuestData, int32 ObjectiveIndex, int32 Progress)
{
	FEldaraActiveQuest* ActiveQuest = FindActiveQuest(QuestData);
	if (!ActiveQuest || ActiveQuest->bIsCompleted)
	{
		return;
	}

	if (ActiveQuest->ObjectiveProgress.IsValidIndex(ObjectiveIndex))
	{
		ActiveQuest->ObjectiveProgress[ObjectiveIndex] = Progress;

		// Auto-complete quest if all objectives met
		bool bAllComplete = true;
		for (int32 i = 0; i < ActiveQuest->ObjectiveProgress.Num(); ++i)
		{
			if (QuestData->Objectives.IsValidIndex(i))
			{
				if (ActiveQuest->ObjectiveProgress[i] < QuestData->Objectives[i].TargetCount)
				{
					bAllComplete = false;
					break;
				}
			}
		}

		if (bAllComplete)
		{
			ActiveQuest->bIsCompleted = true;
		}
	}
}

bool UEldaraQuestSubsystem::CompleteQuest(UEldaraQuestData* QuestData)
{
	FEldaraActiveQuest* ActiveQuest = FindActiveQuest(QuestData);
	if (!ActiveQuest || !ActiveQuest->bIsCompleted || ActiveQuest->bRewardsClaimed)
	{
		return false;
	}

	ActiveQuest->bRewardsClaimed = true;
	// TODO: Grant rewards (experience, gold, reputation, items)
	return true;
}

FEldaraActiveQuest* UEldaraQuestSubsystem::FindActiveQuest(UEldaraQuestData* QuestData)
{
	for (FEldaraActiveQuest& Quest : ActiveQuests)
	{
		if (Quest.QuestData == QuestData)
		{
			return &Quest;
		}
	}
	return nullptr;
}
