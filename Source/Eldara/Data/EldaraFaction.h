#pragma once

#include "CoreMinimal.h"
#include "EldaraFaction.generated.h"

/**
 * Canonical factions within Eldara
 */
UENUM(BlueprintType)
enum class EEldaraFaction : uint8
{
	Neutral         UMETA(DisplayName = "Neutral"),
	VerdantAccord   UMETA(DisplayName = "Verdant Accord"),
	AshenDominion   UMETA(DisplayName = "Ashen Dominion"),
	PrimalConclave  UMETA(DisplayName = "Primal Conclave"),
	FreeCities      UMETA(DisplayName = "Free Cities Compact")
};
