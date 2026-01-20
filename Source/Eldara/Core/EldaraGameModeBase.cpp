#include "EldaraGameModeBase.h"
#include "../Characters/EldaraCharacterBase.h"
#include "EldaraPlayerController.h"
#include "EldaraGameInstance.h"
#include "Engine/StaticMeshActor.h"
#include "GameFramework/HUD.h"
#include "UObject/ConstructorHelpers.h"

namespace
{
	const FVector PreviewGroundScale(25.f, 25.f, 1.f);
}

AEldaraGameModeBase::AEldaraGameModeBase()
{
	PlayerControllerClass = AEldaraPlayerController::StaticClass();
	DefaultPawnClass = AEldaraCharacterBase::StaticClass();
	HUDClass = AHUD::StaticClass(); // HUD uses base class; UI is spawned via player controller widgets.

	static ConstructorHelpers::FObjectFinder<UStaticMesh> GroundMeshFinder(TEXT("/Engine/BasicShapes/Plane.Plane"));
	if (GroundMeshFinder.Succeeded())
	{
		PreviewGroundMesh = GroundMeshFinder.Object;
	}

	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase constructed"));
}

void AEldaraGameModeBase::BeginPlay()
{
	Super::BeginPlay();

	if (UEldaraGameInstance* EldaraInstance = Cast<UEldaraGameInstance>(GetGameInstance()))
	{
		EldaraInstance->LoadQuestProgressSnapshot(EldaraInstance->GetDefaultQuestSlotName());
	}
	
	SpawnViewportPreviewGround();

	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase: Game started"));
}

void AEldaraGameModeBase::Logout(AController* Exiting)
{
	if (UEldaraGameInstance* EldaraInstance = Cast<UEldaraGameInstance>(GetGameInstance()))
	{
		EldaraInstance->SaveQuestProgressSnapshot(EldaraInstance->GetDefaultQuestSlotName());
	}

	Super::Logout(Exiting);
}

void AEldaraGameModeBase::SpawnViewportPreviewGround()
{
#if WITH_EDITOR
	if (!PreviewGroundMesh)
	{
		return;
	}

	UWorld* World = GetWorld();
	if (!World)
	{
		return;
	}

	const FString MapName = World->GetMapName();
	if (!MapName.Contains(TEXT("Entry")))
	{
		return;
	}

	FActorSpawnParameters SpawnParams;
	SpawnParams.SpawnCollisionHandlingOverride = ESpawnActorCollisionHandlingMethod::AdjustIfPossibleButAlwaysSpawn;
	AStaticMeshActor* GroundActor = World->SpawnActor<AStaticMeshActor>(FVector::ZeroVector, FRotator::ZeroRotator, SpawnParams);
	if (!GroundActor)
	{
		return;
	}

	UStaticMeshComponent* MeshComponent = GroundActor->GetStaticMeshComponent();
	if (!MeshComponent)
	{
		return;
	}

	MeshComponent->SetStaticMesh(PreviewGroundMesh);
	MeshComponent->SetMobility(EComponentMobility::Static);
	MeshComponent->SetCollisionProfileName(TEXT("BlockAll"));
	GroundActor->SetActorScale3D(PreviewGroundScale);
	GroundActor->SetActorLocation(FVector(0.f, 0.f, 0.f));
#endif
}
