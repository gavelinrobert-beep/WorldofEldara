#include "EldaraQuestSubsystem.h"

DEFINE_LOG_CATEGORY_STATIC(LogEldaraQuest, Log, All);

namespace
{
	int32 ResolveCompletedObjectiveCount(int32 Stage, int32 ObjectiveCount)
	{
		// Stage is stored as the count of objectives already completed in the snapshot.
		return FMath::Clamp(Stage, 0, ObjectiveCount);
	}
}

void UEldaraQuestSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);
	UE_LOG(LogEldaraQuest, Log, TEXT("EldaraQuestSubsystem initialized"));
}

void UEldaraQuestSubsystem::PostInitialize()
{
	Super::PostInitialize();

	for (const TSoftObjectPtr<UEldaraQuestData>& QuestPath : QuestAssetPaths)
	{
		if (QuestPath.IsNull())
		{
			continue;
		}

		UEldaraQuestData* QuestData = QuestPath.Get();
		if (!QuestData)
		{
			QuestData = QuestPath.LoadSynchronous();
		}

		if (QuestData)
		{
			RegisterQuestAsset(QuestData);
		}
		else
		{
			UE_LOG(LogEldaraQuest, Warning, TEXT("PostInitialize: Failed to load quest asset for %s"), *QuestPath.ToString());
		}
	}
}

void UEldaraQuestSubsystem::Deinitialize()
{
	ActiveQuests.Empty();
	QuestAssetLookup.Empty();
	Super::Deinitialize();
}

bool UEldaraQuestSubsystem::AcceptQuest(UEldaraQuestData* QuestData)
{
	if (!QuestData)
	{
		return false;
	}

	RegisterQuestAsset(QuestData);

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
	NewQuest.ObjectiveProgress.Init(0, QuestData->Objectives.Num());

	ActiveQuests.Add(NewQuest);
	UE_LOG(LogEldaraQuest, Log, TEXT("AcceptQuest: %s"), *QuestData->QuestName.ToString());
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

bool UEldaraQuestSubsystem::MarkQuestProgressSnapshot(FName QuestId, int32 Stage, bool bCompleted)
{
	if (QuestId.IsNone())
	{
		return false;
	}

	UEldaraQuestData* QuestData = FindQuestAsset(QuestId);
	if (!QuestData)
	{
		UE_LOG(LogEldaraQuest, Warning, TEXT("MarkQuestProgressSnapshot: Quest asset not registered for '%s'"), *QuestId.ToString());
		return false;
	}

	FEldaraActiveQuest* ActiveQuest = FindActiveQuest(QuestData);
	if (!ActiveQuest)
	{
		if (!AcceptQuest(QuestData))
		{
			return false;
		}
		ActiveQuest = FindActiveQuest(QuestData);
	}

	if (!ActiveQuest)
	{
		return false;
	}

	ActiveQuest->bIsCompleted = bCompleted;
	const int32 CompletedObjectives = bCompleted
		? ActiveQuest->ObjectiveProgress.Num()
		: ResolveCompletedObjectiveCount(Stage, ActiveQuest->ObjectiveProgress.Num());
	const int32 ObjectiveCount = QuestData->Objectives.Num();
	for (int32 Index = 0; Index < ActiveQuest->ObjectiveProgress.Num(); ++Index)
	{
		if (Index < ObjectiveCount)
		{
			if (bCompleted)
			{
				ActiveQuest->ObjectiveProgress[Index] = QuestData->Objectives[Index].TargetCount;
			}
			else
			{
				ActiveQuest->ObjectiveProgress[Index] = Index < CompletedObjectives
					? QuestData->Objectives[Index].TargetCount
					: 0;
			}
		}
	}

	return true;
}

void UEldaraQuestSubsystem::RegisterQuestAsset(UEldaraQuestData* QuestData)
{
	if (!QuestData || QuestData->QuestId.IsNone())
	{
		return;
	}

	QuestAssetLookup.FindOrAdd(QuestData->QuestId) = QuestData;
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

UEldaraQuestData* UEldaraQuestSubsystem::FindQuestAsset(FName QuestId) const
{
	if (const TObjectPtr<UEldaraQuestData>* Found = QuestAssetLookup.Find(QuestId))
	{
		return Found->Get();
	}

	return nullptr;
}
