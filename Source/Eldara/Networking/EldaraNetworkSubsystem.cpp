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
	
	// Clear receive buffers
	ReceiveBuffer.Empty();
	ExpectedPacketSize = 0;
	
	bIsConnected = false;
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Disconnected"));
}

void UEldaraNetworkSubsystem::CheckForData()
{
	if (!ConnectionSocket || !bIsConnected)
	{
		return;
	}
	
	// === ADD THIS LOG ===
	UE_LOG(LogTemp, VeryVerbose, TEXT("EldaraNetworkSubsystem: CheckForData - checking for data..."));
	
	// Check if there's pending data
	uint32 PendingDataSize = 0;
	if (ConnectionSocket->HasPendingData(PendingDataSize))
	{
		// === ADD THIS LOG ===
		UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: HasPendingData returned TRUE - %d bytes pending"), PendingDataSize);
		
		// Allocate buffer for received data
		TArray<uint8> ReceivedData;
		ReceivedData.SetNumUninitialized(PendingDataSize);
		
		// Read the data
		int32 BytesRead = 0;
		if (ConnectionSocket->Recv(ReceivedData.GetData(), PendingDataSize, BytesRead))
		{
			// === ADD THIS LOG ===
			UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Successfully read %d bytes from socket"), BytesRead);
			
			if (BytesRead > 0)
			{
				// === ADD THIS LOG ===
				UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Appending %d bytes to ReceiveBuffer (current size: %d)"), 
					BytesRead, ReceiveBuffer.Num());
				
				// Append to receive buffer
				ReceiveBuffer.Append(ReceivedData.GetData(), BytesRead);
				
				// Process complete packets from buffer
				while (true)
				{
					// === ADD THIS LOG ===
					UE_LOG(LogTemp, VeryVerbose, TEXT("EldaraNetworkSubsystem: ReceiveBuffer has %d bytes, ExpectedPacketSize: %d"), 
						ReceiveBuffer.Num(), ExpectedPacketSize);
					
					// If we don't have an expected packet size yet, try to read the length prefix
					if (ExpectedPacketSize == 0)
					{
						if (ReceiveBuffer.Num() < LengthPrefixSize)
						{
							// === ADD THIS LOG ===
							UE_LOG(LogTemp, VeryVerbose, TEXT("EldaraNetworkSubsystem: Waiting for length prefix (%d/%d bytes)"), 
								ReceiveBuffer.Num(), LengthPrefixSize);
							// Not enough data for length prefix yet
							break;
						}
						
						// Read 4-byte length prefix (Little Endian)
						ExpectedPacketSize = ReceiveBuffer[0] | (ReceiveBuffer[1] << 8) | 
						                    (ReceiveBuffer[2] << 16) | (ReceiveBuffer[3] << 24);
						
						// === ADD THIS LOG ===
						UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Read length prefix - expecting %d byte packet"), ExpectedPacketSize);
						
						// Validate packet size
						if (ExpectedPacketSize <= 0 || ExpectedPacketSize > MaxPacketSize)
						{
							UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Invalid packet size: %d"), ExpectedPacketSize);
							Disconnect();
							return;
						}
						
						// === ADD THIS LOG ===
						UE_LOG(LogTemp, VeryVerbose, TEXT("EldaraNetworkSubsystem: Removing %d byte length prefix"), LengthPrefixSize);
						
						// Remove length prefix from buffer
						ReceiveBuffer.RemoveAt(0, LengthPrefixSize, EAllowShrinking::No);
					}
					
					// Check if we have the complete packet
					if (ReceiveBuffer.Num() < ExpectedPacketSize)
					{
						// === ADD THIS LOG ===
						UE_LOG(LogTemp, VeryVerbose, TEXT("EldaraNetworkSubsystem: Waiting for complete packet (%d/%d bytes)"), 
							ReceiveBuffer.Num(), ExpectedPacketSize);
						// Need more data
						break;
					}
					
					// === ADD THIS LOG ===
					UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Complete packet received (%d bytes), processing..."), ExpectedPacketSize);
					
					// Extract packet data
					TArray<uint8> PacketData;
					PacketData.Append(ReceiveBuffer.GetData(), ExpectedPacketSize);
					
					// Remove packet from buffer
					ReceiveBuffer.RemoveAt(0, ExpectedPacketSize, EAllowShrinking::No);
					
					// === ADD THIS LOG ===
					UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Calling ProcessReceivedData with %d bytes"), PacketData.Num());
					
					// Process the packet
					ProcessReceivedData(PacketData);
					
					// Reset for next packet
					ExpectedPacketSize = 0;
					
					// === ADD THIS LOG ===
					UE_LOG(LogTemp, VeryVerbose, TEXT("EldaraNetworkSubsystem: Packet processed, checking for more packets..."));
				}
			}
			else
			{
				// === ADD THIS LOG ===
				UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Recv succeeded but BytesRead = 0"));
			}
		}
		else
		{
			// === ADD THIS LOG ===
			UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Recv() returned false even though HasPendingData was true"));
			
			// Error reading from socket
			ESocketErrors Error = ISocketSubsystem::Get(PLATFORM_SOCKETSUBSYSTEM)->GetLastErrorCode();
			if (Error != SE_EWOULDBLOCK && Error != SE_NO_ERROR)
			{
				UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Error receiving data (Error: %d)"), (int32)Error);
				Disconnect();
			}
		}
	}
	else
	{
		// === ADD THIS LOG (only once per second to avoid spam) ===
		static float LogTimer = 0.0f;
		LogTimer += PollInterval;
		if (LogTimer >= 1.0f)
		{
			UE_LOG(LogTemp, VeryVerbose, TEXT("EldaraNetworkSubsystem: No pending data on socket"));
			LogTimer = 0.0f;
		}
	}
	
	// Check socket state
	ESocketConnectionState State = ConnectionSocket->GetConnectionState();
	if (State == SCS_ConnectionError || State == SCS_NotConnected)
	{
		UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Connection lost (State: %d)"), (int32)State);
		Disconnect();
	}
}

void UEldaraNetworkSubsystem::ProcessReceivedData(const TArray<uint8>& Data)
{
	// === ADD THIS LOG ===
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: ProcessReceivedData called with %d bytes"), Data.Num());
	
	// Determine packet type
	int32 PacketType = -1;
	if (!FPacketDeserializer::Deserialize(Data, PacketType))
	{
		UE_LOG(LogTemp, Error, TEXT("EldaraNetworkSubsystem: Failed to determine packet type"));
		return;
	}
	
	// === ADD THIS LOG ===
	UE_LOG(LogTemp, Log, TEXT("EldaraNetworkSubsystem: Received packet type %d, routing to deserializer..."), PacketType);
	
	// Deserialize based on packet type
	switch (PacketType)
	{
		case 1: // LoginResponse
		{
			FLoginResponse Response;
			if (FPacketDeserializer::DeserializeLoginResponse(Data, Response))
			{
				OnLoginResponse.Broadcast(Response);
			}
			break;
		}
		
		case 3: // CharacterListResponse
		{
			FCharacterListResponse Response;
			if (FPacketDeserializer::DeserializeCharacterListResponse(Data, Response))
			{
				OnCharacterListResponse.Broadcast(Response);
			}
			break;
		}
		
		case 5: // CreateCharacterResponse
		{
			FCreateCharacterResponse Response;
			if (FPacketDeserializer::DeserializeCreateCharacterResponse(Data, Response))
			{
				OnCreateCharacterResponse.Broadcast(Response);
			}
			break;
		}
		
		case 7: // SelectCharacterResponse
		{
			FSelectCharacterResponse Response;
			if (FPacketDeserializer::DeserializeSelectCharacterResponse(Data, Response))
			{
				OnSelectCharacterResponse.Broadcast(Response);
			}
			break;
		}
		
		case 11: // MovementUpdateResponse
		{
			FMovementUpdateResponse Response;
			if (FPacketDeserializer::DeserializeMovementUpdateResponse(Data, Response))
			{
				OnMovementUpdateResponse.Broadcast(Response);
			}
			break;
		}
		
		default:
			UE_LOG(LogTemp, Warning, TEXT("EldaraNetworkSubsystem: Unhandled packet type %d"), PacketType);
			break;
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
	Request.AccountId = 0; // AccountId set to 0 as server derives account from session token
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

bool UEldaraNetworkSubsystem::IsResponseSuccess(EResponseCode ResponseCode)
{
	return ResponseCode == EResponseCode::Success;
}

FString UEldaraNetworkSubsystem::ResponseCodeToString(EResponseCode ResponseCode)
{
	switch (ResponseCode)
	{
		case EResponseCode::Success:
			return TEXT("Success");
		case EResponseCode::Error:
			return TEXT("Error");
		case EResponseCode::InvalidRequest:
			return TEXT("Invalid Request");
		case EResponseCode::NotAuthenticated:
			return TEXT("Not Authenticated");
		case EResponseCode::AlreadyExists:
			return TEXT("Already Exists");
		case EResponseCode::NotFound:
			return TEXT("Not Found");
		case EResponseCode::InsufficientPermissions:
			return TEXT("Insufficient Permissions");
		case EResponseCode::InvalidData:
			return TEXT("Invalid Data");
		case EResponseCode::ServerError:
			return TEXT("Server Error");
		case EResponseCode::Timeout:
			return TEXT("Timeout");
		case EResponseCode::NameTaken:
			return TEXT("Name Taken");
		case EResponseCode::InvalidName:
			return TEXT("Invalid Name");
		case EResponseCode::InvalidRaceClassCombination:
			return TEXT("Invalid Race-Class Combination");
		case EResponseCode::MaxCharactersReached:
			return TEXT("Max Characters Reached");
		case EResponseCode::NotInRange:
			return TEXT("Not In Range");
		case EResponseCode::NotEnoughMana:
			return TEXT("Not Enough Mana");
		case EResponseCode::OnCooldown:
			return TEXT("On Cooldown");
		case EResponseCode::Interrupted:
			return TEXT("Interrupted");
		case EResponseCode::InvalidTarget:
			return TEXT("Invalid Target");
		case EResponseCode::InsufficientResources:
			return TEXT("Insufficient Resources");
		case EResponseCode::LoreInconsistency:
			return TEXT("Lore Inconsistency");
		default:
			return TEXT("Unknown");
	}
}
