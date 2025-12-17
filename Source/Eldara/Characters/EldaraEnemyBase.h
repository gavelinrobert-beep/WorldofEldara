#pragma once

#include "CoreMinimal.h"
#include "EldaraNPCBase.h"
#include "EldaraEnemyBase.generated.h"

/**
 * Base class for hostile NPCs
 */
UCLASS(Blueprintable)
class ELDARA_API AEldaraEnemyBase : public AEldaraNPCBase
{
	GENERATED_BODY()

public:
	AEldaraEnemyBase();

	/** Aggro detection range for this enemy */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "AI")
	float AggroRange = 1200.0f;
};
