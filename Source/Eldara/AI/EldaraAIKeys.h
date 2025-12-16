#pragma once

#include "CoreMinimal.h"

/**
 * Blackboard Key Constants for AI
 * Defines FName constants for all blackboard keys used by Eldara AI
 */
namespace EldaraAIKeys
{
	/** Key for target actor (player or NPC) */
	const FName TargetActor = TEXT("TargetActor");

	/** Key for target location (patrol point, ground AoE location) */
	const FName TargetLocation = TEXT("TargetLocation");

	/** Key for current aggro/threat value */
	const FName AggroValue = TEXT("AggroValue");

	/** Key for combat state (bool) */
	const FName IsInCombat = TEXT("IsInCombat");

	/** Key for health percentage (0.0 - 1.0) */
	const FName HealthPercent = TEXT("HealthPercent");

	/** Key for line-of-sight to target (bool) */
	const FName HasLineOfSight = TEXT("HasLineOfSight");

	/** Key for corruption state (bool, for corrupted enemies) */
	const FName IsCorrupted = TEXT("IsCorrupted");
}
