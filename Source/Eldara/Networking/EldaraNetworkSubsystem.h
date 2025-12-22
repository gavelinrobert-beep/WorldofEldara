#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "Tickable.h"
#include "Eldara/Networking/EldaraProtocolTypes.h"
#include "EldaraNetworkSubsystem.generated.h"

class FSocket;
class AEldaraCharacterBase;
class AEldaraNPCBase;

USTRUCT(BlueprintType)
struct FEldaraMovementUpdate
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	int32 EntityId = 0;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	FVector Position = FVector::ZeroVector;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	FVector Velocity = FVector::ZeroVector;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	float RotationYaw = 0.f;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	float RotationPitch = 0.f;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	EEldaraMovementState State = EEldaraMovementState::Idle;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	int64 ServerTimestamp = 0;
};

USTRUCT(BlueprintType)
struct FEldaraEntitySpawn
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	int32 EntityId = 0;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	EEldaraEntityType Type = EEldaraEntityType::Player;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	FString Name;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	FVector Position = FVector::ZeroVector;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	float RotationYaw = 0.f;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	int32 Level = 1;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	bool bIsHostile = false;
};

USTRUCT(BlueprintType)
struct FEldaraPlayerSpawn
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	int32 CharacterId = 0;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	FString CharacterName;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	FVector Position = FVector::ZeroVector;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	FString ZoneId;

	UPROPERTY(BlueprintReadOnly, Category = "Eldara|Network")
	int64 ServerTime = 0;
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnEldaraConnected);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnEldaraDisconnected, const FString&, Reason);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnEldaraPlayerSpawned, const FEldaraPlayerSpawn&, Spawn);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnEldaraEntitySpawned, const FEldaraEntitySpawn&, Spawn);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnEldaraEntityDespawned, int32, EntityId);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnEldaraMovementUpdate, const FEldaraMovementUpdate&, Movement);

/**
 * Lightweight TCP client for the authoritative .NET server.
 * Handles shared protocol packets and dispatches visualization events.
 */
UCLASS()
class ELDARA_API UEldaraNetworkSubsystem : public UGameInstanceSubsystem, public FTickableGameObject
{
	GENERATED_BODY()

public:
	UEldaraNetworkSubsystem();

	virtual void Initialize(FSubsystemCollectionBase& Collection) override;
	virtual void Deinitialize() override;

	virtual void Tick(float DeltaTime) override;
	virtual bool IsTickable() const override { return true; }
	virtual TStatId GetStatId() const override;

	UFUNCTION(BlueprintCallable, Category = "Eldara|Network")
	bool ConnectToServer(const FString& Host, int32 Port);

	UFUNCTION(BlueprintCallable, Category = "Eldara|Network")
	void Disconnect();

	UFUNCTION(BlueprintCallable, Category = "Eldara|Network")
	bool SendMovementInput(const FVector2D& MoveInput, const FRotator& Look, float DeltaTime, const FVector& PredictedPosition);

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Network")
	FOnEldaraConnected OnConnected;

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Network")
	FOnEldaraDisconnected OnDisconnected;

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Network")
	FOnEldaraPlayerSpawned OnPlayerSpawn;

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Network")
	FOnEldaraEntitySpawned OnEntitySpawn;

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Network")
	FOnEldaraEntityDespawned OnEntityDespawn;

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Network")
	FOnEldaraMovementUpdate OnMovementUpdate;

	/** Optional override classes for spawning visuals. */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Eldara|Network")
	TSubclassOf<AEldaraCharacterBase> RemotePlayerClass;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Eldara|Network")
	TSubclassOf<AEldaraNPCBase> RemoteNPCClass;

	private:
		enum class EPacketType : uint16
		{
			LoginRequest = 0,
			LoginResponse = 1,
			MovementInput = 10,
			MovementUpdate = 11,
			PositionCorrection = 12,
			MovementSync = 13,
			ChatMessage = 30,
			EnterWorld = 100,
			LeaveWorld = 101,
			EntitySpawn = 102,
			EntityDespawn = 103,
			PlayerSpawn = 105,
			NPCStateUpdate = 106
		};

	public:
		struct FMsgPackReader
		{
			const TArray<uint8>& Buffer;
			int32 Offset = 0;

		explicit FMsgPackReader(const TArray<uint8>& InBuffer)
			: Buffer(InBuffer)
		{
		}

		bool ReadArrayHeader(uint32& OutLength);
		bool ReadMapHeader(uint32& OutLength);
		bool ReadString(FString& Out);
		bool ReadFloat(float& Out);
		bool ReadBool(bool& Out);
		bool ReadInt64(int64& Out);
		bool ReadUInt64(uint64& Out);
		bool SkipValue();

	private:
		bool ReadBytes(int32 Count, const uint8*& OutPtr);
	};

	struct FMsgPackWriter
	{
		TArray<uint8>& OutBuffer;

		explicit FMsgPackWriter(TArray<uint8>& InBuffer)
			: OutBuffer(InBuffer)
		{
		}

		void WriteArrayHeader(uint32 Length);
		void WriteUInt(uint64 Value);
		void WriteInt(int64 Value);
		void WriteFloat(float Value);
		void WriteBool(bool bValue);
	};

	private:
		struct FSocketDeleter
		{
			void operator()(FSocket* InSocket) const;
		};

	using FEldaraSocketPtr = TUniquePtr<FSocket, FSocketDeleter>;

	bool ReceivePacket(TArray<uint8>& OutPacket);
	void ProcessPacket(const TArray<uint8>& PacketData);

	bool ParseMovementUpdate(FMsgPackReader& Reader, FEldaraMovementUpdate& OutUpdate);
	bool ParseVector3(FMsgPackReader& Reader, FVector& OutVector);
	bool ParsePlayerSpawn(FMsgPackReader& Reader, FEldaraPlayerSpawn& OutSpawn);
	bool ParseCharacterSnapshot(FMsgPackReader& Reader, FEldaraPlayerSpawn& OutSpawn);
	bool ParseCharacterPosition(FMsgPackReader& Reader, FEldaraPlayerSpawn& OutSpawn);
	bool ParseEntitySpawn(FMsgPackReader& Reader, FEldaraEntitySpawn& OutSpawn);
	bool ParseEntityDespawn(FMsgPackReader& Reader, int32& OutEntityId);
	bool ParseNPCStateUpdate(FMsgPackReader& Reader, int32& OutEntityId, EEldaraNPCServerState& OutState);

	void HandleMovementUpdate(const FEldaraMovementUpdate& Update);
	void HandlePlayerSpawn(const FEldaraPlayerSpawn& Spawn);
	void HandleEntitySpawn(const FEldaraEntitySpawn& Spawn);
	void HandleEntityDespawn(int32 EntityId);
	void HandleNPCState(int32 EntityId, EEldaraNPCServerState State);

	void SendRawPacket(const TArray<uint8>& Payload);

	static constexpr int32 MaxPacketSize = 32768;

	FEldaraSocketPtr Socket;
	int32 ExpectedPacketSize;
	TArray<uint8> PartialBuffer;
	FString ConnectedHost;
	int32 ConnectedPort;

	int32 LocalEntityId;
	int32 LocalCharacterId;
	uint32 LocalInputSequence;

	TMap<int32, TWeakObjectPtr<AActor>> EntityActors;
	TWeakObjectPtr<AEldaraCharacterBase> LocalPlayer;
};
