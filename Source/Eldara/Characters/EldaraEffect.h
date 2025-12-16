#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "EldaraEffect.generated.h"

// Forward declarations
class UParticleSystem;
class USoundBase;

/**
 * Effect type enumeration
 */
UENUM(BlueprintType)
enum class EEffectType : uint8
{
	Damage              UMETA(DisplayName = "Damage"),
	Healing             UMETA(DisplayName = "Healing"),
	Buff                UMETA(DisplayName = "Buff"),
	Debuff              UMETA(DisplayName = "Debuff"),
	CrowdControl        UMETA(DisplayName = "Crowd Control"),
	ResourceRestore     UMETA(DisplayName = "Resource Restore")
};

/**
 * Damage type enumeration
 */
UENUM(BlueprintType)
enum class EDamageType : uint8
{
	Physical        UMETA(DisplayName = "Physical"),
	Magic           UMETA(DisplayName = "Magic"),
	Fire            UMETA(DisplayName = "Fire"),
	Poison          UMETA(DisplayName = "Poison"),
	Frost           UMETA(DisplayName = "Frost"),
	Shadow          UMETA(DisplayName = "Shadow"),
	Nature          UMETA(DisplayName = "Nature"),
	Void            UMETA(DisplayName = "Void")
};

/**
 * Crowd control type enumeration
 */
UENUM(BlueprintType)
enum class ECrowdControlType : uint8
{
	Stun            UMETA(DisplayName = "Stun"),
	Root            UMETA(DisplayName = "Root"),
	Silence         UMETA(DisplayName = "Silence"),
	Fear            UMETA(DisplayName = "Fear"),
	Sleep           UMETA(DisplayName = "Sleep"),
	Slow            UMETA(DisplayName = "Slow"),
	Disarm          UMETA(DisplayName = "Disarm"),
	Polymorph       UMETA(DisplayName = "Polymorph")
};

/**
 * Effect Data Asset
 * Defines an effect that can be applied to actors (damage, healing, buffs, etc.)
 */
UCLASS(BlueprintType)
class ELDARA_API UEldaraEffect : public UDataAsset
{
	GENERATED_BODY()

public:
	/** Effect name */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effect")
	FText EffectName;

	/** Effect type */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effect")
	EEffectType EffectType;

	/** Damage type (for damage effects only) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effect")
	EDamageType DamageType;

	/** Magnitude of the effect (damage amount, healing amount, stat modifier, etc.) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effect")
	float Magnitude;

	/** Duration of the effect in seconds (0 = instant) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effect")
	float Duration;

	/** Tick interval for DoT/HoT effects (0 = instant, no ticking) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effect")
	float TickInterval;

	/** Maximum number of stacks (1 = non-stacking) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effect")
	int32 MaxStacks;

	/** Does applying this effect again refresh the duration? */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effect")
	bool bRefreshDuration;

	/** Crowd control type (for CC effects only) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Effect")
	ECrowdControlType CCType;

	/** Visual effect on target */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Visual")
	TObjectPtr<UParticleSystem> EffectVisual;

	/** Sound played when effect is applied */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Audio")
	TObjectPtr<USoundBase> ApplySound;

	/**
	 * Execute effect logic (Blueprint implementable)
	 * @param Target The actor this effect is applied to
	 * @param Instigator The actor that caused this effect
	 */
	UFUNCTION(BlueprintImplementableEvent, Category = "Effect")
	void ExecuteEffect(AActor* Target, AActor* Instigator);
};
