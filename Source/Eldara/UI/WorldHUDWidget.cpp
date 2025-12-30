#include "WorldHUDWidget.h"

#include "Components/Border.h"
#include "Components/CanvasPanel.h"
#include "Components/CanvasPanelSlot.h"
#include "Components/HorizontalBox.h"
#include "Components/HorizontalBoxSlot.h"
#include "Components/ProgressBar.h"
#include "Components/SizeBox.h"
#include "Components/TextBlock.h"
#include "Components/Overlay.h"
#include "Components/OverlaySlot.h"
#include "Blueprint/WidgetTree.h"

namespace
{
	constexpr float BarWidth = 250.f;
	constexpr float BarHeight = 18.f;
	constexpr float BlockPadding = 8.f;
	constexpr float VitalsPadding = 4.f;
	constexpr float MinimapLocationPadding = 8.f;
	constexpr float ActionBarButtonWidth = 60.f;
	constexpr float ActionBarButtonHeight = 42.f;
	const FMargin VitalsOverlayPadding(0.f, 0.f, 0.f, VitalsPadding);
	const FMargin ActionButtonPadding(2.f);
}

void UWorldHUDWidget::NativeConstruct()
{
	Super::NativeConstruct();
	EnsureWidgetTree();
}

void UWorldHUDWidget::EnsureWidgetTree()
{
	if (!WidgetTree)
	{
		return;
	}

	WidgetTree->RootWidget = WidgetTree->ConstructWidget<UCanvasPanel>(UCanvasPanel::StaticClass(), TEXT("RootCanvas"));
	UCanvasPanel* RootCanvas = Cast<UCanvasPanel>(WidgetTree->RootWidget);

	if (!RootCanvas)
	{
		return;
	}

	BuildHealthResourceBlock(RootCanvas);
	BuildMinimapBlock(RootCanvas);
	BuildActionBarBlock(RootCanvas);
}

void UWorldHUDWidget::BuildHealthResourceBlock(UCanvasPanel* RootCanvas)
{
	UOverlay* Overlay = WidgetTree->ConstructWidget<UOverlay>(UOverlay::StaticClass(), TEXT("VitalsOverlay"));

	// Health
	{
		UOverlay* HealthOverlay = WidgetTree->ConstructWidget<UOverlay>(UOverlay::StaticClass(), TEXT("HealthOverlay"));
		HealthBar = WidgetTree->ConstructWidget<UProgressBar>(UProgressBar::StaticClass(), TEXT("HealthBar"));
		HealthBar->SetFillColorAndOpacity(FLinearColor(0.9f, 0.1f, 0.1f));
		HealthBar->SetPercent(1.f);

		UOverlaySlot* HealthBarSlot = HealthOverlay->AddChildToOverlay(HealthBar);
		HealthBarSlot->SetHorizontalAlignment(EHorizontalAlignment::HAlign_Fill);
		HealthBarSlot->SetVerticalAlignment(EVerticalAlignment::VAlign_Fill);

		HealthText = WidgetTree->ConstructWidget<UTextBlock>(UTextBlock::StaticClass(), TEXT("HealthText"));
		HealthText->SetText(FText::FromString(TEXT("Health 100/100")));
		HealthText->SetJustification(ETextJustify::Center);
		UOverlaySlot* HealthTextSlot = HealthOverlay->AddChildToOverlay(HealthText);
		HealthTextSlot->SetHorizontalAlignment(EHorizontalAlignment::HAlign_Center);
		HealthTextSlot->SetVerticalAlignment(EVerticalAlignment::VAlign_Center);

		UOverlaySlot* HealthOverlaySlot = Overlay->AddChildToOverlay(HealthOverlay);
		HealthOverlaySlot->SetPadding(VitalsOverlayPadding);
	}

	// Resource
	{
		UOverlay* ResourceOverlay = WidgetTree->ConstructWidget<UOverlay>(UOverlay::StaticClass(), TEXT("ResourceOverlay"));
		ResourceBar = WidgetTree->ConstructWidget<UProgressBar>(UProgressBar::StaticClass(), TEXT("ResourceBar"));
		ResourceBar->SetFillColorAndOpacity(FLinearColor(0.15f, 0.35f, 0.9f));
		ResourceBar->SetPercent(1.f);

		UOverlaySlot* ResourceBarSlot = ResourceOverlay->AddChildToOverlay(ResourceBar);
		ResourceBarSlot->SetHorizontalAlignment(EHorizontalAlignment::HAlign_Fill);
		ResourceBarSlot->SetVerticalAlignment(EVerticalAlignment::VAlign_Fill);

		ResourceText = WidgetTree->ConstructWidget<UTextBlock>(UTextBlock::StaticClass(), TEXT("ResourceText"));
		ResourceText->SetText(FText::FromString(TEXT("Resource 100/100")));
		ResourceText->SetJustification(ETextJustify::Center);
		UOverlaySlot* ResourceTextSlot = ResourceOverlay->AddChildToOverlay(ResourceText);
		ResourceTextSlot->SetHorizontalAlignment(EHorizontalAlignment::HAlign_Center);
		ResourceTextSlot->SetVerticalAlignment(EVerticalAlignment::VAlign_Center);

		UOverlaySlot* ResourceOverlaySlot = Overlay->AddChildToOverlay(ResourceOverlay);
		ResourceOverlaySlot->SetPadding(FMargin(0.f));
	}

	USizeBox* VitalsBox = WidgetTree->ConstructWidget<USizeBox>(USizeBox::StaticClass(), TEXT("VitalsSizeBox"));
	VitalsBox->SetWidthOverride(BarWidth);
	VitalsBox->SetHeightOverride(BarHeight * 2.f + VitalsPadding);
	VitalsBox->AddChild(Overlay);

	UCanvasPanelSlot* VitalsCanvasSlot = RootCanvas->AddChildToCanvas(VitalsBox);
	VitalsCanvasSlot->SetAnchors(FAnchors(0.f, 0.f, 0.f, 0.f));
	VitalsCanvasSlot->SetOffsets(FMargin(BlockPadding, BlockPadding, 0.f, 0.f));
	VitalsCanvasSlot->SetAutoSize(true);
}

void UWorldHUDWidget::BuildMinimapBlock(UCanvasPanel* RootCanvas)
{
	UBorder* MinimapBorder = WidgetTree->ConstructWidget<UBorder>(UBorder::StaticClass(), TEXT("MinimapBorder"));
	MinimapBorder->SetBrushColor(FLinearColor(0.f, 0.f, 0.f, 0.65f));
	MinimapBorder->SetPadding(FMargin(6.f));

	UOverlay* MinimapOverlay = WidgetTree->ConstructWidget<UOverlay>(UOverlay::StaticClass(), TEXT("MinimapOverlay"));
	UTextBlock* Title = WidgetTree->ConstructWidget<UTextBlock>(UTextBlock::StaticClass(), TEXT("MinimapTitle"));
	Title->SetText(FText::FromString(TEXT("Minimap (placeholder)")));
	Title->SetJustification(ETextJustify::Center);
	MinimapOverlay->AddChildToOverlay(Title);

	MinimapText = WidgetTree->ConstructWidget<UTextBlock>(UTextBlock::StaticClass(), TEXT("MinimapText"));
	MinimapText->SetText(FText::FromString(TEXT("X:0 Y:0")));
	MinimapText->SetJustification(ETextJustify::Center);
	UOverlaySlot* LocationSlot = MinimapOverlay->AddChildToOverlay(MinimapText);
	LocationSlot->SetHorizontalAlignment(EHorizontalAlignment::HAlign_Center);
	LocationSlot->SetVerticalAlignment(EVerticalAlignment::VAlign_Bottom);
	LocationSlot->SetPadding(FMargin(0.f, 0.f, 0.f, MinimapLocationPadding));

	MinimapBorder->SetContent(MinimapOverlay);

	USizeBox* MinimapBox = WidgetTree->ConstructWidget<USizeBox>(USizeBox::StaticClass(), TEXT("MinimapSizeBox"));
	MinimapBox->SetWidthOverride(180.f);
	MinimapBox->SetHeightOverride(180.f);
	MinimapBox->AddChild(MinimapBorder);

	UCanvasPanelSlot* MinimapCanvasSlot = RootCanvas->AddChildToCanvas(MinimapBox);
	MinimapCanvasSlot->SetAnchors(FAnchors(1.f, 0.f, 1.f, 0.f));
	MinimapCanvasSlot->SetAlignment(FVector2D(1.f, 0.f));
	MinimapCanvasSlot->SetOffsets(FMargin(-BlockPadding, BlockPadding, 0.f, 0.f));
	MinimapCanvasSlot->SetAutoSize(true);
}

void UWorldHUDWidget::BuildActionBarBlock(UCanvasPanel* RootCanvas)
{
	ActionBar = WidgetTree->ConstructWidget<UHorizontalBox>(UHorizontalBox::StaticClass(), TEXT("ActionBar"));

	for (int32 Index = 0; Index < 10; ++Index)
	{
		const FString SlotName = FString::Printf(TEXT("ActionSlot_%d"), Index);
		USizeBox* ButtonBox = WidgetTree->ConstructWidget<USizeBox>(USizeBox::StaticClass(), *SlotName);
		ButtonBox->SetWidthOverride(ActionBarButtonWidth);
		ButtonBox->SetHeightOverride(ActionBarButtonHeight);

		UBorder* ButtonBorder = WidgetTree->ConstructWidget<UBorder>(UBorder::StaticClass());
		ButtonBorder->SetBrushColor(FLinearColor(0.f, 0.f, 0.f, 0.4f));

		UTextBlock* Label = WidgetTree->ConstructWidget<UTextBlock>(UTextBlock::StaticClass());
		Label->SetText(FText::FromString(FString::Printf(TEXT("%d"), Index + 1)));
		Label->SetJustification(ETextJustify::Center);

		ButtonBorder->SetContent(Label);
		ButtonBox->AddChild(ButtonBorder);

		UHorizontalBoxSlot* ActionBarSlot = ActionBar->AddChildToHorizontalBox(ButtonBox);
		ActionBarSlot->SetVerticalAlignment(EVerticalAlignment::VAlign_Bottom);
		ActionBarSlot->SetPadding(ActionButtonPadding);
	}

	const float ButtonWidthWithPadding = ActionBarButtonWidth + ActionButtonPadding.Left + ActionButtonPadding.Right;
	const float ButtonHeightWithPadding = ActionBarButtonHeight + ActionButtonPadding.Top + ActionButtonPadding.Bottom;

	USizeBox* ActionBarBox = WidgetTree->ConstructWidget<USizeBox>(USizeBox::StaticClass(), TEXT("ActionBarBox"));
	ActionBarBox->SetWidthOverride(ButtonWidthWithPadding * 10.f);
	ActionBarBox->SetHeightOverride(ButtonHeightWithPadding);
	ActionBarBox->AddChild(ActionBar);

	UCanvasPanelSlot* ActionBarCanvasSlot = RootCanvas->AddChildToCanvas(ActionBarBox);
	ActionBarCanvasSlot->SetAnchors(FAnchors(0.5f, 1.f, 0.5f, 1.f));
	ActionBarCanvasSlot->SetAlignment(FVector2D(0.5f, 1.f));
	ActionBarCanvasSlot->SetOffsets(FMargin(0.f, -BlockPadding, 0.f, 0.f));
}

void UWorldHUDWidget::UpdateVitals(float CurrentHealth, float MaxHealth, float CurrentResource, float MaxResource)
{
	ApplyVitalsToWidget(CurrentHealth, MaxHealth, HealthBar, HealthText, FLinearColor(0.9f, 0.1f, 0.1f));
	ApplyVitalsToWidget(CurrentResource, MaxResource, ResourceBar, ResourceText, FLinearColor(0.15f, 0.35f, 0.9f));
}

void UWorldHUDWidget::UpdateMinimapLocation(const FVector& WorldLocation)
{
	if (MinimapText)
	{
		MinimapText->SetText(FText::FromString(FString::Printf(TEXT("X: %.0f  Y: %.0f"), WorldLocation.X, WorldLocation.Y)));
	}
}

void UWorldHUDWidget::SetActionBarLabels(const TArray<FText>& Labels)
{
	if (!ActionBar)
	{
		return;
	}

	const int32 ChildCount = ActionBar->GetChildrenCount();
	for (int32 Index = 0; Index < ChildCount && Index < Labels.Num(); ++Index)
	{
		if (USizeBox* Box = Cast<USizeBox>(ActionBar->GetChildAt(Index)))
		{
			if (UBorder* Border = Cast<UBorder>(Box->GetContent()))
			{
				if (UTextBlock* Label = Cast<UTextBlock>(Border->GetContent()))
				{
					Label->SetText(Labels[Index]);
				}
			}
		}
	}
}

void UWorldHUDWidget::ApplyVitalsToWidget(float Current, float Max, UProgressBar* Bar, UTextBlock* Text, const FLinearColor& Tint)
{
	if (Bar)
	{
		const float Ratio = (Max > 0.f) ? FMath::Clamp(Current / Max, 0.f, 1.f) : 0.f;
		Bar->SetPercent(Ratio);
		Bar->SetFillColorAndOpacity(Tint);
	}

	if (Text)
	{
		Text->SetText(FText::FromString(FString::Printf(TEXT("%.0f / %.0f"), Current, Max)));
	}
}
