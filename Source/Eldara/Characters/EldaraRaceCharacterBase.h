#pragma once

#include "CoreMinimal.h"
#include "EldaraCharacterBase.h"
#include "EldaraRaceCharacterBase.generated.h"

class USkeletalMesh;

UCLASS(Abstract)
class ELDARA_API AEldaraRaceCharacterBase : public AEldaraCharacterBase
{
	GENERATED_BODY()

public:
	AEldaraRaceCharacterBase();

protected:
	virtual void BeginPlay() override;

public:
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Race")
	TObjectPtr<USkeletalMesh> RaceSkeletalMesh;
};
