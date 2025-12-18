#pragma once

#include "CoreMinimal.h"
#include "EldaraRaceCharacterBase.h"
#include "EldaraRaceCharacters.generated.h"

UCLASS()
class ELDARA_API ASylvanethCharacter : public AEldaraRaceCharacterBase
{
	GENERATED_BODY()

public:
	ASylvanethCharacter();
};

UCLASS()
class ELDARA_API ATrollkinCharacter : public AEldaraRaceCharacterBase
{
	GENERATED_BODY()

public:
	ATrollkinCharacter();
};

UCLASS()
class ELDARA_API ABeastkinCharacter : public AEldaraRaceCharacterBase
{
	GENERATED_BODY()

public:
	ABeastkinCharacter();
};

UCLASS()
class ELDARA_API AHumanCharacter : public AEldaraRaceCharacterBase
{
	GENERATED_BODY()

public:
	AHumanCharacter();
};

UCLASS()
class ELDARA_API ADwarrowCharacter : public AEldaraRaceCharacterBase
{
	GENERATED_BODY()

public:
	ADwarrowCharacter();
};

UCLASS()
class ELDARA_API AVoidboundCharacter : public AEldaraRaceCharacterBase
{
	GENERATED_BODY()

public:
	AVoidboundCharacter();
};
