#pragma once

#include "CoreMinimal.h"
#include "EldaraCharacterCreatePayload.generated.h"

// Forward declarations
class UEldaraRaceData;
class UEldaraClassData;

/**
 * Character creation payload structure
 * Sent from client to server when creating a new character
 */
USTRUCT(BlueprintType)
struct FEldaraCharacterCreatePayload
{
	GENERATED_BODY()

	/** Character name */
	UPROPERTY(BlueprintReadWrite, Category = "Character Creation")
	FString CharacterName;

	/** Selected race data */
	UPROPERTY(BlueprintReadWrite, Category = "Character Creation")
	TObjectPtr<UEldaraRaceData> RaceData;

	/** Selected class data */
	UPROPERTY(BlueprintReadWrite, Category = "Character Creation")
	TObjectPtr<UEldaraClassData> ClassData;

	/** Appearance choices (SlotName → OptionIndex) */
	UPROPERTY(BlueprintReadWrite, Category = "Character Creation")
	TMap<FName, int32> AppearanceChoices;

	/** Skin tone color */
	UPROPERTY(BlueprintReadWrite, Category = "Character Creation")
	FLinearColor SkinTone;

	/** Hair color */
	UPROPERTY(BlueprintReadWrite, Category = "Character Creation")
	FLinearColor HairColor;

	FEldaraCharacterCreatePayload()
		: CharacterName(TEXT(""))
		, RaceData(nullptr)
		, ClassData(nullptr)
		, SkinTone(FLinearColor::White)
		, HairColor(FLinearColor::Black)
	{
	}
};
