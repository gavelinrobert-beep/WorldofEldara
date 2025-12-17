#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "EldaraRaceData.generated.h"

// Forward declarations
class UEldaraClassData;
class USkeletalMesh;

/**
 * Appearance slot definition for character customization
 */
USTRUCT(BlueprintType)
struct FAppearanceSlot
{
	GENERATED_BODY()

	/** Name of the appearance slot (e.g., "Hair", "Face", "Ears") */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Appearance")
	FName SlotName;

	/** Number of available options for this slot */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Appearance")
	int32 OptionCount;

	/** Display name for the slot */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Appearance")
	FText DisplayName;
};

/**
 * Race Data Asset
 * Defines a playable race (Sylvaen, High Elf, Human, etc.)
 */
UCLASS(BlueprintType)
class ELDARA_API UEldaraRaceData : public UDataAsset
{
	GENERATED_BODY()

public:
	/** Canonical race identifier (data/serialization) vs DisplayName (player-facing localized) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Race")
	FName RaceName;

	/** Lore description of the race (long-form) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Race", meta = (MultiLine = "true"))
	FText RaceDescription;

	/** Display name of the race */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Race")
	FText DisplayName;

	/** Short-form description for UI (legacy/backward-compatible; prefer RaceDescription for new data) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Race", meta = (MultiLine = "true"))
	FText Description;

	/** Classes this race is allowed to play */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Race")
	TArray<TObjectPtr<UEldaraClassData>> AllowedClasses;

	/** Base skeletal mesh for this race */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Race")
	TObjectPtr<USkeletalMesh> BaseMesh;

	/** Appearance customization slots available for this race */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Appearance")
	TArray<FAppearanceSlot> AppearanceSlots;

	/** UI icon for race selection */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "UI")
	TObjectPtr<UTexture2D> RaceIcon;

	/** Starting zone location for this race */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "World")
	FVector StartingZoneLocation;

	/** Health modifier applied to base stats */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Stats")
	float HealthModifier = 1.0f;

	/** Stamina modifier applied to base stats */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Stats")
	float StaminaModifier = 1.0f;
};
