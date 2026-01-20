#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "EldaraGameModeBase.generated.h"

class AController;

/**
 * Base Game Mode for World of Eldara
 * Manages game rules, player spawning, and game flow
 */
UCLASS()
class ELDARA_API AEldaraGameModeBase : public AGameModeBase
{
	GENERATED_BODY()

public:
	AEldaraGameModeBase();

protected:
	/** Called when the game mode is initialized */
	virtual void BeginPlay() override;
	virtual void Logout(AController* Exiting) override;

private:
	void SpawnViewportPreviewGround();

	UPROPERTY()
	TObjectPtr<class UStaticMesh> PreviewGroundMesh;
};
