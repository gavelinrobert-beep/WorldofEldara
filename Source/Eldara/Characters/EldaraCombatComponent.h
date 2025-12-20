#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "EldaraCombatComponent.generated.h"

// Forward declarations
class UEldaraAbility;
class UEldaraEffect;

/** Runtime state for an applied UEldaraEffect (duration, ticking, instigator) */
USTRUCT()
struct FActiveEffectRuntime
{
	GENERATED_BODY()

	UPROPERTY()
	TObjectPtr<UEldaraEffect> Effect;

	UPROPERTY()
	TWeakObjectPtr<AActor> InstigatorActor;

	UPROPERTY()
	float RemainingTime = 0.0f;

	UPROPERTY()
	float NextTickTime = 0.0f;

	UPROPERTY()
	int32 Stacks = 1;
};

/**
 * Combat Component
 * Handles ability activation, cooldowns, and effect application
 * Attached to all combat-capable actors
 */
UCLASS(ClassGroup=(Custom), meta=(BlueprintSpawnableComponent))
class ELDARA_API UEldaraCombatComponent : public UActorComponent
{
	GENERATED_BODY()

public:	
	UEldaraCombatComponent();

protected:
	virtual void BeginPlay() override;

public:	
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, 
		FActorComponentTickFunction* ThisTickFunction) override;

	/**
	 * Server RPC: Activate an ability
	 * @param Ability The ability to activate
	 * @param Target The target actor (can be nullptr for self/ground-targeted abilities)
	 */
	UFUNCTION(Server, Reliable)
	void Server_ActivateAbility(UEldaraAbility* Ability, AActor* Target);

	/**
	 * Check if an ability can be activated
	 * @param Ability The ability to check
	 * @param Target The target actor
	 * @return True if the ability can be activated
	 */
	UFUNCTION(BlueprintCallable, Category = "Combat")
	bool CanActivateAbility(UEldaraAbility* Ability, AActor* Target);

	/**
	 * Apply an effect to the owner of this component
	 * @param Effect The effect to apply
	 * @param Instigator The actor that caused this effect
	 */
	UFUNCTION(BlueprintCallable, Category = "Combat")
	void ApplyEffect(UEldaraEffect* Effect, AActor* Instigator);

	/**
	 * Check if an ability is on cooldown
	 * @param Ability The ability to check
	 * @return True if the ability is on cooldown
	 */
	UFUNCTION(BlueprintCallable, Category = "Combat")
	bool IsAbilityOnCooldown(UEldaraAbility* Ability) const;

	/**
	 * Get remaining cooldown time for an ability
	 * @param Ability The ability to check
	 * @return Remaining cooldown time in seconds (0 if not on cooldown)
	 */
	UFUNCTION(BlueprintCallable, Category = "Combat")
	float GetAbilityCooldownRemaining(UEldaraAbility* Ability) const;

protected:
	/** Map of ability names to their cooldown end times for better performance */
	UPROPERTY()
	TMap<FName, float> AbilityCooldowns;

	/** List of active effects on this actor */
	UPROPERTY()
	TArray<TObjectPtr<UEldaraEffect>> ActiveEffects;

	/** Internal active effect tracking */
	UPROPERTY()
	TArray<struct FActiveEffectRuntime> ActiveEffectRuntime;

	UPROPERTY()
	TMap<TObjectPtr<UEldaraEffect>, int32> EffectIndexMap;

	/** Validate ability activation (cooldown, resources, range, etc.) */
	bool ValidateAbilityActivation(UEldaraAbility* Ability, AActor* Target, FString& OutErrorMessage);

	/** Execute ability logic */
	void ExecuteAbility(UEldaraAbility* Ability, AActor* Target);

	/** Trigger ability cooldown */
	void TriggerCooldown(UEldaraAbility* Ability);

	/** Update active effects (tick DoTs/HoTs) */
	void UpdateActiveEffects(float DeltaTime);

	/** Apply a single effect tick or instant payload */
	void ApplyEffectMagnitude(UEldaraEffect* Effect, AActor* Target, AActor* Instigator);

	/** Helper to get owning character for resource/vitals */
	class AEldaraCharacterBase* GetOwnerCharacter() const;

	/** Rebuild effect lookup for fast stacking checks */
	void RebuildEffectIndexMap();
};
