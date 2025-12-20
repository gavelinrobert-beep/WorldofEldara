#include "EldaraCombatComponent.h"
#include "EldaraAbility.h"
#include "EldaraEffect.h"
#include "EldaraCharacterBase.h"
#include "GameFramework/Actor.h"
#include "Engine/World.h"
#include "GameFramework/Pawn.h"
#include "GameFramework/Controller.h"
#include "GameFramework/DamageType.h"
#include "Engine/EngineTypes.h"

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

	ActiveEffects.AddUnique(Effect);

	if (Effect->Duration <= 0.0f)
	{
		// Instant effects apply immediately
		ApplyEffectMagnitude(Effect, GetOwner(), Instigator);
		Effect->ExecuteEffect(GetOwner(), Instigator);
		return;
	}

	// Find existing stack
	FActiveEffectRuntime* Existing = nullptr;
	if (int32* ExistingIndex = EffectIndexMap.Find(Effect))
	{
		if (ActiveEffectRuntime.IsValidIndex(*ExistingIndex))
		{
			Existing = &ActiveEffectRuntime[*ExistingIndex];
		}
		else
		{
			EffectIndexMap.Remove(Effect);
		}
	}

	if (Existing)
	{
		if (Existing->Stacks < Effect->MaxStacks)
		{
			Existing->Stacks++;
		}
		if (Effect->bRefreshDuration)
		{
			Existing->RemainingTime = Effect->Duration;
			Existing->NextTickTime = Effect->TickInterval;
		}
	}
	else
	{
		FActiveEffectRuntime Runtime;
		Runtime.Effect = Effect;
		Runtime.InstigatorActor = Instigator;
		Runtime.RemainingTime = Effect->Duration;
		Runtime.NextTickTime = Effect->TickInterval;
		Runtime.Stacks = 1;
		const int32 NewIndex = ActiveEffectRuntime.Add(Runtime);
		EffectIndexMap.Add(Effect, NewIndex);

		// Apply initial tick immediately if no interval
		if (Effect->TickInterval <= 0.0f)
		{
			ApplyEffectMagnitude(Effect, GetOwner(), Instigator);
		}
	}

	UE_LOG(LogTemp, Log, TEXT("ApplyEffect: %s applied to %s (duration %.2fs)"), 
		*Effect->GetName(), *GetOwner()->GetName(), Effect->Duration);
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

	AEldaraCharacterBase* OwnerCharacter = GetOwnerCharacter();
	if (OwnerCharacter && Ability->ResourceCost > 0.0f)
	{
		switch (Ability->ResourceType)
		{
		case EResourceType::Health:
			if (OwnerCharacter->GetHealth() < Ability->ResourceCost)
			{
				OutErrorMessage = TEXT("Not enough health to cast");
				return false;
			}
			break;
		case EResourceType::Mana:
		case EResourceType::Rage:
		case EResourceType::Energy:
		case EResourceType::Focus:
		case EResourceType::Corruption:
		default:
			if (OwnerCharacter->GetResource() < Ability->ResourceCost)
			{
				OutErrorMessage = TEXT("Not enough resource");
				return false;
			}
			break;
		}
	}

	// Target validation
	const bool bRequiresTarget = Ability->TargetType == EAbilityTargetType::SingleTarget;
	if (bRequiresTarget && !Target)
	{
		OutErrorMessage = TEXT("Target required");
		return false;
	}

	if (Target && Ability->Range > 0.0f)
	{
		const float Distance = FVector::Dist(Target->GetActorLocation(), GetOwner()->GetActorLocation());
		if (Distance > Ability->Range)
		{
			OutErrorMessage = TEXT("Target out of range");
			return false;
		}
	}

	return true;
}

void UEldaraCombatComponent::ExecuteAbility(UEldaraAbility* Ability, AActor* Target)
{
	if (!Ability)
	{
		return;
	}

	AEldaraCharacterBase* OwnerCharacter = GetOwnerCharacter();
	if (OwnerCharacter && Ability->ResourceCost > 0.0f)
	{
		FString ErrorMessage;
		if (!OwnerCharacter->ConsumeResource(Ability->ResourceCost, Ability->ResourceType, ErrorMessage))
		{
			UE_LOG(LogTemp, Warning, TEXT("ExecuteAbility: Failed to pay cost for %s (%s)"), *Ability->GetName(), *ErrorMessage);
			return;
		}
	}

	AActor* ResolvedTarget = Target;
	if (!ResolvedTarget && Ability->TargetType == EAbilityTargetType::Self)
	{
		ResolvedTarget = GetOwner();
	}
	else if (!ResolvedTarget && Ability->TargetType == EAbilityTargetType::NoTarget)
	{
		ResolvedTarget = GetOwner();
	}

	UE_LOG(LogTemp, Log, TEXT("ExecuteAbility: %s executed on %s"), 
		*Ability->GetName(), ResolvedTarget ? *ResolvedTarget->GetName() : TEXT("No Target"));

	// Apply effects
	for (UEldaraEffect* Effect : Ability->EffectsToApply)
	{
		if (!Effect)
		{
			continue;
		}

		AActor* EffectTarget = ResolvedTarget ? ResolvedTarget : GetOwner();
		if (EffectTarget)
		{
			UEldaraCombatComponent* TargetCombat = EffectTarget->FindComponentByClass<UEldaraCombatComponent>();
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
	bool bNeedsCleanup = false;

	for (FActiveEffectRuntime& Runtime : ActiveEffectRuntime)
	{
		if (!Runtime.Effect)
		{
			bNeedsCleanup = true;
			continue;
		}

		Runtime.RemainingTime -= DeltaTime;

		if (Runtime.Effect->TickInterval > 0.0f)
		{
			Runtime.NextTickTime -= DeltaTime;
			if (Runtime.NextTickTime <= 0.0f)
			{
				AActor* InstigatorActor = Runtime.InstigatorActor.Get();
				ApplyEffectMagnitude(Runtime.Effect, GetOwner(), InstigatorActor);
				Runtime.NextTickTime = Runtime.Effect->TickInterval;
			}
		}

		if (Runtime.RemainingTime <= 0.0f)
		{
			bNeedsCleanup = true;
		}
	}

	if (bNeedsCleanup)
	{
		ActiveEffectRuntime.RemoveAllSwap([](const FActiveEffectRuntime& Runtime)
		{
			return !Runtime.Effect || Runtime.RemainingTime <= 0.0f;
		});
		RebuildEffectIndexMap();
	}
}

void UEldaraCombatComponent::ApplyEffectMagnitude(UEldaraEffect* Effect, AActor* Target, AActor* Instigator)
{
	if (!Effect || !Target)
	{
		return;
	}

	AEldaraCharacterBase* TargetCharacter = Cast<AEldaraCharacterBase>(Target);
	APawn* InstigatorPawn = Instigator ? Cast<APawn>(Instigator) : nullptr;
	AController* InstigatorController = InstigatorPawn ? InstigatorPawn->GetController() : nullptr;

	switch (Effect->EffectType)
	{
	case EEffectType::Damage:
		if (TargetCharacter)
		{
			FDamageEvent DamageEvent(UDamageType::StaticClass());
			TargetCharacter->TakeDamage(Effect->Magnitude, DamageEvent, InstigatorController, Instigator);
		}
		else
		{
			Target->TakeDamage(Effect->Magnitude, FDamageEvent(), InstigatorController, Instigator);
		}
		break;
	case EEffectType::Healing:
		if (TargetCharacter)
		{
			TargetCharacter->ApplyHealing(Effect->Magnitude);
		}
		break;
	case EEffectType::ResourceRestore:
		if (TargetCharacter)
		{
			TargetCharacter->RestoreResource(Effect->Magnitude);
		}
		break;
	default:
		// For buffs/debuffs/CC we allow Blueprint to implement details
		break;
	}

	Effect->ExecuteEffect(Target, Instigator);
}

AEldaraCharacterBase* UEldaraCombatComponent::GetOwnerCharacter() const
{
	return Cast<AEldaraCharacterBase>(GetOwner());
}

void UEldaraCombatComponent::RebuildEffectIndexMap()
{
	EffectIndexMap.Empty();
	for (int32 Index = 0; Index < ActiveEffectRuntime.Num(); ++Index)
	{
		if (ActiveEffectRuntime[Index].Effect)
		{
			EffectIndexMap.Add(ActiveEffectRuntime[Index].Effect.Get(), Index);
		}
	}
}
