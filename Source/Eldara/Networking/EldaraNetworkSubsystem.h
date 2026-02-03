#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "Sockets.h"
#include "Networking.h"
#include "NetworkTypes.h"
#include "NetworkPackets.h"
#include "PacketSerializer.h"
#include "PacketDeserializer.h"
#include "EldaraNetworkSubsystem.generated.h"

/**
 * Network Subsystem for handling TCP networking with the C# server.
 * Manages socket connections, packet sending, and receiving.
 */
UCLASS()
class ELDARA_API UEldaraNetworkSubsystem : public UGameInstanceSubsystem
{
	GENERATED_BODY()

public:
	// Delegates for server responses
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnLoginResponse, FLoginResponse, Response);
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnCharacterListResponse, FCharacterListResponse, Response);
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnCreateCharacterResponse, FCreateCharacterResponse, Response);
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnSelectCharacterResponse, FSelectCharacterResponse, Response);
	DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnMovementUpdateResponse, FMovementUpdateResponse, Response);

	// Blueprint-assignable events
	UPROPERTY(BlueprintAssignable, Category = "Eldara|Networking")
	FOnLoginResponse OnLoginResponse;

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Networking")
	FOnCharacterListResponse OnCharacterListResponse;

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Networking")
	FOnCreateCharacterResponse OnCreateCharacterResponse;

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Networking")
	FOnSelectCharacterResponse OnSelectCharacterResponse;

	UPROPERTY(BlueprintAssignable, Category = "Eldara|Networking")
	FOnMovementUpdateResponse OnMovementUpdateResponse;

	/** Initialize the subsystem */
	virtual void Initialize(FSubsystemCollectionBase& Collection) override;
	
	/** Deinitialize the subsystem */
	virtual void Deinitialize() override;

	/**
	 * Connect to the game server at the specified IP address and port
	 * @param IpAddress The IP address to connect to (default: 127.0.0.1)
	 * @param Port The port to connect to (default: 7777)
	 * @return True if connection was successful, false otherwise
	 */
	UFUNCTION(BlueprintCallable, Category = "Eldara|Networking")
	bool ConnectToGameServer(FString IpAddress = "127.0.0.1", int32 Port = 7777);

	/**
	 * Connect to the server (alias for ConnectToGameServer)
	 * @param IpAddress The IP address to connect to (default: 127.0.0.1)
	 * @param Port The port to connect to (default: 7777)
	 * @return True if connection was successful, false otherwise
	 */
	UFUNCTION(BlueprintCallable, Category = "Eldara|Networking")
	bool ConnectToServer(FString IpAddress = "127.0.0.1", int32 Port = 7777);

	/**
	 * Disconnect from the game server
	 */
	UFUNCTION(BlueprintCallable, Category = "Eldara|Networking")
	void Disconnect();

	/**
	 * Send movement input to the server
	 * @param Input 2D movement input (X=forward/back, Y=left/right)
	 * @param Rotation Player rotation
	 * @param DeltaTime Time since last update
	 * @param Position Current player position
	 */
	UFUNCTION(BlueprintCallable, Category = "Eldara|Networking")
	void SendMovementInput(FVector2D Input, FRotator Rotation, float DeltaTime, FVector Position);

	/**
	 * Send login request to the server
	 * @param Username The username for authentication
	 * @param PasswordHash The hashed password for authentication
	 */
	UFUNCTION(BlueprintCallable, Category = "Eldara|Networking")
	void SendLogin(FString Username, FString PasswordHash);

	/**
	 * Send character list request to the server
	 * Requests the list of characters for the current account
	 */
	UFUNCTION(BlueprintCallable, Category = "Eldara|Networking")
	void SendCharacterListRequest();

	/**
	 * Send create character request to the server
	 * @param Name Character name
	 * @param Race Character race
	 * @param Class Character class
	 * @param Faction Character faction
	 * @param TotemSpirit Totem spirit (if applicable)
	 * @param Appearance Character appearance customization
	 */
	UFUNCTION(BlueprintCallable, Category = "Eldara|Networking")
	void SendCreateCharacter(FString Name, ERace Race, EClass Class, EFaction Faction, ETotemSpirit TotemSpirit, FCharacterAppearance Appearance);

	/**
	 * Send select character request to the server
	 * @param CharacterId The ID of the character to select
	 */
	UFUNCTION(BlueprintCallable, Category = "Eldara|Networking")
	void SendSelectCharacter(int64 CharacterId);

	/**
	 * Send a packet to the server
	 * Template function for sending typed packets
	 */
	template<typename T>
	void SendPacket(const T& Packet)
	{
		static_assert(TIsDerivedFrom<T, FPacketBase>::Value, "T must derive from FPacketBase");
		
		if (!ConnectionSocket || !bIsConnected)
		{
			UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Cannot send packet - not connected"));
			return;
		}
		
		// Serialize the packet using MessagePack format (templated version for proper type detection)
		TArray<uint8> SerializedData;
		if (!FPacketSerializer::Serialize(Packet, SerializedData))
		{
			UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Failed to serialize packet"));
			return;
		}
		
		// Prepend 4-byte length prefix (Little Endian) as expected by C# server
		int32 PayloadSize = SerializedData.Num();
		if (PayloadSize <= 0)
		{
			UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Cannot send empty packet"));
			return;
		}
		
		// Check against maximum packet size (8KB as defined in C# NetworkConstants.MaxPacketSize)
		// Note: C# server validates the payload size only (excluding the length prefix)
		if (PayloadSize > MaxPacketSize)
		{
			UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Packet too large (%d bytes, max %d bytes)"), PayloadSize, MaxPacketSize);
			return;
		}
		
		// Allocate final packet buffer with length prefix + payload in a single allocation
		TArray<uint8> FinalPacket;
		FinalPacket.SetNumUninitialized(LengthPrefixSize + PayloadSize);
		uint8* PacketData = FinalPacket.GetData();
		
		// Write 4-byte length prefix as Little Endian bytes
		PacketData[0] = static_cast<uint8>(PayloadSize & 0xFF);
		PacketData[1] = static_cast<uint8>((PayloadSize >> 8) & 0xFF);
		PacketData[2] = static_cast<uint8>((PayloadSize >> 16) & 0xFF);
		PacketData[3] = static_cast<uint8>((PayloadSize >> 24) & 0xFF);
		
		// Copy the serialized payload after the length prefix
		FMemory::Memcpy(PacketData + LengthPrefixSize, SerializedData.GetData(), PayloadSize);
		
		// Send the final packet with length prefix
		int32 BytesSent = 0;
		if (!ConnectionSocket->Send(FinalPacket.GetData(), FinalPacket.Num(), BytesSent))
		{
			UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Failed to send packet"));
			return;
		}
		
		if (BytesSent != FinalPacket.Num())
		{
			UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Partial send (%d of %d bytes)"), BytesSent, FinalPacket.Num());
		}
		else
		{
			UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Successfully sent packet (%d bytes payload, %d bytes total)"), PayloadSize, FinalPacket.Num());
		}
	}

	/**
	 * Check if the subsystem is currently connected to a server
	 */
	UFUNCTION(BlueprintPure, Category = "Eldara|Networking")
	bool IsConnected() const { return bIsConnected; }

private:
	/** Network protocol constants matching C# server NetworkConstants */
	static constexpr int32 MaxPacketSize = 8192;  // 8KB - C# NetworkConstants.MaxPacketSize
	static constexpr int32 LengthPrefixSize = 4;  // 4-byte int32 length prefix
	
	/** Polling interval for checking socket data (60 times per second) */
	static constexpr float PollInterval = 0.016f;
	
	/** The TCP socket connection to the server */
	FSocket* ConnectionSocket = nullptr;
	
	/** Whether we're currently connected */
	bool bIsConnected = false;
	
	/** Timer handle for polling data from the socket */
	FTimerHandle PollTimerHandle;
	
	/**
	 * Check for incoming data on the socket
	 * Called periodically by a timer
	 */
	void CheckForData();
	
	/**
	 * Process received packet data
	 * @param Data Raw packet data (without length prefix)
	 */
	void ProcessReceivedData(const TArray<uint8>& Data);
	
	/** Buffer for assembling multi-part packets */
	TArray<uint8> ReceiveBuffer;
	
	/** Expected size of the current packet being received */
	int32 ExpectedPacketSize = 0;
};
