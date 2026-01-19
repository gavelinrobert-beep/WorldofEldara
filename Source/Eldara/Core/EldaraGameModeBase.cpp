#include "EldaraGameModeBase.h"
#include "../Characters/EldaraCharacterBase.h"
#include "EldaraPlayerController.h"
#include "Engine/StaticMeshActor.h"
#include "UObject/ConstructorHelpers.h"

AEldaraGameModeBase::AEldaraGameModeBase()
{
	PlayerControllerClass = AEldaraPlayerController::StaticClass();
	DefaultPawnClass = AEldaraCharacterBase::StaticClass();

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
	
	SpawnViewportPreviewGround();

	UE_LOG(LogTemp, Log, TEXT("EldaraGameModeBase: Game started"));
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

	AStaticMeshActor* GroundActor = World->SpawnActor<AStaticMeshActor>();
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
	GroundActor->SetActorScale3D(FVector(25.f, 25.f, 1.f));
	GroundActor->SetActorLocation(FVector(0.f, 0.f, 0.f));
#endif
}
