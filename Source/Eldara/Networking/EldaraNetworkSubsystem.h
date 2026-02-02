#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "Sockets.h"
#include "Networking.h"
#include "NetworkTypes.h"
#include "NetworkPackets.h"
#include "PacketSerializer.h"
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
		
		TArray<uint8> FinalPacket;
		FinalPacket.Reserve(4 + PayloadSize);
		
		// Add 4-byte length prefix (Little Endian) using AddUninitialized for efficiency
		int32 SizeIndex = FinalPacket.AddUninitialized(4);
		uint8* SizePtr = FinalPacket.GetData() + SizeIndex;
		
		// Write length as Little Endian bytes explicitly
		SizePtr[0] = static_cast<uint8>(PayloadSize & 0xFF);
		SizePtr[1] = static_cast<uint8>((PayloadSize >> 8) & 0xFF);
		SizePtr[2] = static_cast<uint8>((PayloadSize >> 16) & 0xFF);
		SizePtr[3] = static_cast<uint8>((PayloadSize >> 24) & 0xFF);
		
		// Append the serialized payload
		FinalPacket.Append(SerializedData);
		
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
			UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Successfully sent packet (%d bytes payload, %d bytes total)"), PayloadSize, BytesSent);
		}
	}

	/**
	 * Check if the subsystem is currently connected to a server
	 */
	UFUNCTION(BlueprintPure, Category = "Eldara|Networking")
	bool IsConnected() const { return bIsConnected; }

private:
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
};
