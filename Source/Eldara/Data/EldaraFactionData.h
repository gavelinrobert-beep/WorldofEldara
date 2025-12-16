#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "EldaraFactionData.generated.h"

// Forward declarations
class UEldaraZoneData;

/**
 * Faction Data Asset
 * Defines a faction (The Covenant, The Dominion, etc.)
 */
UCLASS(BlueprintType)
class ELDARA_API UEldaraFactionData : public UDataAsset
{
	GENERATED_BODY()

public:
	/** Display name of the faction */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Faction")
	FText DisplayName;

	/** Faction philosophy and lore description */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Faction", meta = (MultiLine = "true"))
	FText Description;

	/** Faction color for UI theming */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Faction")
	FLinearColor FactionColor;

	/** Zones controlled by this faction */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "World")
	TArray<TObjectPtr<UEldaraZoneData>> ControlledZones;

	/** Starting reputation value with this faction */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Reputation")
	int32 StartingReputation;

	/** Faction icon for UI */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "UI")
	TObjectPtr<UTexture2D> FactionIcon;
};
