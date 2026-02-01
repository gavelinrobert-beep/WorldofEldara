#pragma once

#include "CoreMinimal.h"
#include "NetworkTypes.h"
#include "NetworkPackets.h"

/**
 * Helper class for serializing packets to MessagePack format.
 * Implements the MessagePack wire format to communicate with the C# server.
 * 
 * Wire format for packets: [ UnionKey, [ Field0, Field1, ... ] ]
 * Example: LoginRequest (Union Key 0) -> [ 0, [ "Username", "Hash", ... ] ]
 */
class ELDARA_API FPacketSerializer
{
public:
	/**
	 * Serialize a packet to MessagePack format
	 * @param Packet The packet to serialize
	 * @param OutBytes Output buffer to write the serialized data
	 * @return True if serialization succeeded, false otherwise
	 */
	static bool Serialize(const FPacketBase& Packet, TArray<uint8>& OutBytes);

	// ============================================================================
	// MessagePack Primitive Writers
	// ============================================================================

	/**
	 * Write a MessagePack array header
	 * @param OutBytes Output buffer
	 * @param Count Number of elements in the array
	 */
	static void WriteArrayHeader(TArray<uint8>& OutBytes, int32 Count);

	/**
	 * Write a MessagePack integer (int32)
	 * @param OutBytes Output buffer
	 * @param Value The integer value to write
	 */
	static void WriteInt(TArray<uint8>& OutBytes, int32 Value);

	/**
	 * Write a MessagePack 64-bit integer
	 * @param OutBytes Output buffer
	 * @param Value The int64 value to write
	 */
	static void WriteInt64(TArray<uint8>& OutBytes, int64 Value);

	/**
	 * Write a MessagePack string
	 * @param OutBytes Output buffer
	 * @param Value The string value to write
	 */
	static void WriteString(TArray<uint8>& OutBytes, const FString& Value);

	/**
	 * Write a MessagePack float (float32)
	 * @param OutBytes Output buffer
	 * @param Value The float value to write
	 */
	static void WriteFloat(TArray<uint8>& OutBytes, float Value);

	/**
	 * Write a MessagePack boolean
	 * @param OutBytes Output buffer
	 * @param Value The boolean value to write
	 */
	static void WriteBool(TArray<uint8>& OutBytes, bool Value);

private:
	/**
	 * Serialize a LoginRequest packet
	 * @param Packet The LoginRequest packet to serialize
	 * @param OutBytes Output buffer
	 */
	static void SerializeLoginRequest(const FLoginRequest& Packet, TArray<uint8>& OutBytes);

	/**
	 * Determine the packet type from the base packet
	 * This is a helper to identify which specific packet type we're dealing with
	 * @param Packet The base packet
	 * @return The packet type enum value, or -1 if unknown
	 */
	static int32 GetPacketTypeFromInstance(const FPacketBase& Packet);
};
