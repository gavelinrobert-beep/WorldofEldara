#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "EldaraAbility.h"
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

	/** Assign race/class data (used by save/load or creation flows) */
	UFUNCTION(BlueprintCallable, Category = "Character")
	void SetRaceAndClass(UEldaraRaceData* InRace, UEldaraClassData* InClass);

	/** Bulk set vitals from external systems (clamped to max values) */
	UFUNCTION(BlueprintCallable, Category = "Stats")
	void SetVitals(float NewHealth, float NewResource, float NewStamina);

	/** Accessors for data assets */
	UFUNCTION(BlueprintCallable, Category = "Character")
	UEldaraRaceData* GetRaceData() const { return RaceData; }

	UFUNCTION(BlueprintCallable, Category = "Character")
	UEldaraClassData* GetClassData() const { return ClassData; }

	UFUNCTION(BlueprintCallable, Category = "Character")
	FString GetCharacterName() const { return CharacterName; }

	UFUNCTION(BlueprintCallable, Category = "Character")
	void SetCharacterName(const FString& NewName);

	UFUNCTION(BlueprintCallable, Category = "Character")
	int32 GetCharacterLevel() const { return Level; }

	UFUNCTION(BlueprintCallable, Category = "Character")
	void SetLevel(int32 NewLevel);

	UFUNCTION(BlueprintCallable, Category = "Character")
	int32 GetExperience() const { return Experience; }

	UFUNCTION(BlueprintCallable, Category = "Character")
	void SetExperience(int32 NewExperience);

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

	/** Get current stamina */
	UFUNCTION(BlueprintCallable, Category = "Stats")
	float GetStamina() const { return Stamina; }

	/** Get maximum stamina */
	UFUNCTION(BlueprintCallable, Category = "Stats")
	float GetMaxStamina() const { return MaxStamina; }

	/** Consume resource for abilities */
	bool ConsumeResource(float Amount, EResourceType ResourceType, FString& OutErrorMessage);

	/** Restore resource pool */
	void RestoreResource(float Amount);

	/** Heal the character (clamped) */
	void ApplyHealing(float Amount);

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

	/** Player/NPC display name */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Character", Replicated)
	FString CharacterName;

	/** Current level (persisted) */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Character", Replicated)
	int32 Level = 1;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Character", Replicated)
	int32 Experience = 0;

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

	/** Current stamina (used for dodge/block/sprint) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Stats", Replicated)
	float Stamina;

	/** Maximum stamina */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Stats", Replicated)
	float MaxStamina;

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
