#include "EldaraCharacterBase.h"
#include "EldaraCombatComponent.h"
#include "Eldara/Data/EldaraRaceData.h"
#include "Eldara/Data/EldaraClassData.h"
#include "Net/UnrealNetwork.h"
#include "Internationalization/Text.h"
#include "Misc/ConfigCacheIni.h"
#include "GameFramework/Controller.h"
#include "Math/RotationMatrix.h"

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
	Stamina = 100.0f;
	MaxStamina = 100.0f;
	FString ConfigDefaultName;
	if (GConfig && GConfig->GetString(TEXT("/Script/Eldara.EldaraCharacterBase"), TEXT("DefaultCharacterName"), ConfigDefaultName, GGameIni))
	{
		CharacterName = ConfigDefaultName;
	}
	else
	{
		CharacterName = NSLOCTEXT("Eldara", "DefaultCharacterName", "Wanderer").ToString();
	}
	Level = 1;
	Experience = 0;

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

void AEldaraCharacterBase::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	if (!PlayerInputComponent)
	{
		return;
	}

	PlayerInputComponent->BindAxis(TEXT("MoveForward"), this, &AEldaraCharacterBase::MoveForward);
	PlayerInputComponent->BindAxis(TEXT("MoveRight"), this, &AEldaraCharacterBase::MoveRight);
	PlayerInputComponent->BindAxis(TEXT("Turn"), this, &APawn::AddControllerYawInput);
	PlayerInputComponent->BindAxis(TEXT("LookUp"), this, &APawn::AddControllerPitchInput);
}

void AEldaraCharacterBase::SetRaceAndClass(UEldaraRaceData* InRace, UEldaraClassData* InClass)
{
	RaceData = InRace;
	ClassData = InClass;
	InitializeStats();
}

void AEldaraCharacterBase::SetVitals(float NewHealth, float NewResource, float NewStamina)
{
	Health = FMath::Clamp(NewHealth, 0.0f, MaxHealth);
	Resource = FMath::Clamp(NewResource, 0.0f, MaxResource);
	Stamina = FMath::Clamp(NewStamina, 0.0f, MaxStamina);
}

void AEldaraCharacterBase::SetCharacterName(const FString& NewName)
{
	CharacterName = NewName;
}

void AEldaraCharacterBase::SetLevel(int32 NewLevel)
{
	Level = FMath::Max(1, NewLevel);
}

void AEldaraCharacterBase::SetExperience(int32 NewExperience)
{
	Experience = FMath::Max(0, NewExperience);
}

bool AEldaraCharacterBase::ConsumeResource(float Amount, EResourceType ResourceType, FString& OutErrorMessage)
{
	switch (ResourceType)
	{
	case EResourceType::Health:
		if (Health < Amount)
		{
			OutErrorMessage = TEXT("Not enough health");
			return false;
		}
		Health -= Amount;
		return true;
	case EResourceType::Mana:
	case EResourceType::Rage:
	case EResourceType::Energy:
	case EResourceType::Focus:
	case EResourceType::Corruption:
	default:
		if (Resource < Amount)
		{
			OutErrorMessage = TEXT("Not enough resource");
			return false;
		}
		Resource -= Amount;
		return true;
	}
}

void AEldaraCharacterBase::RestoreResource(float Amount)
{
	Resource = FMath::Clamp(Resource + Amount, 0.0f, MaxResource);
}

void AEldaraCharacterBase::ApplyHealing(float Amount)
{
	Health = FMath::Clamp(Health + Amount, 0.0f, MaxHealth);
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
		MaxStamina = ClassData->BaseStats.Stamina;
		Stamina = MaxStamina;

		UE_LOG(LogTemp, Log, TEXT("%s initialized with ClassData: Health=%.1f, Resource=%.1f, Stamina=%.1f"), 
			*GetName(), Health, Resource, Stamina);
	}

	if (RaceData)
	{
		MaxHealth *= RaceData->HealthModifier;
		Health = MaxHealth;
		MaxStamina *= RaceData->StaminaModifier;
		Stamina = MaxStamina;

		UE_LOG(LogTemp, Log, TEXT("%s applied RaceData modifiers: Health=%.1f, Stamina=%.1f"),
			*GetName(), MaxHealth, MaxStamina);
	}

	// TODO: Apply equipment bonuses
	// TODO: Apply buffs/debuffs
}

void AEldaraCharacterBase::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
	Super::GetLifetimeReplicatedProps(OutLifetimeProps);

	DOREPLIFETIME(AEldaraCharacterBase, RaceData);
	DOREPLIFETIME(AEldaraCharacterBase, ClassData);
	DOREPLIFETIME(AEldaraCharacterBase, CharacterName);
	DOREPLIFETIME(AEldaraCharacterBase, Level);
	DOREPLIFETIME(AEldaraCharacterBase, Experience);
	DOREPLIFETIME(AEldaraCharacterBase, Health);
	DOREPLIFETIME(AEldaraCharacterBase, MaxHealth);
	DOREPLIFETIME(AEldaraCharacterBase, Resource);
	DOREPLIFETIME(AEldaraCharacterBase, MaxResource);
	DOREPLIFETIME(AEldaraCharacterBase, Stamina);
	DOREPLIFETIME(AEldaraCharacterBase, MaxStamina);
}

void AEldaraCharacterBase::MoveForward(float Value)
{
	if (!Controller || FMath::IsNearlyZero(Value))
	{
		return;
	}

	AddMovementInput(GetMovementDirection(EAxis::X), Value);
}

void AEldaraCharacterBase::MoveRight(float Value)
{
	if (!Controller || FMath::IsNearlyZero(Value))
	{
		return;
	}

	AddMovementInput(GetMovementDirection(EAxis::Y), Value);
}

FVector AEldaraCharacterBase::GetMovementDirection(EAxis::Type Axis) const
{
	if (!Controller)
	{
		return FVector::ZeroVector;
	}

	const FRotator ControlRotation = Controller->GetControlRotation();
	const FRotator YawRotation(0.f, ControlRotation.Yaw, 0.f);

	return FRotationMatrix(YawRotation).GetUnitAxis(Axis);
}
