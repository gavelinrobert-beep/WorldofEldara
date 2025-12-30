#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "WorldHUDWidget.generated.h"

class UProgressBar;
class UTextBlock;
class UHorizontalBox;

/**
 * Minimal MMO-style HUD (health/resource, minimap placeholder, action bar)
 */
UCLASS()
class ELDARA_API UWorldHUDWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	virtual void NativeConstruct() override;

	/** Update health/resource bars */
	void UpdateVitals(float CurrentHealth, float MaxHealth, float CurrentResource, float MaxResource);

	/** Update coordinate readout in the minimap placeholder */
	void UpdateMinimapLocation(const FVector& WorldLocation);

	/** Set action bar labels with hotkeys */
	void SetActionBarLabels(const TArray<FText>& Labels);

protected:
	/** Lazily build widget tree if not created via designer */
	void EnsureWidgetTree();

private:
	UPROPERTY()
	UProgressBar* HealthBar = nullptr;

	UPROPERTY()
	UTextBlock* HealthText = nullptr;

	UPROPERTY()
	UProgressBar* ResourceBar = nullptr;

	UPROPERTY()
	UTextBlock* ResourceText = nullptr;

	UPROPERTY()
	UTextBlock* MinimapText = nullptr;

	UPROPERTY()
	UHorizontalBox* ActionBar = nullptr;

	void BuildHealthResourceBlock(class UCanvasPanel* RootCanvas);
	void BuildMinimapBlock(class UCanvasPanel* RootCanvas);
	void BuildActionBarBlock(class UCanvasPanel* RootCanvas);

	void ApplyVitalsToWidget(float Current, float Max, UProgressBar* Bar, UTextBlock* Text, const FLinearColor& Tint);
};
