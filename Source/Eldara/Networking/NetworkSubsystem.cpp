#include "NetworkSubsystem.h"
#include "SocketSubsystem.h"
#include "IPAddress.h"
#include "TimerManager.h"
#include "PacketSerializer.h"

void UNetworkSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);
	
	UE_LOG(LogTemp, Log, TEXT("NetworkSubsystem: Initialized"));
}

void UNetworkSubsystem::Deinitialize()
{
	// Disconnect and cleanup before shutting down
	Disconnect();
	
	UE_LOG(LogTemp, Log, TEXT("NetworkSubsystem: Deinitialized"));
	
	Super::Deinitialize();
}

bool UNetworkSubsystem::ConnectToGameServer(FString IpAddress, int32 Port)
{
	// If already connected, disconnect first
	if (bIsConnected)
	{
		UE_LOG(LogTemp, Warning, TEXT("NetworkSubsystem: Already connected. Disconnecting first..."));
		Disconnect();
	}
	
	// Get the socket subsystem
	ISocketSubsystem* SocketSubsystem = ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM);
	if (!SocketSubsystem)
	{
		UE_LOG(LogTemp, Error, TEXT("NetworkSubsystem: Failed to get socket subsystem"));
		return false;
	}
	
	// Create the socket
	ConnectionSocket = SocketSubsystem->CreateSocket(NAME_Stream, TEXT("NetworkSubsystemSocket"), false);
	if (!ConnectionSocket)
	{
		UE_LOG(LogTemp, Error, TEXT("NetworkSubsystem: Failed to create socket"));
		return false;
	}
	
	// Set socket to non-blocking mode
	ConnectionSocket->SetNonBlocking(true);
	
	// Resolve the IP address
	TSharedRef<FInternetAddr> Address = SocketSubsystem->CreateInternetAddr();
	bool bIsValid = false;
	Address->SetIp(*IpAddress, bIsValid);
	Address->SetPort(Port);
	
	if (!bIsValid)
	{
		UE_LOG(LogTemp, Error, TEXT("NetworkSubsystem: Invalid IP address: %s"), *IpAddress);
		ConnectionSocket->Close();
		SocketSubsystem->DestroySocket(ConnectionSocket);
		ConnectionSocket = nullptr;
		return false;
	}
	
	// Attempt to connect
	bool bConnected = ConnectionSocket->Connect(*Address);
	if (!bConnected)
	{
		ESocketErrors Error = SocketSubsystem->GetLastErrorCode();
		// EINPROGRESS/EWOULDBLOCK is expected for non-blocking sockets
		if (Error != SE_EINPROGRESS && Error != SE_EWOULDBLOCK)
		{
			UE_LOG(LogTemp, Error, TEXT("NetworkSubsystem: Failed to connect to %s:%d (Error: %d)"), *IpAddress, Port, (int32)Error);
			ConnectionSocket->Close();
			SocketSubsystem->DestroySocket(ConnectionSocket);
			ConnectionSocket = nullptr;
			return false;
		}
	}
	
	bIsConnected = true;
	UE_LOG(LogTemp, Log, TEXT("NetworkSubsystem: Connected to %s:%d"), *IpAddress, Port);
	
	// Start polling for data
	if (UWorld* World = GetWorld())
	{
		World->GetTimerManager().SetTimer(
			PollTimerHandle,
			this,
			&UNetworkSubsystem::CheckForData,
			PollInterval,
			true
		);
	}
	
	return true;
}

void UNetworkSubsystem::Disconnect()
{
	// Stop polling timer
	if (UWorld* World = GetWorld())
	{
		World->GetTimerManager().ClearTimer(PollTimerHandle);
	}
	
	// Close and destroy socket
	if (ConnectionSocket)
	{
		ConnectionSocket->Close();
		
		if (ISocketSubsystem* SocketSubsystem = ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM))
		{
			SocketSubsystem->DestroySocket(ConnectionSocket);
		}
		
		ConnectionSocket = nullptr;
	}
	
	bIsConnected = false;
	UE_LOG(LogTemp, Log, TEXT("NetworkSubsystem: Disconnected"));
}

void UNetworkSubsystem::CheckForData()
{
	if (!ConnectionSocket || !bIsConnected)
	{
		return;
	}
	
	// Check if there's pending data
	uint32 PendingDataSize = 0;
	if (ConnectionSocket->HasPendingData(PendingDataSize))
	{
		// Allocate buffer for received data
		TArray<uint8> ReceivedData;
		ReceivedData.SetNumUninitialized(PendingDataSize);
		
		// Read the data
		int32 BytesRead = 0;
		if (ConnectionSocket->Recv(ReceivedData.GetData(), PendingDataSize, BytesRead))
		{
			if (BytesRead > 0)
			{
				UE_LOG(LogTemp, Log, TEXT("NetworkSubsystem: Received %d bytes of data"), BytesRead);
				
				// TODO: Parse and handle the received packet
				// For now, just log that we received something
			}
		}
		else
		{
			// Error reading from socket
			ESocketErrors Error = ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM)->GetLastErrorCode();
			if (Error != SE_EWOULDBLOCK && Error != SE_NO_ERROR)
			{
				UE_LOG(LogTemp, Error, TEXT("NetworkSubsystem: Error receiving data (Error: %d)"), (int32)Error);
				Disconnect();
			}
		}
	}
	
	// Check socket state
	ESocketConnectionState State = ConnectionSocket->GetConnectionState();
	if (State == SCS_ConnectionError || State == SCS_NotConnected)
	{
		UE_LOG(LogTemp, Warning, TEXT("NetworkSubsystem: Connection lost"));
		Disconnect();
	}
}

void UNetworkSubsystem::SerializeAndSend(const FPacketBase& Packet)
{
	if (!ConnectionSocket || !bIsConnected)
	{
		UE_LOG(LogTemp, Warning, TEXT("NetworkSubsystem: Cannot send packet - not connected"));
		return;
	}
	
	// Serialize the packet using MessagePack format
	TArray<uint8> SerializedData;
	if (!FPacketSerializer::Serialize(Packet, SerializedData))
	{
		UE_LOG(LogTemp, Error, TEXT("NetworkSubsystem: Failed to serialize packet"));
		return;
	}
	
	// Send the serialized data
	int32 BytesSent = 0;
	if (!ConnectionSocket->Send(SerializedData.GetData(), SerializedData.Num(), BytesSent))
	{
		UE_LOG(LogTemp, Error, TEXT("NetworkSubsystem: Failed to send packet"));
		return;
	}
	
	if (BytesSent != SerializedData.Num())
	{
		UE_LOG(LogTemp, Warning, TEXT("NetworkSubsystem: Partial send (%d of %d bytes)"), BytesSent, SerializedData.Num());
	}
	else
	{
		UE_LOG(LogTemp, Log, TEXT("NetworkSubsystem: Successfully sent packet (%d bytes)"), BytesSent);
	}
}

FString UNetworkSubsystem::GetPacketTypeName(const FPacketBase& Packet) const
{
	// Try to get the struct name using reflection
	// FPacketBase is a USTRUCT, so derived types should have StaticStruct()
	// We need to use the actual derived type's StaticStruct, not the base
	// For now, return a generic name since we can't easily get the derived type at runtime
	return TEXT("Packet");
}
