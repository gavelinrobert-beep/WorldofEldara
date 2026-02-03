#include "PacketDeserializer.h"

// MessagePack format constants (same as serializer)
namespace MessagePackFormat
{
	constexpr uint8 FixIntMax = 0x7f;
	constexpr uint8 NegativeFixIntMin = 0xe0;
	constexpr uint8 Uint8 = 0xcc;
	constexpr uint8 Uint16 = 0xcd;
	constexpr uint8 Uint32 = 0xce;
	constexpr uint8 Uint64 = 0xcf;
	constexpr uint8 Int8 = 0xd0;
	constexpr uint8 Int16 = 0xd1;
	constexpr uint8 Int32 = 0xd2;
	constexpr uint8 Int64 = 0xd3;
	constexpr uint8 Float32 = 0xca;
	constexpr uint8 Float64 = 0xcb;
	constexpr uint8 FixStrMask = 0xa0;
	constexpr uint8 Str8 = 0xd9;
	constexpr uint8 Str16 = 0xda;
	constexpr uint8 Str32 = 0xdb;
	constexpr uint8 FixArrayMask = 0x90;
	constexpr uint8 Array16 = 0xdc;
	constexpr uint8 Array32 = 0xdd;
	constexpr uint8 False = 0xc2;
	constexpr uint8 True = 0xc3;
	constexpr uint8 Nil = 0xc0;
}

int32 FPacketDeserializer::ReadPosition = 0;

bool FPacketDeserializer::ReadByte(const TArray<uint8>& InBytes, uint8& OutByte)
{
	if (ReadPosition >= InBytes.Num())
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Read past end of buffer"));
		return false;
	}
	OutByte = InBytes[ReadPosition++];
	return true;
}

bool FPacketDeserializer::ReadArrayHeader(const TArray<uint8>& InBytes, int32& OutCount)
{
	uint8 Byte;
	if (!ReadByte(InBytes, Byte))
		return false;
	
	if ((Byte & 0xf0) == MessagePackFormat::FixArrayMask)
	{
		// FixArray: 0x90 - 0x9f
		OutCount = Byte & 0x0f;
		return true;
	}
	else if (Byte == MessagePackFormat::Array16)
	{
		// Array16: uint16 count
		uint8 High, Low;
		if (!ReadByte(InBytes, High) || !ReadByte(InBytes, Low))
			return false;
		OutCount = (High << 8) | Low;
		return true;
	}
	else if (Byte == MessagePackFormat::Array32)
	{
		// Array32: uint32 count
		uint8 B1, B2, B3, B4;
		if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
			return false;
		OutCount = (B1 << 24) | (B2 << 16) | (B3 << 8) | B4;
		return true;
	}
	
	UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid array header byte: 0x%02X"), Byte);
	return false;
}

bool FPacketDeserializer::ReadInt(const TArray<uint8>& InBytes, int32& OutValue)
{
	uint8 Byte;
	if (!ReadByte(InBytes, Byte))
		return false;
	
	if (Byte <= MessagePackFormat::FixIntMax)
	{
		// Positive fixint: 0x00 - 0x7f
		OutValue = Byte;
		return true;
	}
	else if (Byte >= MessagePackFormat::NegativeFixIntMin)
	{
		// Negative fixint: 0xe0 - 0xff
		OutValue = static_cast<int8>(Byte);
		return true;
	}
	else if (Byte == MessagePackFormat::Uint8)
	{
		uint8 Val;
		if (!ReadByte(InBytes, Val))
			return false;
		OutValue = Val;
		return true;
	}
	else if (Byte == MessagePackFormat::Uint16)
	{
		uint8 High, Low;
		if (!ReadByte(InBytes, High) || !ReadByte(InBytes, Low))
			return false;
		OutValue = (High << 8) | Low;
		return true;
	}
	else if (Byte == MessagePackFormat::Uint32)
	{
		uint8 B1, B2, B3, B4;
		if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
			return false;
		OutValue = (B1 << 24) | (B2 << 16) | (B3 << 8) | B4;
		return true;
	}
	else if (Byte == MessagePackFormat::Int8)
	{
		uint8 Val;
		if (!ReadByte(InBytes, Val))
			return false;
		OutValue = static_cast<int8>(Val);
		return true;
	}
	else if (Byte == MessagePackFormat::Int16)
	{
		uint8 High, Low;
		if (!ReadByte(InBytes, High) || !ReadByte(InBytes, Low))
			return false;
		OutValue = static_cast<int16>((High << 8) | Low);
		return true;
	}
	else if (Byte == MessagePackFormat::Int32)
	{
		uint8 B1, B2, B3, B4;
		if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
			return false;
		OutValue = static_cast<int32>((B1 << 24) | (B2 << 16) | (B3 << 8) | B4);
		return true;
	}
	
	UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid int byte: 0x%02X"), Byte);
	return false;
}

bool FPacketDeserializer::ReadInt64(const TArray<uint8>& InBytes, int64& OutValue)
{
	uint8 Byte;
	if (!ReadByte(InBytes, Byte))
		return false;
	
	if (Byte <= MessagePackFormat::FixIntMax)
	{
		OutValue = Byte;
		return true;
	}
	else if (Byte >= MessagePackFormat::NegativeFixIntMin)
	{
		OutValue = static_cast<int8>(Byte);
		return true;
	}
	else if (Byte == MessagePackFormat::Uint8)
	{
		uint8 Val;
		if (!ReadByte(InBytes, Val))
			return false;
		OutValue = Val;
		return true;
	}
	else if (Byte == MessagePackFormat::Uint16)
	{
		uint8 High, Low;
		if (!ReadByte(InBytes, High) || !ReadByte(InBytes, Low))
			return false;
		OutValue = (High << 8) | Low;
		return true;
	}
	else if (Byte == MessagePackFormat::Uint32)
	{
		uint8 B1, B2, B3, B4;
		if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
			return false;
		OutValue = (static_cast<uint32>(B1) << 24) | (B2 << 16) | (B3 << 8) | B4;
		return true;
	}
	else if (Byte == MessagePackFormat::Uint64)
	{
		uint8 B[8];
		for (int i = 0; i < 8; i++)
		{
			if (!ReadByte(InBytes, B[i]))
				return false;
		}
		OutValue = (static_cast<uint64>(B[0]) << 56) | (static_cast<uint64>(B[1]) << 48) |
		           (static_cast<uint64>(B[2]) << 40) | (static_cast<uint64>(B[3]) << 32) |
		           (static_cast<uint64>(B[4]) << 24) | (static_cast<uint64>(B[5]) << 16) |
		           (static_cast<uint64>(B[6]) << 8) | B[7];
		return true;
	}
	else if (Byte == MessagePackFormat::Int8)
	{
		uint8 Val;
		if (!ReadByte(InBytes, Val))
			return false;
		OutValue = static_cast<int8>(Val);
		return true;
	}
	else if (Byte == MessagePackFormat::Int16)
	{
		uint8 High, Low;
		if (!ReadByte(InBytes, High) || !ReadByte(InBytes, Low))
			return false;
		OutValue = static_cast<int16>((High << 8) | Low);
		return true;
	}
	else if (Byte == MessagePackFormat::Int32)
	{
		uint8 B1, B2, B3, B4;
		if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
			return false;
		OutValue = static_cast<int32>((B1 << 24) | (B2 << 16) | (B3 << 8) | B4);
		return true;
	}
	else if (Byte == MessagePackFormat::Int64)
	{
		uint8 B[8];
		for (int i = 0; i < 8; i++)
		{
			if (!ReadByte(InBytes, B[i]))
				return false;
		}
		OutValue = (static_cast<int64>(B[0]) << 56) | (static_cast<int64>(B[1]) << 48) |
		           (static_cast<int64>(B[2]) << 40) | (static_cast<int64>(B[3]) << 32) |
		           (static_cast<int64>(B[4]) << 24) | (static_cast<int64>(B[5]) << 16) |
		           (static_cast<int64>(B[6]) << 8) | B[7];
		return true;
	}
	
	UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid int64 byte: 0x%02X"), Byte);
	return false;
}

bool FPacketDeserializer::ReadString(const TArray<uint8>& InBytes, FString& OutValue)
{
	uint8 Byte;
	if (!ReadByte(InBytes, Byte))
		return false;
	
	int32 Length = 0;
	
	if ((Byte & 0xe0) == MessagePackFormat::FixStrMask)
	{
		// FixStr: 0xa0 - 0xbf
		Length = Byte & 0x1f;
	}
	else if (Byte == MessagePackFormat::Str8)
	{
		uint8 Len;
		if (!ReadByte(InBytes, Len))
			return false;
		Length = Len;
	}
	else if (Byte == MessagePackFormat::Str16)
	{
		uint8 High, Low;
		if (!ReadByte(InBytes, High) || !ReadByte(InBytes, Low))
			return false;
		Length = (High << 8) | Low;
	}
	else if (Byte == MessagePackFormat::Str32)
	{
		uint8 B1, B2, B3, B4;
		if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
			return false;
		Length = (B1 << 24) | (B2 << 16) | (B3 << 8) | B4;
	}
	else
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid string header byte: 0x%02X"), Byte);
		return false;
	}
	
	// Read string bytes
	TArray<uint8> StrBytes;
	StrBytes.SetNum(Length);
	for (int32 i = 0; i < Length; i++)
	{
		if (!ReadByte(InBytes, StrBytes[i]))
			return false;
	}
	
	// Convert from UTF-8 to FString
	FUTF8ToTCHAR Converter(reinterpret_cast<const ANSICHAR*>(StrBytes.GetData()), Length);
	OutValue = FString(Converter.Length(), Converter.Get());
	return true;
}

bool FPacketDeserializer::ReadFloat(const TArray<uint8>& InBytes, float& OutValue)
{
	uint8 Byte;
	if (!ReadByte(InBytes, Byte))
		return false;
	
	if (Byte == MessagePackFormat::Float32)
	{
		uint8 B1, B2, B3, B4;
		if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
			return false;
		
		uint32 IntValue = (static_cast<uint32>(B1) << 24) | (B2 << 16) | (B3 << 8) | B4;
		OutValue = *reinterpret_cast<float*>(&IntValue);
		return true;
	}
	
	UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid float byte: 0x%02X"), Byte);
	return false;
}

bool FPacketDeserializer::ReadBool(const TArray<uint8>& InBytes, bool& OutValue)
{
	uint8 Byte;
	if (!ReadByte(InBytes, Byte))
		return false;
	
	if (Byte == MessagePackFormat::True)
	{
		OutValue = true;
		return true;
	}
	else if (Byte == MessagePackFormat::False)
	{
		OutValue = false;
		return true;
	}
	
	UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid bool byte: 0x%02X"), Byte);
	return false;
}

bool FPacketDeserializer::ReadVector(const TArray<uint8>& InBytes, FVector& OutValue)
{
	// Vector is serialized as [X, Y, Z]
	int32 ArraySize;
	if (!ReadArrayHeader(InBytes, ArraySize) || ArraySize != 3)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid Vector array size: %d"), ArraySize);
		return false;
	}
	
	float X, Y, Z;
	if (!ReadFloat(InBytes, X) || !ReadFloat(InBytes, Y) || !ReadFloat(InBytes, Z))
		return false;
	
	OutValue = FVector(X, Y, Z);
	return true;
}

bool FPacketDeserializer::ReadRotator(const TArray<uint8>& InBytes, FRotator& OutValue)
{
	// Rotator is serialized as [Pitch, Yaw, Roll]
	int32 ArraySize;
	if (!ReadArrayHeader(InBytes, ArraySize) || ArraySize != 3)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid Rotator array size: %d"), ArraySize);
		return false;
	}
	
	float Pitch, Yaw, Roll;
	if (!ReadFloat(InBytes, Pitch) || !ReadFloat(InBytes, Yaw) || !ReadFloat(InBytes, Roll))
		return false;
	
	OutValue = FRotator(Pitch, Yaw, Roll);
	return true;
}

bool FPacketDeserializer::Deserialize(const TArray<uint8>& InBytes, int32& OutPacketType)
{
	ResetReadPosition();
	
	// Read outer array header (should be 2: [UnionKey, FieldArray])
	int32 OuterArraySize;
	if (!ReadArrayHeader(InBytes, OuterArraySize) || OuterArraySize != 2)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid packet format - expected array of 2, got %d"), OuterArraySize);
		return false;
	}
	
	// Read union key (packet type)
	if (!ReadInt(InBytes, OutPacketType))
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Failed to read packet type"));
		return false;
	}
	
	return true;
}

bool FPacketDeserializer::DeserializeLoginResponse(const TArray<uint8>& InBytes, FLoginResponse& OutPacket)
{
	ResetReadPosition();
	
	int32 PacketType;
	if (!Deserialize(InBytes, PacketType) || PacketType != 1)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Expected LoginResponse (1), got packet type %d"), PacketType);
		return false;
	}
	
	// Read field array header
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount != 4)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: LoginResponse expected 4 fields, got %d"), FieldCount);
		return false;
	}
	
	// Read fields in order: Success, Message, AccountId, SessionToken
	if (!ReadBool(InBytes, OutPacket.Success))
		return false;
	if (!ReadString(InBytes, OutPacket.Message))
		return false;
	if (!ReadInt64(InBytes, OutPacket.AccountId))
		return false;
	if (!ReadString(InBytes, OutPacket.SessionToken))
		return false;
	
	UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: Deserialized LoginResponse - Success: %s, Message: %s, AccountId: %lld"),
		OutPacket.Success ? TEXT("true") : TEXT("false"), *OutPacket.Message, OutPacket.AccountId);
	
	return true;
}

bool FPacketDeserializer::DeserializeCharacterListResponse(const TArray<uint8>& InBytes, FCharacterListResponse& OutPacket)
{
	ResetReadPosition();
	
	int32 PacketType;
	if (!Deserialize(InBytes, PacketType) || PacketType != 3)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Expected CharacterListResponse (3), got packet type %d"), PacketType);
		return false;
	}
	
	// Read field array header (1 field: Characters array)
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount != 1)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: CharacterListResponse expected 1 field, got %d"), FieldCount);
		return false;
	}
	
	// Read Characters array
	int32 CharacterCount;
	if (!ReadArrayHeader(InBytes, CharacterCount))
		return false;
	
	OutPacket.Characters.Empty();
	for (int32 i = 0; i < CharacterCount; i++)
	{
		FCharacterInfo CharInfo;
		
		// Each character is an array of fields
		int32 CharFieldCount;
		if (!ReadArrayHeader(InBytes, CharFieldCount) || CharFieldCount < 5)
		{
			UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid character field count: %d"), CharFieldCount);
			return false;
		}
		
		// Read character fields: CharacterId, Name, Race, Class, Level (minimum)
		if (!ReadInt64(InBytes, CharInfo.CharacterId))
			return false;
		if (!ReadString(InBytes, CharInfo.Name))
			return false;
		
		int32 RaceInt, ClassInt;
		if (!ReadInt(InBytes, RaceInt))
			return false;
		CharInfo.Race = static_cast<ERace>(RaceInt);
		
		if (!ReadInt(InBytes, ClassInt))
			return false;
		CharInfo.Class = static_cast<EClass>(ClassInt);
		
		if (!ReadInt(InBytes, CharInfo.Level))
			return false;
		
		OutPacket.Characters.Add(CharInfo);
	}
	
	UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: Deserialized CharacterListResponse - %d characters"), CharacterCount);
	
	return true;
}

bool FPacketDeserializer::DeserializeCreateCharacterResponse(const TArray<uint8>& InBytes, FCreateCharacterResponse& OutPacket)
{
	ResetReadPosition();
	
	int32 PacketType;
	if (!Deserialize(InBytes, PacketType) || PacketType != 5)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Expected CreateCharacterResponse (5), got packet type %d"), PacketType);
		return false;
	}
	
	// Read field array header (3 fields: Success, Message, CharacterId)
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount != 3)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: CreateCharacterResponse expected 3 fields, got %d"), FieldCount);
		return false;
	}
	
	if (!ReadBool(InBytes, OutPacket.Success))
		return false;
	if (!ReadString(InBytes, OutPacket.Message))
		return false;
	if (!ReadInt64(InBytes, OutPacket.CharacterId))
		return false;
	
	UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: Deserialized CreateCharacterResponse - Success: %s, CharacterId: %lld"),
		OutPacket.Success ? TEXT("true") : TEXT("false"), OutPacket.CharacterId);
	
	return true;
}

bool FPacketDeserializer::DeserializeSelectCharacterResponse(const TArray<uint8>& InBytes, FSelectCharacterResponse& OutPacket)
{
	ResetReadPosition();
	
	int32 PacketType;
	if (!Deserialize(InBytes, PacketType) || PacketType != 7)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Expected SelectCharacterResponse (7), got packet type %d"), PacketType);
		return false;
	}
	
	// Read field array header (2 fields: Success, Message)
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount != 2)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: SelectCharacterResponse expected 2 fields, got %d"), FieldCount);
		return false;
	}
	
	if (!ReadBool(InBytes, OutPacket.Success))
		return false;
	if (!ReadString(InBytes, OutPacket.Message))
		return false;
	
	UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: Deserialized SelectCharacterResponse - Success: %s, Message: %s"),
		OutPacket.Success ? TEXT("true") : TEXT("false"), *OutPacket.Message);
	
	return true;
}

bool FPacketDeserializer::DeserializeMovementUpdateResponse(const TArray<uint8>& InBytes, FMovementUpdateResponse& OutPacket)
{
	ResetReadPosition();
	
	int32 PacketType;
	if (!Deserialize(InBytes, PacketType) || PacketType != 11)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Expected MovementUpdateResponse (11), got packet type %d"), PacketType);
		return false;
	}
	
	// Read field array header (4 fields: EntityId, Position, Rotation, Velocity)
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount != 4)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: MovementUpdateResponse expected 4 fields, got %d"), FieldCount);
		return false;
	}
	
	if (!ReadInt64(InBytes, OutPacket.EntityId))
		return false;
	if (!ReadVector(InBytes, OutPacket.Position))
		return false;
	if (!ReadRotator(InBytes, OutPacket.Rotation))
		return false;
	if (!ReadVector(InBytes, OutPacket.Velocity))
		return false;
	
	UE_LOG(LogTemp, Verbose, TEXT("PacketDeserializer: Deserialized MovementUpdateResponse - EntityId: %lld, Position: %s"),
		OutPacket.EntityId, *OutPacket.Position.ToString());
	
	return true;
}
