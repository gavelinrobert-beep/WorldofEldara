#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "EldaraAbility.generated.h"

// Forward declarations
class UEldaraEffect;
class UAnimMontage;
class USoundBase;
class UParticleSystem;

/**
 * Ability target type enumeration
 */
UENUM(BlueprintType)
enum class EAbilityTargetType : uint8
{
	Self            UMETA(DisplayName = "Self"),
	SingleTarget    UMETA(DisplayName = "Single Target"),
	GroundAoE       UMETA(DisplayName = "Ground AoE"),
	ConeAoE         UMETA(DisplayName = "Cone AoE"),
	CircleAoE       UMETA(DisplayName = "Circle AoE"),
	LineAoE         UMETA(DisplayName = "Line AoE"),
	NoTarget        UMETA(DisplayName = "No Target")
};

/**
 * Resource type enumeration
 */
UENUM(BlueprintType)
enum class EResourceType : uint8
{
	Health          UMETA(DisplayName = "Health"),
	Mana            UMETA(DisplayName = "Mana"),
	Rage            UMETA(DisplayName = "Rage"),
	Energy          UMETA(DisplayName = "Energy"),
	Focus           UMETA(DisplayName = "Focus"),
	Corruption      UMETA(DisplayName = "Corruption")
};

/**
 * Ability Data Asset
 * Defines an ability with targeting, costs, cooldown, and effects
 */
UCLASS(BlueprintType)
class ELDARA_API UEldaraAbility : public UDataAsset
{
	GENERATED_BODY()

public:
	/** Ability name */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Ability")
	FText AbilityName;

	/** Ability description */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Ability", meta = (MultiLine = "true"))
	FText Description;

	/** Ability icon for UI */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "UI")
	TObjectPtr<UTexture2D> Icon;

	/** Target type for this ability */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Targeting")
	EAbilityTargetType TargetType;

	/** Maximum range in centimeters (0 = melee range) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Targeting")
	float Range;

	/** Cooldown duration in seconds */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Ability")
	float Cooldown;

	/** Resource type consumed by this ability */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Resources")
	EResourceType ResourceType;

	/** Resource cost to activate this ability */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Resources")
	float ResourceCost;

	/** Cast time in seconds (0 = instant cast) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Ability")
	float CastTime;

	/** Can this ability be cast while moving? */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Ability")
	bool bCanCastWhileMoving;

	/** Effects applied by this ability */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effects")
	TArray<TObjectPtr<UEldaraEffect>> EffectsToApply;

	/** Cast animation montage */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Visual")
	TObjectPtr<UAnimMontage> CastAnimation;

	/** Cast sound effect */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Audio")
	TObjectPtr<USoundBase> CastSound;

	/** Cast visual effect (particle system) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Visual")
	TObjectPtr<UParticleSystem> CastVisual;
};
