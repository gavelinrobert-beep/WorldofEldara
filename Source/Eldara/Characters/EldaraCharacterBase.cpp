#include "EldaraCharacterBase.h"
#include "EldaraCombatComponent.h"
#include "Data/EldaraRaceData.h"
#include "Data/EldaraClassData.h"
#include "Net/UnrealNetwork.h"

AEldaraCharacterBase::AEldaraCharacterBase()
{
	PrimaryActorTick.bCanEverTick = true;

	// Create combat component
	CombatComponent = CreateDefaultSubobject<UEldaraCombatComponent>(TEXT("CombatComponent"));

	// Initialize stats with default values
	Health = 100.0f;
	MaxHealth = 100.0f;
	Resource = 100.0f;
	MaxResource = 100.0f;

	// Enable replication
	bReplicates = true;
}

void AEldaraCharacterBase::BeginPlay()
{
	Super::BeginPlay();
	
	InitializeStats();
}

void AEldaraCharacterBase::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}

float AEldaraCharacterBase::TakeDamage(float DamageAmount, FDamageEvent const& DamageEvent, 
	AController* EventInstigator, AActor* DamageCauser)
{
	if (IsDead())
	{
		return 0.0f;
	}

	// Apply damage
	Health = FMath::Max(0.0f, Health - DamageAmount);

	UE_LOG(LogTemp, Log, TEXT("%s took %.1f damage, remaining health: %.1f"), 
		*GetName(), DamageAmount, Health);

	// Check for death
	if (IsDead())
	{
		HandleDeath();
	}

	return DamageAmount;
}

void AEldaraCharacterBase::HandleDeath()
{
	UE_LOG(LogTemp, Log, TEXT("%s has died"), *GetName());
	
	// TODO: Implement death logic
	// - Play death animation
	// - Disable input
	// - Trigger respawn timer
	// - Drop loot (for NPCs)
	// - Grant XP to killer (for NPCs)
}

void AEldaraCharacterBase::InitializeStats()
{
	if (ClassData)
	{
		// Initialize stats from class data
		MaxHealth = ClassData->BaseStats.Health;
		Health = MaxHealth;
		MaxResource = ClassData->BaseStats.Resource;
		Resource = MaxResource;

		UE_LOG(LogTemp, Log, TEXT("%s initialized with ClassData: Health=%.1f, Resource=%.1f"), 
			*GetName(), MaxHealth, MaxResource);
	}

	// TODO: Apply race modifiers
	// TODO: Apply equipment bonuses
	// TODO: Apply buffs/debuffs
}

void AEldaraCharacterBase::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(AEldaraCharacterBase, RaceData);
	DOREPLIFETIME(AEldaraCharacterBase, ClassData);
	DOREPLIFETIME(AEldaraCharacterBase, Health);
	DOREPLIFETIME(AEldaraCharacterBase, MaxHealth);
	DOREPLIFETIME(AEldaraCharacterBase, Resource);
	DOREPLIFETIME(AEldaraCharacterBase, MaxResource);
}
