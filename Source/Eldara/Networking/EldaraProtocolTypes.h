#pragma once

#include "CoreMinimal.h"
#include "EldaraProtocolTypes.generated.h"

UENUM(BlueprintType)
enum class EEldaraMovementState : uint8
{
	Idle,
	Walking,
	Running,
	Jumping,
	Falling,
	Swimming,
	Flying,
	Mounted,
	Stunned,
	Rooted
};

UENUM(BlueprintType)
enum class EEldaraEntityType : uint8
{
	Player,
	NPC,
	Monster,
	Object,
	Vehicle,
	Pet
};

UENUM(BlueprintType)
enum class EEldaraNPCServerState : uint8
{
	Idle = 0,
	Patrol = 1,
	Combat = 2
};
