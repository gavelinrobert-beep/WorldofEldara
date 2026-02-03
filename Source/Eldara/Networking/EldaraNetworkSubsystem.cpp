#include "EldaraNetworkSubsystem.h"
#include "SocketSubsystem.h"
#include "IPAddress.h"
#include "TimerManager.h"

void UEldaraNetworkSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);
	
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Initialized"));
}

void UEldaraNetworkSubsystem::Deinitialize()
{
	// Disconnect and cleanup before shutting down
	Disconnect();
	
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Deinitialized"));
	
	Super::Deinitialize();
}

bool UEldaraNetworkSubsystem::ConnectToGameServer(FString IpAddress, int32 Port)
{
	// If already connected, disconnect first
	if (bIsConnected)
	{
		UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Already connected. Disconnecting first..."));
		Disconnect();
	}
	
	// Get the socket subsystem
	ISocketSubsystem* SocketSubsystem = ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM);
	if (!SocketSubsystem)
	{
		UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Failed to get socket subsystem"));
		return false;
	}
	
	// Create the socket
	ConnectionSocket = SocketSubsystem->CreateSocket(NAME_Stream, TEXT("EldaraNetworkSubsystemSocket"), false);
	if (!ConnectionSocket)
	{
		UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Failed to create socket"));
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
		UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Invalid IP address: %s"), *IpAddress);
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
			UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Failed to connect to %s:%d (Error: %d)"), *IpAddress, Port, (int32)Error);
			ConnectionSocket->Close();
			SocketSubsystem->DestroySocket(ConnectionSocket);
			ConnectionSocket = nullptr;
			return false;
		}
	}
	
	bIsConnected = true;
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Connected to %s:%d"), *IpAddress, Port);
	
	// Start polling for data
	if (UWorld* World = GetWorld())
	{
		World->GetTimerManager().SetTimer(
			PollTimerHandle,
			this,
			&UEldaraNetworkSubsystem::CheckForData,
			PollInterval,
			true
		);
	}
	
	return true;
}

bool UEldaraNetworkSubsystem::ConnectToServer(FString IpAddress, int32 Port)
{
	// Alias for ConnectToGameServer
	return ConnectToGameServer(IpAddress, Port);
}

void UEldaraNetworkSubsystem::Disconnect()
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
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Disconnected"));
}

void UEldaraNetworkSubsystem::CheckForData()
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
				UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Received %d bytes of data"), BytesRead);
				
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
				UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Error receiving data (Error: %d)"), (int32)Error);
				Disconnect();
			}
		}
	}
	
	// Check socket state
	ESocketConnectionState State = ConnectionSocket->GetConnectionState();
	if (State == SCS_ConnectionError || State == SCS_NotConnected)
	{
		UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Connection lost"));
		Disconnect();
	}
}

void UEldaraNetworkSubsystem::SendMovementInput(FVector2D Input, FRotator Rotation, float DeltaTime, FVector Position)
{
	// TODO: Implement proper movement packet sending
	// For now, just log the call
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: SendMovementInput called - Input:(%.2f,%.2f) Rotation:(%.1f,%.1f,%.1f) DeltaTime:%.3f Position:(%.1f,%.1f,%.1f)"),
		Input.X, Input.Y, Rotation.Pitch, Rotation.Yaw, Rotation.Roll, DeltaTime, Position.X, Position.Y, Position.Z);
	
	// When implementing, create and send FMovementInputPacket here
	// Example:
	// FMovementInputPacket Packet;
	// Packet.Forward = Input.X;
	// Packet.Strafe = Input.Y;
	// ... populate other fields ...
	// SendPacket(Packet);
}

void UEldaraNetworkSubsystem::SendLogin(FString Username, FString PasswordHash)
{
	if (!bIsConnected)
	{
		UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Cannot send login - not connected"));
		return;
	}
	
	// Create and populate login request packet
	FLoginRequest Packet;
	Packet.Username = Username;
	Packet.PasswordHash = PasswordHash;
	Packet.ClientVersion = "1.0.0";
	Packet.ProtocolVersion = "1.0.0";
	Packet.Timestamp = FDateTime::UtcNow().ToUnixTimestamp();
	Packet.SequenceNumber = 0;
	
	// Send the packet
	SendPacket(Packet);
	
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Login request sent for user: %s"), *Username);
}

void UEldaraNetworkSubsystem::SendCharacterListRequest()
{
	FCharacterListRequest Request;
	SendPacket(Request);
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Character list request sent"));
}

void UEldaraNetworkSubsystem::SendCreateCharacter(FString Name, ERace Race, EClass Class, EFaction Faction, ETotemSpirit TotemSpirit, FCharacterAppearance Appearance)
{
	FCreateCharacterRequest Request;
	Request.AccountId = 0; // Server will use session account
	Request.Name = Name;
	Request.Race = Race;
	Request.Class = Class;
	Request.Faction = Faction;
	Request.TotemSpirit = TotemSpirit;
	Request.Appearance = Appearance;
	
	SendPacket(Request);
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Create character request sent for '%s'"), *Name);
}

void UEldaraNetworkSubsystem::SendSelectCharacter(int64 CharacterId)
{
	FSelectCharacterRequest Request;
	Request.CharacterId = CharacterId;
	
	SendPacket(Request);
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Select character request sent (ID: %lld)"), CharacterId);
}
