#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"
#include "EldaraZoneData.generated.h"

// Forward declarations
class UEldaraFactionData;

/**
 * Zone biome type
 */
UENUM(BlueprintType)
enum class EZoneBiome : uint8
{
	Forest          UMETA(DisplayName = "Forest"),
	Plains          UMETA(DisplayName = "Plains"),
	Mountains       UMETA(DisplayName = "Mountains"),
	Desert          UMETA(DisplayName = "Desert"),
	Swamp           UMETA(DisplayName = "Swamp"),
	Tundra          UMETA(DisplayName = "Tundra"),
	Void            UMETA(DisplayName = "Void"),
	Corrupted       UMETA(DisplayName = "Corrupted")
};

/**
 * Zone Data Asset
 * Defines a zone/area in the game world
 */
UCLASS(BlueprintType)
class ELDARA_API UEldaraZoneData : public UDataAsset
{
	GENERATED_BODY()

public:
	/** Zone name */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Zone")
	FText ZoneName;

	/** Zone description/lore */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Zone", meta = (MultiLine = "true"))
	FText Description;

	/** Minimum recommended level */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Zone")
	int32 MinimumLevel = 1;

	/** Maximum recommended level */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Zone")
	int32 MaximumLevel = 10;

	/** Zone biome type */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Zone")
	EZoneBiome Biome;

	/** Faction that controls this zone (nullptr for neutral zones) */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Zone")
	TObjectPtr<UEldaraFactionData> ControllingFaction;

	/** Is this a PvP-enabled zone? */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Zone")
	bool bIsPvPZone = false;

	/** Is this a sanctuary (safe zone with no combat)? */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Zone")
	bool bIsSanctuary = false;

	/** Map icon/texture */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "UI")
	TObjectPtr<UTexture2D> MapIcon;
};
