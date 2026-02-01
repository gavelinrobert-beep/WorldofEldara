#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "Sockets.h"
#include "Networking.h"
#include "NetworkTypes.h"
#include "NetworkPackets.h"
#include "NetworkSubsystem.generated.h"

/**
 * Network Subsystem for handling TCP networking with the C# server.
 * Manages socket connections, packet sending, and receiving.
 */
UCLASS()
class ELDARA_API UNetworkSubsystem : public UGameInstanceSubsystem
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
	 * Disconnect from the game server
	 */
	UFUNCTION(BlueprintCallable, Category = "Eldara|Networking")
	void Disconnect();

	/**
	 * Send a packet to the server
	 * Template function for sending typed packets
	 */
	template<typename T>
	void SendPacket(const T& Packet)
	{
		static_assert(TIsDerivedFrom<T, FPacketBase>::Value, "T must derive from FPacketBase");
		SerializeAndSend(Packet);
	}

	/**
	 * Check if the subsystem is currently connected to a server
	 */
	UFUNCTION(BlueprintPure, Category = "Eldara|Networking")
	bool IsConnected() const { return bIsConnected; }

private:
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
	 * Serialize and send a packet to the server
	 * Currently a placeholder implementation
	 * @param Packet The packet to serialize and send
	 */
	void SerializeAndSend(const FPacketBase& Packet);
	
	/**
	 * Helper function to get the packet type as a string
	 */
	FString GetPacketTypeName(const FPacketBase& Packet) const;
};
