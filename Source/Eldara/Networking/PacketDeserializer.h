#pragma once

#include "CoreMinimal.h"
#include "NetworkPackets.h"

/**
 * Handles deserialization of MessagePack-encoded packets from the server
 * 
 * MessagePack wire format from C# server:
 * Packet: [ UnionKey (int), [ Field0, Field1, ... ] ]
 * 
 * This deserializer reads the union key to determine packet type,
 * then parses the field array according to the packet structure.
 */
class ELDARA_API FPacketDeserializer
{
public:
	/**
	 * Deserialize a packet from raw bytes
	 * @param InBytes Raw MessagePack data from server
	 * @param OutPacketType The packet type ID (union key) that was deserialized
	 * @return true if deserialization succeeded
	 */
	static bool Deserialize(const TArray<uint8>& InBytes, int32& OutPacketType);
	
	/**
	 * Deserialize specific packet types
	 */
	static bool DeserializeLoginResponse(const TArray<uint8>& InBytes, FLoginResponse& OutPacket);
	static bool DeserializeCharacterListResponse(const TArray<uint8>& InBytes, FCharacterListResponse& OutPacket);
	static bool DeserializeCreateCharacterResponse(const TArray<uint8>& InBytes, FCreateCharacterResponse& OutPacket);
	static bool DeserializeSelectCharacterResponse(const TArray<uint8>& InBytes, FSelectCharacterResponse& OutPacket);
	static bool DeserializeMovementUpdateResponse(const TArray<uint8>& InBytes, FMovementUpdateResponse& OutPacket);

private:
	// Current read position in the byte array
	static int32 ReadPosition;
	
	/**
	 * MessagePack format readers
	 */
	static bool ReadArrayHeader(const TArray<uint8>& InBytes, int32& OutCount);
	static bool ReadInt(const TArray<uint8>& InBytes, int32& OutValue);
	static bool ReadInt64(const TArray<uint8>& InBytes, int64& OutValue);
	static bool ReadString(const TArray<uint8>& InBytes, FString& OutValue);
	static bool ReadFloat(const TArray<uint8>& InBytes, float& OutValue);
	static bool ReadBool(const TArray<uint8>& InBytes, bool& OutValue);
	static bool ReadVector(const TArray<uint8>& InBytes, FVector& OutValue);
	static bool ReadRotator(const TArray<uint8>& InBytes, FRotator& OutValue);
	
	/**
	 * Helper to read a single byte at current position
	 */
	static bool ReadByte(const TArray<uint8>& InBytes, uint8& OutByte);
	
	/**
	 * Reset read position for new packet
	 */
	static void ResetReadPosition() { ReadPosition = 0; }
};
