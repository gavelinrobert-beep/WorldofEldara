#include "Eldara/Networking/EldaraNetworkSubsystem.h"

#include "Eldara.h"
#include "Eldara/Characters/EldaraCharacterBase.h"
#include "Eldara/Characters/EldaraNPCBase.h"
#include "Engine/World.h"
#include "GameFramework/Character.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "IPAddress.h"
#include "Containers/StringConv.h"
#include "Sockets.h"
#include "SocketSubsystem.h"
#include "Interfaces/IPv4/IPv4Address.h"
#include "Stats/Stats.h"
#include <limits>

namespace
{
constexpr int32 DefaultRemoteLevel = 1;
constexpr bool bDefaultRemoteHostile = false;
constexpr float LocalEntityMatchTolerance = 50.f;

using FMsgPackReader = UEldaraNetworkSubsystem::FMsgPackReader;

int32 ClampUint64ToInt32(uint64 Value)
{
	const uint64 MaxInt32 = static_cast<uint64>(std::numeric_limits<int32>::max());
	const uint64 Clamped = FMath::Clamp<uint64>(Value, 0, MaxInt32);
	return static_cast<int32>(Clamped);
}

bool ReadClampedUint64(FMsgPackReader& Reader, int32& OutValue)
{
	uint64 Value64 = 0;
	if (!Reader.ReadUInt64(Value64))
	{
		return false;
	}

	OutValue = ClampUint64ToInt32(Value64);
	return true;
}
}

void UEldaraNetworkSubsystem::FSocketDeleter::operator()(FSocket* InSocket) const
{
	if (InSocket)
	{
		ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM)->DestroySocket(InSocket);
	}
}

UEldaraNetworkSubsystem::UEldaraNetworkSubsystem()
	: ExpectedPacketSize(-1)
	, ConnectedPort(0)
	, LocalEntityId(0)
	, LocalCharacterId(0)
	, LocalInputSequence(1)
{
}

void UEldaraNetworkSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);
}

void UEldaraNetworkSubsystem::Deinitialize()
{
	Disconnect();
	Super::Deinitialize();
}

TStatId UEldaraNetworkSubsystem::GetStatId() const
{
	RETURN_QUICK_DECLARE_CYCLE_STAT(UEldaraNetworkSubsystem, STATGROUP_Tickables);
}

bool UEldaraNetworkSubsystem::ConnectToServer(const FString& Host, int32 Port)
{
	Disconnect();

	ISocketSubsystem* SocketSubsystem = ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM);
	if (!SocketSubsystem)
	{
		UE_LOG(LogEldara, Warning, TEXT("Socket subsystem unavailable"));
		return false;
	}

	Socket.Reset(SocketSubsystem->CreateSocket(NAME_Stream, TEXT("EldaraClientSocket"), false));
	if (!Socket)
	{
		UE_LOG(LogEldara, Warning, TEXT("Failed to create socket"));
		return false;
	}

	Socket->SetNonBlocking(true);
	Socket->SetReuseAddr(true);

	TSharedRef<FInternetAddr> Addr = SocketSubsystem->CreateInternetAddr();
	FIPv4Address Ip;
	if (!FIPv4Address::Parse(Host, Ip))
	{
		UE_LOG(LogEldara, Warning, TEXT("Invalid host: %s"), *Host);
		Disconnect();
		return false;
	}

	Addr->SetIp(Ip.Value);
	Addr->SetPort(Port);

	if (!Socket->Connect(*Addr))
	{
		UE_LOG(LogEldara, Warning, TEXT("Failed to connect to %s:%d"), *Host, Port);
		Disconnect();
		return false;
	}

	ConnectedHost = Host;
	ConnectedPort = Port;
	LocalInputSequence = 1;
	ExpectedPacketSize = -1;

	UE_LOG(LogEldara, Log, TEXT("Connected to Eldara server at %s:%d"), *Host, Port);
	OnConnected.Broadcast();
	return true;
}

void UEldaraNetworkSubsystem::Disconnect()
{
	if (Socket)
	{
		Socket->Close();
		Socket.Reset();
	}

	ExpectedPacketSize = -1;
	PartialBuffer.Reset();
	EntityActors.Empty();
	LocalEntityId = 0;
	LocalCharacterId = 0;
	LocalPlayer.Reset();

	if (!ConnectedHost.IsEmpty())
	{
		UE_LOG(LogEldara, Log, TEXT("Disconnected from Eldara server (%s:%d)"), *ConnectedHost, ConnectedPort);
		OnDisconnected.Broadcast(TEXT("Closed"));
	}

	ConnectedHost.Reset();
	ConnectedPort = 0;
}

void UEldaraNetworkSubsystem::Tick(float DeltaTime)
{
	if (!Socket)
	{
		return;
	}

	TArray<uint8> Packet;
	while (ReceivePacket(Packet))
	{
		ProcessPacket(Packet);
		Packet.Reset();
	}
}

bool UEldaraNetworkSubsystem::SendMovementInput(const FVector2D& MoveInput, const FRotator& Look, float DeltaTime,
	const FVector& PredictedPosition)
{
	if (!Socket)
	{
		return false;
	}

	TArray<uint8> Payload;
	FMsgPackWriter Writer(Payload);

	// Body (MovementInputPacket)
	Writer.WriteArrayHeader(5);
	Writer.WriteUInt(LocalInputSequence++);
	Writer.WriteFloat(DeltaTime);

	Writer.WriteArrayHeader(6);
	Writer.WriteFloat(MoveInput.X);
	Writer.WriteFloat(MoveInput.Y);
	Writer.WriteBool(false); // Jump
	Writer.WriteBool(false); // Sprint (hook up to input later)
	Writer.WriteFloat(Look.Yaw);
	Writer.WriteFloat(Look.Pitch);

	Writer.WriteArrayHeader(3);
	Writer.WriteFloat(PredictedPosition.X);
	Writer.WriteFloat(PredictedPosition.Y);
	Writer.WriteFloat(PredictedPosition.Z);

	Writer.WriteFloat(Look.Yaw);

	TArray<uint8> Envelope;
	FMsgPackWriter EnvelopeWriter(Envelope);
	EnvelopeWriter.WriteArrayHeader(2);
	EnvelopeWriter.WriteUInt(static_cast<uint16>(EPacketType::MovementInput));
	Envelope.Append(Payload);

	SendRawPacket(Envelope);
	return true;
}

bool UEldaraNetworkSubsystem::ReceivePacket(TArray<uint8>& OutPacket)
{
	if (!Socket)
	{
		return false;
	}

	uint32 PendingSize = 0;
	if (ExpectedPacketSize < 0)
	{
		if (!Socket->HasPendingData(PendingSize) || PendingSize < 4)
		{
			return false;
		}

		uint8 SizeBytes[4] = {0};
		int32 BytesRead = 0;
		if (!Socket->Recv(SizeBytes, 4, BytesRead) || BytesRead != 4)
		{
			Disconnect();
			return false;
		}

		ExpectedPacketSize = SizeBytes[0] | (SizeBytes[1] << 8) | (SizeBytes[2] << 16) | (SizeBytes[3] << 24);
		if (ExpectedPacketSize <= 0 || ExpectedPacketSize > MaxPacketSize)
		{
			UE_LOG(LogEldara, Warning, TEXT("Invalid packet size %d"), ExpectedPacketSize);
			Disconnect();
			return false;
		}

		PartialBuffer.Reset();
	}

	if (!Socket->HasPendingData(PendingSize))
	{
		return false;
	}

	const int32 Remaining = ExpectedPacketSize - PartialBuffer.Num();
	if (Remaining <= 0)
	{
		return false;
	}

	TArray<uint8> Temp;
	Temp.SetNumUninitialized(Remaining);
	int32 BytesRead = 0;
	Socket->Recv(Temp.GetData(), Remaining, BytesRead);
	if (BytesRead > 0)
	{
		PartialBuffer.Append(Temp.GetData(), BytesRead);
	}

	if (PartialBuffer.Num() == ExpectedPacketSize)
	{
		OutPacket = PartialBuffer;
		PartialBuffer.Reset();
		ExpectedPacketSize = -1;
		return true;
	}

	return false;
}

void UEldaraNetworkSubsystem::ProcessPacket(const TArray<uint8>& PacketData)
{
	FMsgPackReader Reader(PacketData);

	uint32 EnvelopeLength = 0;
	if (!Reader.ReadArrayHeader(EnvelopeLength) || EnvelopeLength < 2)
	{
		return;
	}

	int64 RawType = 0;
	if (!Reader.ReadInt64(RawType))
	{
		return;
	}

	const EPacketType PacketType = static_cast<EPacketType>(RawType);
	switch (PacketType)
	{
	case EPacketType::MovementUpdate:
	case EPacketType::MovementSync:
	case EPacketType::PositionCorrection:
	{
		FEldaraMovementUpdate Update;
		if (ParseMovementUpdate(Reader, Update))
		{
			HandleMovementUpdate(Update);
		}
		break;
	}
	case EPacketType::PlayerSpawn:
	{
		FEldaraPlayerSpawn Spawn;
		if (ParsePlayerSpawn(Reader, Spawn))
		{
			HandlePlayerSpawn(Spawn);
		}
		break;
	}
	case EPacketType::EnterWorld:
	{
		FEldaraPlayerSpawn Spawn;
		if (ParseCharacterSnapshot(Reader, Spawn))
		{
			HandlePlayerSpawn(Spawn);
		}
		break;
	}
	case EPacketType::EntitySpawn:
	{
		FEldaraEntitySpawn Spawn;
		if (ParseEntitySpawn(Reader, Spawn))
		{
			HandleEntitySpawn(Spawn);
		}
		break;
	}
	case EPacketType::EntityDespawn:
	{
		int32 EntityId = 0;
		if (ParseEntityDespawn(Reader, EntityId))
		{
			HandleEntityDespawn(EntityId);
		}
		break;
	}
	case EPacketType::NPCStateUpdate:
	{
		int32 EntityId = 0;
		EEldaraNPCServerState State = EEldaraNPCServerState::Idle;
		if (ParseNPCStateUpdate(Reader, EntityId, State))
		{
			HandleNPCState(EntityId, State);
		}
		break;
	}
	default:
		// Ignored for now
		break;
	}
}

bool UEldaraNetworkSubsystem::ParseMovementUpdate(FMsgPackReader& Reader, FEldaraMovementUpdate& OutUpdate)
{
	uint32 Length = 0;
	if (!Reader.ReadArrayHeader(Length) || Length < 7)
	{
		return false;
	}

	if (!ReadClampedUint64(Reader, OutUpdate.EntityId))
	{
		return false;
	}

	if (!ParseVector3(Reader, OutUpdate.Position))
	{
		return false;
	}

	if (!ParseVector3(Reader, OutUpdate.Velocity))
	{
		return false;
	}

	if (!Reader.ReadFloat(OutUpdate.RotationYaw) || !Reader.ReadFloat(OutUpdate.RotationPitch))
	{
		return false;
	}

	int64 StateInt = 0;
	if (!Reader.ReadInt64(StateInt))
	{
		return false;
	}
	OutUpdate.State = static_cast<EEldaraMovementState>(StateInt);

	int64 Timestamp = 0;
	if (!Reader.ReadInt64(Timestamp))
	{
		return false;
	}
	OutUpdate.ServerTimestamp = Timestamp;

	return true;
}

bool UEldaraNetworkSubsystem::ParseVector3(FMsgPackReader& Reader, FVector& OutVector)
{
	uint32 Len = 0;
	if (!Reader.ReadArrayHeader(Len) || Len < 3)
	{
		return false;
	}

	float X = 0.f, Y = 0.f, Z = 0.f;
	if (!Reader.ReadFloat(X) || !Reader.ReadFloat(Y) || !Reader.ReadFloat(Z))
	{
		return false;
	}

	OutVector = FVector(X, Y, Z);
	return true;
}

bool UEldaraNetworkSubsystem::ParsePlayerSpawn(FMsgPackReader& Reader, FEldaraPlayerSpawn& OutSpawn)
{
	uint32 Len = 0;
	if (!Reader.ReadArrayHeader(Len) || Len < 6)
	{
		return false;
	}

	if (!ParseCharacterSnapshot(Reader, OutSpawn))
	{
		return false;
	}

	if (!Reader.SkipValue()) // Resources
	{
		return false;
	}

	FString Zone;
	if (!Reader.ReadString(Zone))
	{
		return false;
	}
	OutSpawn.ZoneId = Zone;

	int64 ServerTime = 0;
	if (!Reader.ReadInt64(ServerTime))
	{
		return false;
	}
	OutSpawn.ServerTime = ServerTime;

	if (!Reader.SkipValue() || !Reader.SkipValue())
	{
		return false;
	}

	return true;
}

bool UEldaraNetworkSubsystem::ParseCharacterSnapshot(FMsgPackReader& Reader, FEldaraPlayerSpawn& OutSpawn)
{
	uint32 Len = 0;
	if (!Reader.ReadArrayHeader(Len))
	{
		return false;
	}

	if (!ReadClampedUint64(Reader, OutSpawn.CharacterId))
	{
		return false;
	}

	if (!Reader.ReadString(OutSpawn.CharacterName))
	{
		return false;
	}

	if (!Reader.SkipValue() || !Reader.SkipValue() || !Reader.SkipValue() || !Reader.SkipValue() || !Reader.SkipValue()
		|| !Reader.SkipValue())
	{
		return false;
	}

	if (!ParseCharacterPosition(Reader, OutSpawn))
	{
		return false;
	}

	if (!Reader.SkipValue()) // Known abilities
	{
		return false;
	}
	FString ZoneId;
	if (Reader.ReadString(ZoneId))
	{
		if (OutSpawn.ZoneId.IsEmpty())
		{
			OutSpawn.ZoneId = ZoneId;
		}
	}
	else
	{
		return false;
	}

	if (!Reader.SkipValue() || !Reader.SkipValue())
	{
		return false;
	}

	return true;
}

bool UEldaraNetworkSubsystem::ParseCharacterPosition(FMsgPackReader& Reader, FEldaraPlayerSpawn& OutSpawn)
{
	uint32 Len = 0;
	if (!Reader.ReadArrayHeader(Len) || Len < 6)
	{
		return false;
	}

	FString Zone;
	if (!Reader.ReadString(Zone))
	{
		return false;
	}
	if (!Zone.IsEmpty())
	{
		OutSpawn.ZoneId = Zone;
	}

	float X = 0.f, Y = 0.f, Z = 0.f, Yaw = 0.f, Pitch = 0.f;
	if (!Reader.ReadFloat(X) || !Reader.ReadFloat(Y) || !Reader.ReadFloat(Z) || !Reader.ReadFloat(Yaw) || !Reader.ReadFloat(Pitch))
	{
		return false;
	}

	OutSpawn.Position = FVector(X, Y, Z);
	return true;
}

bool UEldaraNetworkSubsystem::ParseEntitySpawn(FMsgPackReader& Reader, FEldaraEntitySpawn& OutSpawn)
{
	uint32 Len = 0;
	if (!Reader.ReadArrayHeader(Len) || Len < 5)
	{
		return false;
	}

	if (!ReadClampedUint64(Reader, OutSpawn.EntityId))
	{
		return false;
	}

	int64 RawType = 0;
	if (!Reader.ReadInt64(RawType))
	{
		return false;
	}
	OutSpawn.Type = static_cast<EEldaraEntityType>(RawType);

	if (!Reader.ReadString(OutSpawn.Name))
	{
		return false;
	}

	if (!ParseVector3(Reader, OutSpawn.Position))
	{
		return false;
	}

	if (!Reader.ReadFloat(OutSpawn.RotationYaw))
	{
		return false;
	}

	// CharacterData (ignored)
	if (Len >= 6)
	{
		if (!Reader.SkipValue())
		{
			return false;
		}
	}

	// NPCData (optional)
	if (Len >= 7)
	{
		FMsgPackReader NpcReader = Reader;
		uint32 NpcLen = 0;
		if (NpcReader.ReadArrayHeader(NpcLen) && NpcLen >= 5)
		{
			if (!NpcReader.SkipValue()) // TemplateId
			{
				return false;
			}
			FString NpcName;
			if (!NpcReader.ReadString(NpcName))
			{
				return false;
			}
			if (!NpcName.IsEmpty())
			{
				OutSpawn.Name = NpcName;
			}

			int64 Level = 0;
			if (NpcReader.ReadInt64(Level))
			{
				const int64 Clamped = FMath::Clamp(Level, static_cast<int64>(std::numeric_limits<int32>::min()),
					static_cast<int64>(std::numeric_limits<int32>::max()));
				OutSpawn.Level = static_cast<int32>(Clamped);
			}

			if (!NpcReader.SkipValue()) // Faction
			{
				return false;
			}

			bool bHostile = false;
			if (NpcReader.ReadBool(bHostile))
			{
				OutSpawn.bIsHostile = bHostile;
			}

			Reader.Offset = NpcReader.Offset;
		}
		else
		{
			if (!Reader.SkipValue())
			{
				return false;
			}
		}
	}

	// Skip remaining payload
	for (uint32 Index = 7; Index < Len; ++Index)
	{
		if (!Reader.SkipValue())
		{
			return false;
		}
	}

	return true;
}

bool UEldaraNetworkSubsystem::ParseEntityDespawn(FMsgPackReader& Reader, int32& OutEntityId)
{
	uint32 Len = 0;
	if (!Reader.ReadArrayHeader(Len) || Len < 1)
	{
		return false;
	}

	return ReadClampedUint64(Reader, OutEntityId);
}

bool UEldaraNetworkSubsystem::ParseNPCStateUpdate(FMsgPackReader& Reader, int32& OutEntityId,
	EEldaraNPCServerState& OutState)
{
	uint32 Len = 0;
	if (!Reader.ReadArrayHeader(Len) || Len < 2)
	{
		return false;
	}

	if (!ReadClampedUint64(Reader, OutEntityId))
	{
		return false;
	}

	int64 RawState = 0;
	if (!Reader.ReadInt64(RawState))
	{
		return false;
	}
	OutState = static_cast<EEldaraNPCServerState>(RawState);
	return true;
}

void UEldaraNetworkSubsystem::HandleMovementUpdate(const FEldaraMovementUpdate& Update)
{
	AActor* TargetActor = nullptr;
	if (const TWeakObjectPtr<AActor>* Found = EntityActors.Find(Update.EntityId))
	{
		TargetActor = Found->Get();
	}
	else if (LocalEntityId == 0 && LocalCharacterId != 0 && LocalPlayer.IsValid())
	{
		if (AActor* LocalActor = LocalPlayer.Get())
		{
			const bool bMatchesLocal = LocalActor->GetActorLocation().Equals(Update.Position, LocalEntityMatchTolerance);
			if (bMatchesLocal)
			{
				TargetActor = LocalActor;
				LocalEntityId = Update.EntityId;
				EntityActors.Add(Update.EntityId, TargetActor);
			}
		}
	}

	if (!TargetActor)
	{
		FEldaraEntitySpawn SpawnData;
		SpawnData.EntityId = Update.EntityId;
		SpawnData.Type = EEldaraEntityType::Player;
		SpawnData.Name = TEXT("Unknown");
		SpawnData.Position = Update.Position;
		SpawnData.RotationYaw = Update.RotationYaw;
		SpawnData.Level = DefaultRemoteLevel;
		SpawnData.bIsHostile = bDefaultRemoteHostile;
		HandleEntitySpawn(SpawnData);
		if (const TWeakObjectPtr<AActor>* Spawned = EntityActors.Find(Update.EntityId))
		{
			TargetActor = Spawned->Get();
		}
	}

	if (!TargetActor)
	{
		return;
	}

	TargetActor->SetActorLocation(Update.Position, false, nullptr, ETeleportType::TeleportPhysics);
	TargetActor->SetActorRotation(FRotator(Update.RotationPitch, Update.RotationYaw, 0.f));

	if (ACharacter* Character = Cast<ACharacter>(TargetActor))
	{
		if (UCharacterMovementComponent* Movement = Character->GetCharacterMovement())
		{
			Movement->Velocity = Update.Velocity;
		}
	}

	OnMovementUpdate.Broadcast(Update);
}

void UEldaraNetworkSubsystem::HandlePlayerSpawn(const FEldaraPlayerSpawn& Spawn)
{
	UWorld* World = GetWorld();
	if (!World)
	{
		return;
	}

	const FActorSpawnParameters Params;
	AEldaraCharacterBase* PlayerActor = World->SpawnActor<AEldaraCharacterBase>(
		RemotePlayerClass ? *RemotePlayerClass : AEldaraCharacterBase::StaticClass(),
		Spawn.Position, FRotator::ZeroRotator, Params);

	if (!PlayerActor)
	{
		return;
	}

	PlayerActor->SetCharacterName(Spawn.CharacterName);

	LocalPlayer = PlayerActor;
	LocalCharacterId = Spawn.CharacterId;

	OnPlayerSpawn.Broadcast(Spawn);
}

void UEldaraNetworkSubsystem::HandleEntitySpawn(const FEldaraEntitySpawn& Spawn)
{
	if (EntityActors.Contains(Spawn.EntityId))
	{
		return;
	}

	UWorld* World = GetWorld();
	if (!World)
	{
		return;
	}

	FActorSpawnParameters Params;
	Params.SpawnCollisionHandlingOverride = ESpawnActorCollisionHandlingMethod::AdjustIfPossibleButAlwaysSpawn;

	AActor* Actor = nullptr;
	if (Spawn.Type == EEldaraEntityType::NPC || Spawn.Type == EEldaraEntityType::Monster)
	{
		Actor = World->SpawnActor<AEldaraNPCBase>(
			RemoteNPCClass ? *RemoteNPCClass : AEldaraNPCBase::StaticClass(),
			Spawn.Position, FRotator(0.f, Spawn.RotationYaw, 0.f), Params);
	}
	else
	{
		Actor = World->SpawnActor<AEldaraCharacterBase>(
			RemotePlayerClass ? *RemotePlayerClass : AEldaraCharacterBase::StaticClass(),
			Spawn.Position, FRotator(0.f, Spawn.RotationYaw, 0.f), Params);
	}

	if (!Actor)
	{
		return;
	}

	if (AEldaraNPCBase* NPC = Cast<AEldaraNPCBase>(Actor))
	{
		NPC->NPCName = FText::FromString(Spawn.Name);
	}
	else if (AEldaraCharacterBase* Character = Cast<AEldaraCharacterBase>(Actor))
	{
		Character->SetCharacterName(Spawn.Name);
	}

	EntityActors.Add(Spawn.EntityId, Actor);
	OnEntitySpawn.Broadcast(Spawn);
}

void UEldaraNetworkSubsystem::HandleEntityDespawn(int32 EntityId)
{
	if (const TWeakObjectPtr<AActor>* Found = EntityActors.Find(EntityId))
	{
		if (AActor* Actor = Found->Get())
		{
			Actor->Destroy();
		}
		EntityActors.Remove(EntityId);
		OnEntityDespawn.Broadcast(EntityId);
	}
}

void UEldaraNetworkSubsystem::HandleNPCState(int32 EntityId, EEldaraNPCServerState State)
{
	const TWeakObjectPtr<AActor>* Found = EntityActors.Find(EntityId);
	if (!Found)
	{
		return;
	}

	if (AEldaraNPCBase* NPC = Cast<AEldaraNPCBase>(Found->Get()))
	{
		NPC->ApplyServerState(State);
	}
}

void UEldaraNetworkSubsystem::SendRawPacket(const TArray<uint8>& Payload)
{
	if (!Socket || Payload.Num() <= 0)
	{
		return;
	}

	const int32 Length = Payload.Num();
	uint8 LengthBytes[4];
	LengthBytes[0] = Length & 0xFF;
	LengthBytes[1] = (Length >> 8) & 0xFF;
	LengthBytes[2] = (Length >> 16) & 0xFF;
	LengthBytes[3] = (Length >> 24) & 0xFF;

	auto SendAll = [this](const uint8* Data, int32 LengthToSend) -> bool
	{
		int32 Remaining = LengthToSend;
		while (Remaining > 0)
		{
			int32 SentBytes = 0;
			if (!Socket->Send(Data, Remaining, SentBytes) || SentBytes <= 0)
			{
				return false;
			}

			Remaining -= SentBytes;
			Data += SentBytes;
		}
		return true;
	};

	if (!SendAll(LengthBytes, 4) || !SendAll(Payload.GetData(), Payload.Num()))
	{
		UE_LOG(LogEldara, Warning, TEXT("Failed to send network packet"));
		Disconnect();
	}
}

bool UEldaraNetworkSubsystem::FMsgPackReader::ReadBytes(int32 Count, const uint8*& OutPtr)
{
	if (Offset + Count > Buffer.Num())
	{
		return false;
	}

	OutPtr = Buffer.GetData() + Offset;
	Offset += Count;
	return true;
}

bool UEldaraNetworkSubsystem::FMsgPackReader::ReadArrayHeader(uint32& OutLength)
{
	if (Offset >= Buffer.Num())
	{
		return false;
	}

	const uint8 Lead = Buffer[Offset++];
	if ((Lead & 0xF0) == 0x90)
	{
		OutLength = Lead & 0x0F;
		return true;
	}

	const uint8* Ptr = nullptr;
	switch (Lead)
	{
	case 0xDC:
		if (!ReadBytes(2, Ptr))
		{
			return false;
		}
		OutLength = (Ptr[0] << 8) | Ptr[1];
		return true;
	case 0xDD:
		if (!ReadBytes(4, Ptr))
		{
			return false;
		}
		OutLength = (Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3];
		return true;
	default:
		return false;
	}
}

bool UEldaraNetworkSubsystem::FMsgPackReader::ReadMapHeader(uint32& OutLength)
{
	if (Offset >= Buffer.Num())
	{
		return false;
	}

	const uint8 Lead = Buffer[Offset++];
	if ((Lead & 0xF0) == 0x80)
	{
		OutLength = Lead & 0x0F;
		return true;
	}

	const uint8* Ptr = nullptr;
	switch (Lead)
	{
	case 0xDE:
		if (!ReadBytes(2, Ptr))
		{
			return false;
		}
		OutLength = (Ptr[0] << 8) | Ptr[1];
		return true;
	case 0xDF:
		if (!ReadBytes(4, Ptr))
		{
			return false;
		}
		OutLength = (Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3];
		return true;
	default:
		return false;
	}
}

bool UEldaraNetworkSubsystem::FMsgPackReader::ReadString(FString& Out)
{
	if (Offset >= Buffer.Num())
	{
		return false;
	}

	const uint8 Lead = Buffer[Offset++];
	uint32 Length = 0;
	const uint8* Ptr = nullptr;

	if ((Lead & 0xE0) == 0xA0)
	{
		Length = Lead & 0x1F;
	}
	else if (Lead == 0xD9)
	{
		if (!ReadBytes(1, Ptr))
		{
			return false;
		}
		Length = Ptr[0];
	}
	else if (Lead == 0xDA)
	{
		if (!ReadBytes(2, Ptr))
		{
			return false;
		}
		Length = (Ptr[0] << 8) | Ptr[1];
	}
	else if (Lead == 0xDB)
	{
		if (!ReadBytes(4, Ptr))
		{
			return false;
		}
		Length = (Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3];
	}
	else
	{
		return false;
	}

	if (!ReadBytes(static_cast<int32>(Length), Ptr))
	{
		return false;
	}

	const auto Converter = StringCast<TCHAR>(reinterpret_cast<const UTF8CHAR*>(Ptr), Length);
	Out = FString(Converter.Length(), Converter.Get());
	return true;
}

bool UEldaraNetworkSubsystem::FMsgPackReader::ReadFloat(float& Out)
{
	if (Offset >= Buffer.Num())
	{
		return false;
	}

	const uint8 Lead = Buffer[Offset++];
	if (Lead == 0xCA)
	{
		const uint8* Ptr = nullptr;
		if (!ReadBytes(4, Ptr))
		{
			return false;
		}

		const uint32 Raw = (Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3];
		float Value;
		FMemory::Memcpy(&Value, &Raw, sizeof(float));
		Out = Value;
		return true;
	}

	if (Lead == 0xCB)
	{
		const uint8* Ptr = nullptr;
		if (!ReadBytes(8, Ptr))
		{
			return false;
		}
		const uint64 Raw = (static_cast<uint64>(Ptr[0]) << 56) | (static_cast<uint64>(Ptr[1]) << 48) |
			(static_cast<uint64>(Ptr[2]) << 40) | (static_cast<uint64>(Ptr[3]) << 32) |
			(Ptr[4] << 24) | (Ptr[5] << 16) | (Ptr[6] << 8) | Ptr[7];
		double Value;
		FMemory::Memcpy(&Value, &Raw, sizeof(double));
		Out = static_cast<float>(Value);
		return true;
	}

	return false;
}

bool UEldaraNetworkSubsystem::FMsgPackReader::ReadBool(bool& Out)
{
	if (Offset >= Buffer.Num())
	{
		return false;
	}

	const uint8 Lead = Buffer[Offset++];
	if (Lead == 0xC2)
	{
		Out = false;
		return true;
	}
	if (Lead == 0xC3)
	{
		Out = true;
		return true;
	}

	return false;
}

bool UEldaraNetworkSubsystem::FMsgPackReader::ReadInt64(int64& Out)
{
	if (Offset >= Buffer.Num())
	{
		return false;
	}

	const uint8 Lead = Buffer[Offset++];
	const uint8* Ptr = nullptr;

	// Positive fixint
	if (Lead <= 0x7F)
	{
		Out = Lead;
		return true;
	}

	// Negative fixint
	if (Lead >= 0xE0)
	{
		Out = static_cast<int8>(Lead);
		return true;
	}

	switch (Lead)
	{
	case 0xCC: // uint8
		if (!ReadBytes(1, Ptr)) return false;
		Out = Ptr[0];
		return true;
	case 0xCD: // uint16
		if (!ReadBytes(2, Ptr)) return false;
		Out = (Ptr[0] << 8) | Ptr[1];
		return true;
	case 0xCE: // uint32
		if (!ReadBytes(4, Ptr)) return false;
		Out = (Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3];
		return true;
	case 0xCF: // uint64
		if (!ReadBytes(8, Ptr)) return false;
		Out = (static_cast<uint64>(Ptr[0]) << 56) | (static_cast<uint64>(Ptr[1]) << 48) |
			(static_cast<uint64>(Ptr[2]) << 40) | (static_cast<uint64>(Ptr[3]) << 32) |
			(Ptr[4] << 24) | (Ptr[5] << 16) | (Ptr[6] << 8) | Ptr[7];
		return true;
	case 0xD0: // int8
		if (!ReadBytes(1, Ptr)) return false;
		Out = static_cast<int8>(Ptr[0]);
		return true;
	case 0xD1: // int16
		if (!ReadBytes(2, Ptr)) return false;
		Out = static_cast<int16>((Ptr[0] << 8) | Ptr[1]);
		return true;
	case 0xD2: // int32
		if (!ReadBytes(4, Ptr)) return false;
		Out = static_cast<int32>((Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3]);
		return true;
	case 0xD3: // int64
		if (!ReadBytes(8, Ptr)) return false;
		Out = (static_cast<int64>(Ptr[0]) << 56) | (static_cast<int64>(Ptr[1]) << 48) |
			(static_cast<int64>(Ptr[2]) << 40) | (static_cast<int64>(Ptr[3]) << 32) |
			(Ptr[4] << 24) | (Ptr[5] << 16) | (Ptr[6] << 8) | Ptr[7];
		return true;
	default:
		return false;
	}
}

bool UEldaraNetworkSubsystem::FMsgPackReader::ReadUInt64(uint64& Out)
{
	if (Offset >= Buffer.Num())
	{
		return false;
	}

	const uint8 Lead = Buffer[Offset];
	if (Lead == 0xCF)
	{
		++Offset; // consume lead byte
		const uint8* Ptr = nullptr;
		if (!ReadBytes(8, Ptr))
		{
			return false;
		}

		Out = (static_cast<uint64>(Ptr[0]) << 56) | (static_cast<uint64>(Ptr[1]) << 48) |
			(static_cast<uint64>(Ptr[2]) << 40) | (static_cast<uint64>(Ptr[3]) << 32) |
			(static_cast<uint64>(Ptr[4]) << 24) | (static_cast<uint64>(Ptr[5]) << 16) |
			(static_cast<uint64>(Ptr[6]) << 8) | Ptr[7];
		return true;
	}

	int64 Signed = 0;
	if (!ReadInt64(Signed))
	{
		return false;
	}
	if (Signed < 0)
	{
		return false;
	}
	Out = static_cast<uint64>(Signed);
	return true;
}

bool UEldaraNetworkSubsystem::FMsgPackReader::SkipValue()
{
	if (Offset >= Buffer.Num())
	{
		return false;
	}

	const uint8 Lead = Buffer[Offset++];

	// Nil, bool, fixint
	if (Lead == 0xC0 || Lead == 0xC2 || Lead == 0xC3 || Lead <= 0x7F || Lead >= 0xE0)
	{
		return true;
	}

	const uint8* Ptr = nullptr;
	switch (Lead)
	{
	case 0xCC: Offset += 1; return Offset <= Buffer.Num();
	case 0xCD: Offset += 2; return Offset <= Buffer.Num();
	case 0xCE: Offset += 4; return Offset <= Buffer.Num();
	case 0xCF: Offset += 8; return Offset <= Buffer.Num();
	case 0xD0: Offset += 1; return Offset <= Buffer.Num();
	case 0xD1: Offset += 2; return Offset <= Buffer.Num();
	case 0xD2: Offset += 4; return Offset <= Buffer.Num();
	case 0xD3: Offset += 8; return Offset <= Buffer.Num();
	case 0xCA: Offset += 4; return Offset <= Buffer.Num();
	case 0xCB: Offset += 8; return Offset <= Buffer.Num();
	case 0xC4: if (!ReadBytes(1, Ptr)) return false; Offset += Ptr[0]; return Offset <= Buffer.Num();
	case 0xC5: if (!ReadBytes(2, Ptr)) return false; Offset += (Ptr[0] << 8) | Ptr[1]; return Offset <= Buffer.Num();
	case 0xC6: if (!ReadBytes(4, Ptr)) return false; Offset += (Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3]; return Offset <= Buffer.Num();
	default:
		break;
	}

	// Strings
	if ((Lead & 0xE0) == 0xA0)
	{
		Offset += Lead & 0x1F;
		return Offset <= Buffer.Num();
	}
	if (Lead == 0xD9)
	{
		if (!ReadBytes(1, Ptr)) return false;
		Offset += Ptr[0];
		return Offset <= Buffer.Num();
	}
	if (Lead == 0xDA)
	{
		if (!ReadBytes(2, Ptr)) return false;
		Offset += (Ptr[0] << 8) | Ptr[1];
		return Offset <= Buffer.Num();
	}
	if (Lead == 0xDB)
	{
		if (!ReadBytes(4, Ptr)) return false;
		Offset += (Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3];
		return Offset <= Buffer.Num();
	}

	// Arrays
	if ((Lead & 0xF0) == 0x90 || Lead == 0xDC || Lead == 0xDD)
	{
		uint32 Elements = 0;
		if ((Lead & 0xF0) == 0x90)
		{
			Elements = Lead & 0x0F;
		}
		else if (Lead == 0xDC)
		{
			if (!ReadBytes(2, Ptr)) return false;
			Elements = (Ptr[0] << 8) | Ptr[1];
		}
		else if (Lead == 0xDD)
		{
			if (!ReadBytes(4, Ptr)) return false;
			Elements = (Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3];
		}

		for (uint32 i = 0; i < Elements; ++i)
		{
			if (!SkipValue())
			{
				return false;
			}
		}
		return true;
	}

	// Maps
	if ((Lead & 0xF0) == 0x80 || Lead == 0xDE || Lead == 0xDF)
	{
		uint32 Pairs = 0;
		if ((Lead & 0xF0) == 0x80)
		{
			Pairs = Lead & 0x0F;
		}
		else if (Lead == 0xDE)
		{
			if (!ReadBytes(2, Ptr)) return false;
			Pairs = (Ptr[0] << 8) | Ptr[1];
		}
		else if (Lead == 0xDF)
		{
			if (!ReadBytes(4, Ptr)) return false;
			Pairs = (Ptr[0] << 24) | (Ptr[1] << 16) | (Ptr[2] << 8) | Ptr[3];
		}

		for (uint32 i = 0; i < Pairs; ++i)
		{
			if (!SkipValue() || !SkipValue())
			{
				return false;
			}
		}
		return true;
	}

	return false;
}

void UEldaraNetworkSubsystem::FMsgPackWriter::WriteArrayHeader(uint32 Length)
{
	if (Length <= 15)
	{
		OutBuffer.Add(static_cast<uint8>(0x90 | Length));
	}
	else if (Length <= 0xFFFF)
	{
		OutBuffer.Add(0xDC);
		OutBuffer.Add(static_cast<uint8>((Length >> 8) & 0xFF));
		OutBuffer.Add(static_cast<uint8>(Length & 0xFF));
	}
	else
	{
		OutBuffer.Add(0xDD);
		OutBuffer.Add(static_cast<uint8>((Length >> 24) & 0xFF));
		OutBuffer.Add(static_cast<uint8>((Length >> 16) & 0xFF));
		OutBuffer.Add(static_cast<uint8>((Length >> 8) & 0xFF));
		OutBuffer.Add(static_cast<uint8>(Length & 0xFF));
	}
}

void UEldaraNetworkSubsystem::FMsgPackWriter::WriteUInt(uint64 Value)
{
	if (Value <= 0x7F)
	{
		OutBuffer.Add(static_cast<uint8>(Value));
		return;
	}

	if (Value <= 0xFF)
	{
		OutBuffer.Add(0xCC);
		OutBuffer.Add(static_cast<uint8>(Value));
		return;
	}

	if (Value <= 0xFFFF)
	{
		OutBuffer.Add(0xCD);
		OutBuffer.Add(static_cast<uint8>((Value >> 8) & 0xFF));
		OutBuffer.Add(static_cast<uint8>(Value & 0xFF));
		return;
	}

	if (Value <= 0xFFFFFFFF)
	{
		OutBuffer.Add(0xCE);
		OutBuffer.Add(static_cast<uint8>((Value >> 24) & 0xFF));
		OutBuffer.Add(static_cast<uint8>((Value >> 16) & 0xFF));
		OutBuffer.Add(static_cast<uint8>((Value >> 8) & 0xFF));
		OutBuffer.Add(static_cast<uint8>(Value & 0xFF));
		return;
	}

	OutBuffer.Add(0xCF);
	for (int Shift = 56; Shift >= 0; Shift -= 8)
	{
		OutBuffer.Add(static_cast<uint8>((Value >> Shift) & 0xFF));
	}
}

void UEldaraNetworkSubsystem::FMsgPackWriter::WriteInt(int64 Value)
{
	if (Value >= 0)
	{
		WriteUInt(static_cast<uint64>(Value));
		return;
	}

	if (Value >= -32)
	{
		OutBuffer.Add(static_cast<uint8>(Value));
		return;
	}

	if (Value >= INT8_MIN)
	{
		OutBuffer.Add(0xD0);
		OutBuffer.Add(static_cast<uint8>(Value));
		return;
	}

	if (Value >= INT16_MIN)
	{
		OutBuffer.Add(0xD1);
		OutBuffer.Add(static_cast<uint8>((Value >> 8) & 0xFF));
		OutBuffer.Add(static_cast<uint8>(Value & 0xFF));
		return;
	}

	if (Value >= INT32_MIN)
	{
		OutBuffer.Add(0xD2);
		OutBuffer.Add(static_cast<uint8>((Value >> 24) & 0xFF));
		OutBuffer.Add(static_cast<uint8>((Value >> 16) & 0xFF));
		OutBuffer.Add(static_cast<uint8>((Value >> 8) & 0xFF));
		OutBuffer.Add(static_cast<uint8>(Value & 0xFF));
		return;
	}

	OutBuffer.Add(0xD3);
	for (int Shift = 56; Shift >= 0; Shift -= 8)
	{
		OutBuffer.Add(static_cast<uint8>((Value >> Shift) & 0xFF));
	}
}

void UEldaraNetworkSubsystem::FMsgPackWriter::WriteFloat(float Value)
{
	OutBuffer.Add(0xCA);
	uint32 Raw;
	FMemory::Memcpy(&Raw, &Value, sizeof(float));
	OutBuffer.Add(static_cast<uint8>((Raw >> 24) & 0xFF));
	OutBuffer.Add(static_cast<uint8>((Raw >> 16) & 0xFF));
	OutBuffer.Add(static_cast<uint8>((Raw >> 8) & 0xFF));
	OutBuffer.Add(static_cast<uint8>(Raw & 0xFF));
}

void UEldaraNetworkSubsystem::FMsgPackWriter::WriteBool(bool bValue)
{
	OutBuffer.Add(bValue ? 0xC3 : 0xC2);
}
