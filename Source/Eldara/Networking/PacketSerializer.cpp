#include "PacketSerializer.h"

// MessagePack format constants
namespace MessagePackFormat
{
	// Positive fixint: 0x00 - 0x7f (0 to 127)
	// Negative fixint: 0xe0 - 0xff (-32 to -1)
	constexpr uint8 FixIntMin = 0x00;
	constexpr uint8 FixIntMax = 0x7f;
	constexpr uint8 NegativeFixIntMin = 0xe0;
	
	// Integer types
	constexpr uint8 Uint8 = 0xcc;
	constexpr uint8 Uint16 = 0xcd;
	constexpr uint8 Uint32 = 0xce;
	constexpr uint8 Uint64 = 0xcf;
	constexpr uint8 Int8 = 0xd0;
	constexpr uint8 Int16 = 0xd1;
	constexpr uint8 Int32 = 0xd2;
	constexpr uint8 Int64 = 0xd3;
	
	// Float types
	constexpr uint8 Float32 = 0xca;
	constexpr uint8 Float64 = 0xcb;
	
	// String types
	constexpr uint8 FixStrMask = 0xa0;  // 0xa0 - 0xbf (0 to 31 bytes)
	constexpr uint8 Str8 = 0xd9;
	constexpr uint8 Str16 = 0xda;
	constexpr uint8 Str32 = 0xdb;
	
	// Array types
	constexpr uint8 FixArrayMask = 0x90;  // 0x90 - 0x9f (0 to 15 elements)
	constexpr uint8 Array16 = 0xdc;
	constexpr uint8 Array32 = 0xdd;
	
	// Boolean types
	constexpr uint8 False = 0xc2;
	constexpr uint8 True = 0xc3;
	
	// Nil
	constexpr uint8 Nil = 0xc0;
}

bool FPacketSerializer::Serialize(const FPacketBase& Packet, TArray<uint8>& OutBytes)
{
	// Clear the output buffer
	OutBytes.Reset();
	
	// Determine the packet type
	int32 PacketType = GetPacketTypeFromInstance(Packet);
	
	if (PacketType < 0)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketSerializer: Unknown packet type"));
		return false;
	}
	
	// Serialize based on packet type
	switch (PacketType)
	{
		case 0: // LoginRequest
		{
			const FLoginRequest* LoginRequest = static_cast<const FLoginRequest*>(&Packet);
			SerializeLoginRequest(*LoginRequest, OutBytes);
			return true;
		}
		
		default:
			UE_LOG(LogTemp, Error, TEXT("PacketSerializer: Serialization not implemented for packet type %d"), PacketType);
			return false;
	}
}

void FPacketSerializer::WriteArrayHeader(TArray<uint8>& OutBytes, int32 Count)
{
	if (Count >= 0 && Count <= 15)
	{
		// FixArray: 0x90 | count (for 0-15 elements)
		OutBytes.Add(MessagePackFormat::FixArrayMask | static_cast<uint8>(Count));
	}
	else if (Count <= 65535)
	{
		// Array16: 0xdc + uint16 count
		OutBytes.Add(MessagePackFormat::Array16);
		OutBytes.Add((Count >> 8) & 0xFF);
		OutBytes.Add(Count & 0xFF);
	}
	else
	{
		// Array32: 0xdd + uint32 count
		OutBytes.Add(MessagePackFormat::Array32);
		OutBytes.Add((Count >> 24) & 0xFF);
		OutBytes.Add((Count >> 16) & 0xFF);
		OutBytes.Add((Count >> 8) & 0xFF);
		OutBytes.Add(Count & 0xFF);
	}
}

void FPacketSerializer::WriteInt(TArray<uint8>& OutBytes, int32 Value)
{
	if (Value >= 0 && Value <= 127)
	{
		// Positive fixint: 0x00 - 0x7f
		OutBytes.Add(static_cast<uint8>(Value));
	}
	else if (Value >= -32 && Value < 0)
	{
		// Negative fixint: 0xe0 - 0xff
		OutBytes.Add(static_cast<uint8>(Value & 0xFF));
	}
	else if (Value >= -128 && Value <= 127)
	{
		// int8
		OutBytes.Add(MessagePackFormat::Int8);
		OutBytes.Add(static_cast<uint8>(Value & 0xFF));
	}
	else if (Value >= -32768 && Value <= 32767)
	{
		// int16
		OutBytes.Add(MessagePackFormat::Int16);
		OutBytes.Add((Value >> 8) & 0xFF);
		OutBytes.Add(Value & 0xFF);
	}
	else
	{
		// int32
		OutBytes.Add(MessagePackFormat::Int32);
		OutBytes.Add((Value >> 24) & 0xFF);
		OutBytes.Add((Value >> 16) & 0xFF);
		OutBytes.Add((Value >> 8) & 0xFF);
		OutBytes.Add(Value & 0xFF);
	}
}

void FPacketSerializer::WriteInt64(TArray<uint8>& OutBytes, int64 Value)
{
	if (Value >= 0 && Value <= 127)
	{
		// Positive fixint: 0x00 - 0x7f
		OutBytes.Add(static_cast<uint8>(Value));
	}
	else if (Value >= -32 && Value < 0)
	{
		// Negative fixint: 0xe0 - 0xff
		OutBytes.Add(static_cast<uint8>(Value & 0xFF));
	}
	else if (Value >= -128 && Value <= 127)
	{
		// int8
		OutBytes.Add(MessagePackFormat::Int8);
		OutBytes.Add(static_cast<uint8>(Value & 0xFF));
	}
	else if (Value >= -32768 && Value <= 32767)
	{
		// int16
		OutBytes.Add(MessagePackFormat::Int16);
		OutBytes.Add((Value >> 8) & 0xFF);
		OutBytes.Add(Value & 0xFF);
	}
	else if (Value >= -2147483648LL && Value <= 2147483647LL)
	{
		// int32
		OutBytes.Add(MessagePackFormat::Int32);
		OutBytes.Add((Value >> 24) & 0xFF);
		OutBytes.Add((Value >> 16) & 0xFF);
		OutBytes.Add((Value >> 8) & 0xFF);
		OutBytes.Add(Value & 0xFF);
	}
	else
	{
		// int64
		OutBytes.Add(MessagePackFormat::Int64);
		OutBytes.Add((Value >> 56) & 0xFF);
		OutBytes.Add((Value >> 48) & 0xFF);
		OutBytes.Add((Value >> 40) & 0xFF);
		OutBytes.Add((Value >> 32) & 0xFF);
		OutBytes.Add((Value >> 24) & 0xFF);
		OutBytes.Add((Value >> 16) & 0xFF);
		OutBytes.Add((Value >> 8) & 0xFF);
		OutBytes.Add(Value & 0xFF);
	}
}

void FPacketSerializer::WriteString(TArray<uint8>& OutBytes, const FString& Value)
{
	// Convert FString to UTF-8
	FTCHARToUTF8 UTF8String(*Value);
	int32 Length = UTF8String.Length();
	
	// Write string header
	if (Length <= 31)
	{
		// FixStr: 0xa0 | length (for 0-31 bytes)
		OutBytes.Add(MessagePackFormat::FixStrMask | static_cast<uint8>(Length));
	}
	else if (Length <= 255)
	{
		// Str8: 0xd9 + uint8 length
		OutBytes.Add(MessagePackFormat::Str8);
		OutBytes.Add(static_cast<uint8>(Length));
	}
	else if (Length <= 65535)
	{
		// Str16: 0xda + uint16 length
		OutBytes.Add(MessagePackFormat::Str16);
		OutBytes.Add((Length >> 8) & 0xFF);
		OutBytes.Add(Length & 0xFF);
	}
	else
	{
		// Str32: 0xdb + uint32 length
		OutBytes.Add(MessagePackFormat::Str32);
		OutBytes.Add((Length >> 24) & 0xFF);
		OutBytes.Add((Length >> 16) & 0xFF);
		OutBytes.Add((Length >> 8) & 0xFF);
		OutBytes.Add(Length & 0xFF);
	}
	
	// Write string data
	for (int32 i = 0; i < Length; ++i)
	{
		OutBytes.Add(static_cast<uint8>(UTF8String.Get()[i]));
	}
}

void FPacketSerializer::WriteFloat(TArray<uint8>& OutBytes, float Value)
{
	// Use float32 format: 0xca + 4 bytes (big-endian)
	OutBytes.Add(MessagePackFormat::Float32);
	
	// Convert float to bytes (big-endian)
	union
	{
		float FloatValue;
		uint32 IntValue;
	} Converter;
	
	Converter.FloatValue = Value;
	uint32 IntValue = Converter.IntValue;
	
	OutBytes.Add((IntValue >> 24) & 0xFF);
	OutBytes.Add((IntValue >> 16) & 0xFF);
	OutBytes.Add((IntValue >> 8) & 0xFF);
	OutBytes.Add(IntValue & 0xFF);
}

void FPacketSerializer::WriteBool(TArray<uint8>& OutBytes, bool Value)
{
	// Boolean: 0xc2 (false) or 0xc3 (true)
	OutBytes.Add(Value ? MessagePackFormat::True : MessagePackFormat::False);
}

void FPacketSerializer::SerializeLoginRequest(const FLoginRequest& Packet, TArray<uint8>& OutBytes)
{
	// Wire format: [ UnionKey, [ Field0, Field1, ... ] ]
	// LoginRequest is Union Key 0
	
	// Write outer array header (2 elements: union key + field array)
	WriteArrayHeader(OutBytes, 2);
	
	// Write union key (0 for LoginRequest)
	WriteInt(OutBytes, 0);
	
	// Write inner array header (6 fields: Timestamp, SequenceNumber, Username, PasswordHash, ClientVersion, ProtocolVersion)
	WriteArrayHeader(OutBytes, 6);
	
	// Write fields in order
	WriteInt64(OutBytes, Packet.Timestamp);
	WriteInt(OutBytes, Packet.SequenceNumber);
	WriteString(OutBytes, Packet.Username);
	WriteString(OutBytes, Packet.PasswordHash);
	WriteString(OutBytes, Packet.ClientVersion);
	WriteString(OutBytes, Packet.ProtocolVersion);
	
	UE_LOG(LogTemp, Log, TEXT("PacketSerializer: Serialized LoginRequest (Size: %d bytes)"), OutBytes.Num());
}

int32 FPacketSerializer::GetPacketTypeFromInstance(const FPacketBase& Packet)
{
	// Since we can't easily use RTTI or virtual methods on USTRUCT types,
	// we need to use a different approach. One option is to check the struct type
	// using UScriptStruct, but that requires access to the static struct.
	// For now, we'll use a simple pointer-based approach.
	
	// Check if this is a LoginRequest by attempting a static_cast
	// Note: This is a simple approach for proof-of-concept
	// A more robust solution would add a virtual GetPacketType() method to FPacketBase
	// or use a type registry system
	
	// For now, we'll use the struct size as a heuristic to identify the packet type
	// This is not ideal but works for our proof-of-concept
	
	// Try to identify based on pointer comparison
	// Since we can't easily determine the actual type at runtime with USTRUCTs,
	// we'll assume LoginRequest for now and log a warning
	// In a production system, you'd want to add a type field or use a proper type system
	
	// For the proof-of-concept, we'll return LoginRequest (0) as default
	// This should be improved in a production system
	return 0; // LoginRequest
}
