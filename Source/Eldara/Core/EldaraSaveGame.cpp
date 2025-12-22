#include "EldaraSaveGame.h"

void UEldaraSaveGame::ApplySnapshot(const FEldaraPlayerPersistenceSnapshot& Snapshot)
{
	CharacterName = Snapshot.CharacterName;
	RaceData = Snapshot.RaceData;
	ClassData = Snapshot.ClassData;
	Level = Snapshot.Level;
	Experience = Snapshot.Experience;
	Health = Snapshot.Health;
	Resource = Snapshot.Resource;
	Stamina = Snapshot.Stamina;
	SavedTransform = Snapshot.Transform;
	Inventory = Snapshot.Inventory;
	QuestProgress = Snapshot.QuestProgress;
	WorldState = Snapshot.WorldState;
}

FEldaraPlayerPersistenceSnapshot UEldaraSaveGame::ToSnapshot() const
{
	FEldaraPlayerPersistenceSnapshot Snapshot;
	Snapshot.CharacterName = CharacterName;
	Snapshot.RaceData = RaceData;
	Snapshot.ClassData = ClassData;
	Snapshot.Level = Level;
	Snapshot.Experience = Experience;
	Snapshot.Health = Health;
	Snapshot.Resource = Resource;
	Snapshot.Stamina = Stamina;
	Snapshot.Transform = SavedTransform;
	Snapshot.Inventory = Inventory;
	Snapshot.QuestProgress = QuestProgress;
	Snapshot.WorldState = WorldState;
	return Snapshot;
}
