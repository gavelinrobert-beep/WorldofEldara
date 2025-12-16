#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "EldaraCharacterBase.generated.h"

// Forward declarations
class UEldaraRaceData;
class UEldaraClassData;
class UEldaraCombatComponent;

/**
 * Base Character class for all characters in World of Eldara
 * Players, NPCs, and enemies all derive from this
 */
UCLASS(Blueprintable, BlueprintType)
class ELDARA_API AEldaraCharacterBase : public ACharacter
{
	GENERATED_BODY()

public:
	AEldaraCharacterBase();

protected:
	virtual void BeginPlay() override;

public:
	virtual void Tick(float DeltaTime) override;

	/** Get current health */
	UFUNCTION(BlueprintCallable, Category = "Stats")
	float GetHealth() const { return Health; }

	/** Get maximum health */
	UFUNCTION(BlueprintCallable, Category = "Stats")
	float GetMaxHealth() const { return MaxHealth; }

	/** Get current resource (mana, rage, energy, etc.) */
	UFUNCTION(BlueprintCallable, Category = "Stats")
	float GetResource() const { return Resource; }

	/** Get maximum resource */
	UFUNCTION(BlueprintCallable, Category = "Stats")
	float GetMaxResource() const { return MaxResource; }

	/** Take damage */
	virtual float TakeDamage(float DamageAmount, struct FDamageEvent const& DamageEvent, 
		class AController* EventInstigator, AActor* DamageCauser) override;

	/** Is this character dead? */
	UFUNCTION(BlueprintCallable, Category = "Stats")
	bool IsDead() const { return Health <= 0.0f; }

protected:
	/** Race data for this character */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Character", Replicated)
	TObjectPtr<UEldaraRaceData> RaceData;

	/** Class data for this character */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Character", Replicated)
	TObjectPtr<UEldaraClassData> ClassData;

	/** Current health */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Stats", Replicated)
	float Health;

	/** Maximum health */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Stats", Replicated)
	float MaxHealth;

	/** Current resource (mana, rage, energy, etc.) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Stats", Replicated)
	float Resource;

	/** Maximum resource */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Stats", Replicated)
	float MaxResource;

	/** Combat component */
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Combat")
	TObjectPtr<UEldaraCombatComponent> CombatComponent;

	/** Handle death */
	virtual void HandleDeath();

	/** Initialize stats from race and class data */
	void InitializeStats();

	/** Setup replication */
	virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;
};
