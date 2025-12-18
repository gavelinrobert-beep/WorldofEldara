#pragma once

#include "CoreMinimal.h"
#include "EldaraCharacterCreatePayload.generated.h"

// Forward declarations
class UEldaraRaceData;
class UEldaraClassData;

/**
 * Appearance choice key-value pair
 * RPC-safe alternative to TMap for appearance selections
 */
USTRUCT(BlueprintType)
struct FAppearanceChoice
{
	GENERATED_BODY()

	/** Appearance slot name (e.g., "Hair", "Face", "EyeColor") */
	UPROPERTY(BlueprintReadWrite, Category = "Character Creation")
	FName SlotName;

	/** Selected option index for this slot */
	UPROPERTY(BlueprintReadWrite, Category = "Character Creation")
	int32 OptionIndex = 0;

	FAppearanceChoice()
		: SlotName(NAME_None)
		, OptionIndex(0)
	{
	}

	FAppearanceChoice(FName InSlotName, int32 InOptionIndex)
		: SlotName(InSlotName)
		, OptionIndex(InOptionIndex)
	{
	}
};

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

	/** 
	 * Appearance choices as array of slot/option pairs (RPC-safe)
	 * Each entry maps a SlotName (e.g., "Hair") to an OptionIndex (e.g., 3)
	 * 
	 * To convert to TMap for lookup:
	 * TMap<FName, int32> ChoicesMap;
	 * for (const FAppearanceChoice& Choice : AppearanceChoices)
	 * {
	 *     ChoicesMap.Add(Choice.SlotName, Choice.OptionIndex);
	 * }
	 */
	UPROPERTY(BlueprintReadWrite, Category = "Character Creation")
	TArray<FAppearanceChoice> AppearanceChoices;

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
