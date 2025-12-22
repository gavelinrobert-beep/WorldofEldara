#pragma once

#include "CoreMinimal.h"
#include "EldaraPersistenceTypes.generated.h"

class UEldaraRaceData;
class UEldaraClassData;

/**
 * DTOs used by persistence providers to move data between gameplay systems and storage.
 * These remain storage-agnostic so different backends (SaveGame, PostgreSQL, etc.) can share the same shapes.
 */
USTRUCT(BlueprintType)
struct FEldaraInventoryItem
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	FString ItemId;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	int32 Quantity = 1;
};

USTRUCT(BlueprintType)
struct FEldaraQuestProgress
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	FName QuestId;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	int32 Stage = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	bool bCompleted = false;
};

USTRUCT(BlueprintType)
struct FEldaraWorldStateSnapshot
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	int32 Version = 1;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	TMap<FName, FString> Flags;
};

USTRUCT(BlueprintType)
struct FEldaraPlayerPersistenceSnapshot
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	FString CharacterName;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	TSoftObjectPtr<UEldaraRaceData> RaceData;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	TSoftObjectPtr<UEldaraClassData> ClassData;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	int32 Level = 1;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	int32 Experience = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	float Health = 0.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	float Resource = 0.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	float Stamina = 0.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	FTransform Transform;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	TArray<FEldaraInventoryItem> Inventory;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	TArray<FEldaraQuestProgress> QuestProgress;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Persistence")
	FEldaraWorldStateSnapshot WorldState;
};
