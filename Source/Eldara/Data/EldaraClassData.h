#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "EldaraClassData.generated.h"

// Forward declarations
class UEldaraFactionData;
class UEldaraAbility;
class UAnimMontage;

/**
 * Class role enumeration
 */
UENUM(BlueprintType)
enum class EClassRole : uint8
{
	Tank        UMETA(DisplayName = "Tank"),
	Healer      UMETA(DisplayName = "Healer"),
	DPS         UMETA(DisplayName = "DPS"),
	Hybrid      UMETA(DisplayName = "Hybrid")
};

/**
 * Character stats structure
 */
USTRUCT(BlueprintType)
struct FCharacterStats
{
	GENERATED_BODY()

	/** Base health points */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
	float Health = 100.0f;

	/** Base resource (mana, rage, energy, etc.) */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
	float Resource = 100.0f;

	/** Base stamina (dodge/block/sprint) */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
	float Stamina = 100.0f;

	/** Base armor value */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
	float Armor = 10.0f;

	/** Base damage */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
	float Damage = 10.0f;
};

/**
 * Starting ability definition
 */
USTRUCT(BlueprintType)
struct FStartingAbility
{
	GENERATED_BODY()

	/** The ability to grant */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Ability")
	TObjectPtr<UEldaraAbility> Ability;

	/** Level at which this ability is granted */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Ability")
	int32 GrantLevel = 1;
};

/**
 * Class Data Asset
 * Defines a playable class (Memory Warden, Temporal Mage, etc.)
 */
UCLASS(BlueprintType)
class ELDARA_API UEldaraClassData : public UDataAsset
{
	GENERATED_BODY()

public:
	/** Display name of the class */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Class")
	FText DisplayName;

	/** Lore description and class fantasy */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Class", meta = (MultiLine = "true"))
	FText Description;

	/** Role of this class (Tank, Healer, DPS, Hybrid) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Class")
	EClassRole Role;

	/** Primary faction for this class */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Faction")
	TObjectPtr<UEldaraFactionData> PrimaryFaction;

	/** Starting abilities granted to this class */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Abilities")
	TArray<FStartingAbility> StartingAbilities;

	/** Base stats for this class */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Stats")
	FCharacterStats BaseStats;

	/** UI icon for class selection */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "UI")
	TObjectPtr<UTexture2D> ClassIcon;
};
