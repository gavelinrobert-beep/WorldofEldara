#include "EldaraCombatComponent.h"
#include "EldaraAbility.h"
#include "EldaraEffect.h"
#include "EldaraCharacterBase.h"
#include "GameFramework/Actor.h"
#include "Engine/World.h"

UEldaraCombatComponent::UEldaraCombatComponent()
{
	PrimaryComponentTick.bCanEverTick = true;
}

void UEldaraCombatComponent::BeginPlay()
{
	Super::BeginPlay();
}

void UEldaraCombatComponent::TickComponent(float DeltaTime, ELevelTick TickType, 
	FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	// Update active effects (DoT/HoT ticking)
	UpdateActiveEffects(DeltaTime);
}

void UEldaraCombatComponent::Server_ActivateAbility_Implementation(UEldaraAbility* Ability, AActor* Target)
{
	if (!Ability)
	{
		UE_LOG(LogTemp, Warning, TEXT("Server_ActivateAbility: Ability is null"));
		return;
	}

	FString ErrorMessage;
	if (!ValidateAbilityActivation(Ability, Target, ErrorMessage))
	{
		UE_LOG(LogTemp, Warning, TEXT("Server_ActivateAbility: Validation failed - %s"), *ErrorMessage);
		// TODO: Send error message back to client
		return;
	}

	// Execute ability
	ExecuteAbility(Ability, Target);

	// Trigger cooldown
	TriggerCooldown(Ability);

	UE_LOG(LogTemp, Log, TEXT("Server_ActivateAbility: %s activated successfully"), *Ability->GetName());
}

bool UEldaraCombatComponent::Server_ActivateAbility_Validate(UEldaraAbility* Ability, AActor* Target)
{
	// Basic validation for RPC
	return Ability != nullptr;
}

bool UEldaraCombatComponent::CanActivateAbility(UEldaraAbility* Ability, AActor* Target)
{
	if (!Ability)
	{
		return false;
	}

	FString ErrorMessage;
	return ValidateAbilityActivation(Ability, Target, ErrorMessage);
}

void UEldaraCombatComponent::ApplyEffect(UEldaraEffect* Effect, AActor* Instigator)
{
	if (!Effect)
	{
		UE_LOG(LogTemp, Warning, TEXT("ApplyEffect: Effect is null"));
		return;
	}

	// TODO: Implement effect application logic
	// - Parse effect data
	// - Apply damage/healing/buffs/debuffs
	// - Handle DoT/HoT setup
	// - Track active effects

	ActiveEffects.Add(Effect);

	UE_LOG(LogTemp, Log, TEXT("ApplyEffect: %s applied to %s"), 
		*Effect->GetName(), *GetOwner()->GetName());

	// Execute the effect (Blueprint implementable)
	Effect->ExecuteEffect(GetOwner(), Instigator);
}

bool UEldaraCombatComponent::IsAbilityOnCooldown(UEldaraAbility* Ability) const
{
	if (!Ability)
	{
		return false;
	}

	const float* CooldownEndTime = AbilityCooldowns.Find(Ability->GetFName());
	if (CooldownEndTime)
	{
		return GetWorld()->GetTimeSeconds() < *CooldownEndTime;
	}

	return false;
}

float UEldaraCombatComponent::GetAbilityCooldownRemaining(UEldaraAbility* Ability) const
{
	if (!Ability)
	{
		return 0.0f;
	}

	const float* CooldownEndTime = AbilityCooldowns.Find(Ability->GetFName());
	if (CooldownEndTime)
	{
		float Remaining = *CooldownEndTime - GetWorld()->GetTimeSeconds();
		return FMath::Max(0.0f, Remaining);
	}

	return 0.0f;
}

bool UEldaraCombatComponent::ValidateAbilityActivation(UEldaraAbility* Ability, AActor* Target, FString& OutErrorMessage)
{
	// Check cooldown
	if (IsAbilityOnCooldown(Ability))
	{
		OutErrorMessage = TEXT("Ability is on cooldown");
		return false;
	}

	// TODO: Check resources (mana, rage, energy)
	// TODO: Check range to target
	// TODO: Check line-of-sight
	// TODO: Check crowd control status (stunned, silenced, etc.)
	// TODO: Validate target (alive, correct faction, etc.)

	return true;
}

void UEldaraCombatComponent::ExecuteAbility(UEldaraAbility* Ability, AActor* Target)
{
	if (!Ability)
	{
		return;
	}

	// TODO: Implement ability execution
	// - Deduct resource cost
	// - Play cast animation
	// - Apply effects to target(s)
	// - Emit combat events

	UE_LOG(LogTemp, Log, TEXT("ExecuteAbility: %s executed on %s"), 
		*Ability->GetName(), Target ? *Target->GetName() : TEXT("No Target"));

	// Apply effects to target
	for (UEldaraEffect* Effect : Ability->EffectsToApply)
	{
		if (Effect && Target)
		{
			// Get target's combat component
			UEldaraCombatComponent* TargetCombat = Target->FindComponentByClass<UEldaraCombatComponent>();
			if (TargetCombat)
			{
				TargetCombat->ApplyEffect(Effect, GetOwner());
			}
		}
	}
}

void UEldaraCombatComponent::TriggerCooldown(UEldaraAbility* Ability)
{
	if (!Ability)
	{
		return;
	}

	float CooldownEndTime = GetWorld()->GetTimeSeconds() + Ability->Cooldown;
	AbilityCooldowns.Add(Ability->GetFName(), CooldownEndTime);

	UE_LOG(LogTemp, Log, TEXT("TriggerCooldown: %s cooldown set for %.1f seconds"), 
		*Ability->GetName(), Ability->Cooldown);
}

void UEldaraCombatComponent::UpdateActiveEffects(float DeltaTime)
{
	// TODO: Implement active effect updates
	// - Tick DoT/HoT effects
	// - Remove expired effects
	// - Apply periodic damage/healing
}
