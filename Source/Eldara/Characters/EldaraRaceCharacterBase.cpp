#include "EldaraRaceCharacterBase.h"
#include "Data/EldaraRaceData.h"
#include "Components/SkeletalMeshComponent.h"

AEldaraRaceCharacterBase::AEldaraRaceCharacterBase()
{
}

void AEldaraRaceCharacterBase::BeginPlay()
{
	Super::BeginPlay();

	if (USkeletalMeshComponent* MeshComponent = GetMesh())
	{
		if (RaceSkeletalMesh)
		{
			MeshComponent->SetSkeletalMesh(RaceSkeletalMesh);
		}
		else if (RaceData && RaceData->BaseMesh)
		{
			MeshComponent->SetSkeletalMesh(RaceData->BaseMesh);
		}
	}
}
