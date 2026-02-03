#include "PacketDeserializer.h"
#include "MessagePackFormat.h"

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

bool FPacketDeserializer::PeekByte(const TArray<uint8>& InBytes, uint8& OutByte)
{
	if (ReadPosition >= InBytes.Num())
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Read past end of buffer"));
		return false;
	}
	OutByte = InBytes[ReadPosition]; // Don't increment
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

bool FPacketDeserializer::SkipValue(const TArray<uint8>& InBytes)
{
	uint8 Byte;
	if (!ReadByte(InBytes, Byte))
		return false;
	
	// Positive fixint (0x00 - 0x7f) or negative fixint (0xe0 - 0xff)
	if (Byte <= MessagePackFormat::FixIntMax || Byte >= MessagePackFormat::NegativeFixIntMin)
		return true;
	
	// FixStr (0xa0 - 0xbf)
	if ((Byte & 0xe0) == MessagePackFormat::FixStrMask)
	{
		int32 Length = Byte & 0x1f;
		for (int32 i = 0; i < Length; i++)
		{
			uint8 Dummy;
			if (!ReadByte(InBytes, Dummy))
				return false;
		}
		return true;
	}
	
	// FixArray (0x90 - 0x9f)
	if ((Byte & 0xf0) == MessagePackFormat::FixArrayMask)
	{
		int32 Count = Byte & 0x0f;
		return SkipArray(InBytes, Count);
	}
	
	// FixMap (0x80 - 0x8f) - THIS IS THE 0x80 ERROR FIX!
	if ((Byte & 0xf0) == MessagePackFormat::FixMapMask)
	{
		int32 Count = Byte & 0x0f;
		return SkipMap(InBytes, Count);
	}
	
	// Handle specific types
	switch (Byte)
	{
		case MessagePackFormat::Nil:
		case MessagePackFormat::True:
		case MessagePackFormat::False:
			return true;
		
		case MessagePackFormat::Uint8:
		case MessagePackFormat::Int8:
		{
			uint8 Dummy;
			return ReadByte(InBytes, Dummy);
		}
		
		case MessagePackFormat::Uint16:
		case MessagePackFormat::Int16:
		{
			uint8 High, Low;
			return ReadByte(InBytes, High) && ReadByte(InBytes, Low);
		}
		
		case MessagePackFormat::Uint32:
		case MessagePackFormat::Int32:
		case MessagePackFormat::Float32:
		{
			uint8 B1, B2, B3, B4;
			return ReadByte(InBytes, B1) && ReadByte(InBytes, B2) && ReadByte(InBytes, B3) && ReadByte(InBytes, B4);
		}
		
		case MessagePackFormat::Uint64:
		case MessagePackFormat::Int64:
		case MessagePackFormat::Float64:
		{
			for (int i = 0; i < 8; i++)
			{
				uint8 Dummy;
				if (!ReadByte(InBytes, Dummy))
					return false;
			}
			return true;
		}
		
		case MessagePackFormat::Str8:
		{
			uint8 Len;
			if (!ReadByte(InBytes, Len))
				return false;
			for (int32 i = 0; i < Len; i++)
			{
				uint8 Dummy;
				if (!ReadByte(InBytes, Dummy))
					return false;
			}
			return true;
		}
		
		case MessagePackFormat::Str16:
		{
			uint8 High, Low;
			if (!ReadByte(InBytes, High) || !ReadByte(InBytes, Low))
				return false;
			int32 Len = (High << 8) | Low;
			for (int32 i = 0; i < Len; i++)
			{
				uint8 Dummy;
				if (!ReadByte(InBytes, Dummy))
					return false;
			}
			return true;
		}
		
		case MessagePackFormat::Str32:
		{
			uint8 B1, B2, B3, B4;
			if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
				return false;
			int32 Len = (B1 << 24) | (B2 << 16) | (B3 << 8) | B4;
			for (int32 i = 0; i < Len; i++)
			{
				uint8 Dummy;
				if (!ReadByte(InBytes, Dummy))
					return false;
			}
			return true;
		}
		
		case MessagePackFormat::Array16:
		{
			uint8 High, Low;
			if (!ReadByte(InBytes, High) || !ReadByte(InBytes, Low))
				return false;
			int32 Count = (High << 8) | Low;
			return SkipArray(InBytes, Count);
		}
		
		case MessagePackFormat::Array32:
		{
			uint8 B1, B2, B3, B4;
			if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
				return false;
			int32 Count = (B1 << 24) | (B2 << 16) | (B3 << 8) | B4;
			return SkipArray(InBytes, Count);
		}
		
		case MessagePackFormat::Map16:
		{
			uint8 High, Low;
			if (!ReadByte(InBytes, High) || !ReadByte(InBytes, Low))
				return false;
			int32 Count = (High << 8) | Low;
			return SkipMap(InBytes, Count);
		}
		
		case MessagePackFormat::Map32:
		{
			uint8 B1, B2, B3, B4;
			if (!ReadByte(InBytes, B1) || !ReadByte(InBytes, B2) || !ReadByte(InBytes, B3) || !ReadByte(InBytes, B4))
				return false;
			int32 Count = (B1 << 24) | (B2 << 16) | (B3 << 8) | B4;
			return SkipMap(InBytes, Count);
		}
		
		default:
			UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Cannot skip unknown MessagePack type: 0x%02X"), Byte);
			return false;
	}
}

bool FPacketDeserializer::SkipArray(const TArray<uint8>& InBytes, int32 ArraySize)
{
	for (int32 i = 0; i < ArraySize; i++)
	{
		if (!SkipValue(InBytes))
			return false;
	}
	return true;
}

bool FPacketDeserializer::SkipMap(const TArray<uint8>& InBytes, int32 MapSize)
{
	// Map has key-value pairs, so we need to skip both key and value for each entry
	for (int32 i = 0; i < MapSize; i++)
	{
		// Skip key
		if (!SkipValue(InBytes))
			return false;
		// Skip value
		if (!SkipValue(InBytes))
			return false;
	}
	return true;
}

bool FPacketDeserializer::ReadCharacterData(const TArray<uint8>& InBytes, FCharacterInfo& OutCharacter)
{
	// CharacterData is array of at least 16 fields
	// NOTE: This must match the C# server's CharacterData structure in 
	// Shared/WorldofEldara.Shared/Data/Character/CharacterData.cs
	// We require at least 16 fields but allow more for forward compatibility.
	constexpr int32 MinimumCharacterDataFields = 16;
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount < MinimumCharacterDataFields)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Invalid CharacterData field count: %d (expected at least %d)"), FieldCount, MinimumCharacterDataFields);
		return false;
	}
	
	// Field 0: CharacterId
	if (!ReadInt64(InBytes, OutCharacter.CharacterId))
		return false;
	
	// Field 1: AccountId (skip, we don't need it)
	if (!SkipValue(InBytes))
		return false;
	
	// Field 2: Name
	if (!ReadString(InBytes, OutCharacter.Name))
		return false;
	
	// Field 3: Race
	int32 RaceInt;
	if (!ReadInt(InBytes, RaceInt))
		return false;
	OutCharacter.Race = static_cast<ERace>(RaceInt);
	
	// Field 4: Class
	int32 ClassInt;
	if (!ReadInt(InBytes, ClassInt))
		return false;
	OutCharacter.Class = static_cast<EClass>(ClassInt);
	
	// Field 5: Faction (skip for now)
	if (!SkipValue(InBytes))
		return false;
	
	// Field 6: Level
	if (!ReadInt(InBytes, OutCharacter.Level))
		return false;
	
	// Field 7: ExperiencePoints (skip)
	if (!SkipValue(InBytes))
		return false;
	
	// Field 8: CharacterStats (nested object) - skip entire object
	if (!SkipValue(InBytes))
		return false;
	
	// Field 9: CharacterPosition (nested object) - skip entire object
	if (!SkipValue(InBytes))
		return false;
	
	// Field 10: CharacterAppearance (nested object) - skip entire object
	if (!SkipValue(InBytes))
		return false;
	
	// Field 11: EquipmentSlots (nested object) - skip entire object
	if (!SkipValue(InBytes))
		return false;
	
	// Field 12: FactionStandings (Map) - skip
	if (!SkipValue(InBytes))
		return false;
	
	// Field 13: TotemSpirit (nullable int) - skip for now
	if (!SkipValue(InBytes))
		return false;
	
	// Field 14: CreatedAt (DateTime/timestamp) - skip
	if (!SkipValue(InBytes))
		return false;
	
	// Field 15: LastPlayedAt (DateTime/timestamp) - skip
	if (!SkipValue(InBytes))
		return false;
	
	// Skip any additional fields beyond the minimum (for forward compatibility)
	for (int32 i = MinimumCharacterDataFields; i < FieldCount; i++)
	{
		if (!SkipValue(InBytes))
			return false;
	}
	
	UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: Successfully read CharacterData - ID: %lld, Name: %s, Race: %d, Class: %d, Level: %d"),
		OutCharacter.CharacterId, *OutCharacter.Name, (int32)OutCharacter.Race, (int32)OutCharacter.Class, OutCharacter.Level);
	
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
	
	// Read field array header (5 fields now)
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount != 5)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: LoginResponse expected 5 fields, got %d"), FieldCount);
		return false;
	}
	
	// Read fields in order: Result, Message, AccountId, SessionToken, ServerProtocolVersion
	int32 ResultInt;
	if (!ReadInt(InBytes, ResultInt))
		return false;
	OutPacket.Result = static_cast<EResponseCode>(ResultInt);
	
	if (!ReadString(InBytes, OutPacket.Message))
		return false;
	if (!ReadInt64(InBytes, OutPacket.AccountId))
		return false;
	if (!ReadString(InBytes, OutPacket.SessionToken))
		return false;
	if (!ReadString(InBytes, OutPacket.ServerProtocolVersion))
		return false;
	
	UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: Deserialized LoginResponse - Result: %d, Message: %s, AccountId: %lld, Protocol: %s"),
		static_cast<int32>(OutPacket.Result), *OutPacket.Message, OutPacket.AccountId, *OutPacket.ServerProtocolVersion);
	
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
	
	// Read field array header (2 fields: Result, Characters)
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount != 2)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: CharacterListResponse expected 2 fields, got %d"), FieldCount);
		return false;
	}
	
	// Read Result
	int32 ResultInt;
	if (!ReadInt(InBytes, ResultInt))
		return false;
	OutPacket.Result = static_cast<EResponseCode>(ResultInt);
	
	// Read Characters array
	int32 CharacterCount;
	if (!ReadArrayHeader(InBytes, CharacterCount))
		return false;
	
	OutPacket.Characters.Empty();
	for (int32 i = 0; i < CharacterCount; i++)
	{
		FCharacterInfo CharInfo;
		
		// Each CharacterData from C# server has 16 fields:
		// 0: CharacterId, 1: AccountId, 2: Name, 3: Race, 4: Class, 5: Faction, 6: Level,
		// 7: ExperiencePoints, 8: Stats, 9: Position, 10: Appearance, 11: Equipment,
		// 12: FactionStandings, 13: TotemSpirit, 14: CreatedAt, 15: LastPlayedAt
		// We only need fields: 0, 2, 3, 4, 6
		int32 CharFieldCount;
		if (!ReadArrayHeader(InBytes, CharFieldCount))
		{
			UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Failed to read character field array header"));
			return false;
		}
		
		if (CharFieldCount < 7)
		{
			UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: Character has too few fields: %d (expected at least 7)"), CharFieldCount);
			return false;
		}
		
		// Field 0: CharacterId
		if (!ReadInt64(InBytes, CharInfo.CharacterId))
			return false;
		
		// Field 1: AccountId (skip)
		if (!SkipValue(InBytes))
			return false;
		
		// Field 2: Name
		if (!ReadString(InBytes, CharInfo.Name))
			return false;
		
		// Field 3: Race
		int32 RaceInt;
		if (!ReadInt(InBytes, RaceInt))
			return false;
		CharInfo.Race = static_cast<ERace>(RaceInt);
		
		// Field 4: Class
		int32 ClassInt;
		if (!ReadInt(InBytes, ClassInt))
			return false;
		CharInfo.Class = static_cast<EClass>(ClassInt);
		
		// Field 5: Faction (skip)
		if (!SkipValue(InBytes))
			return false;
		
		// Field 6: Level
		if (!ReadInt(InBytes, CharInfo.Level))
			return false;
		
		// Skip remaining fields (7 through CharFieldCount-1)
		for (int32 FieldIndex = 7; FieldIndex < CharFieldCount; FieldIndex++)
		{
			if (!SkipValue(InBytes))
				return false;
		}
		
		OutPacket.Characters.Add(CharInfo);
	}
	
	UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: Deserialized CharacterListResponse - Result: %d, Characters: %d"), 
		static_cast<int32>(OutPacket.Result), CharacterCount);
	
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
	
	// Read field array header (3 fields: Result, Message, Character)
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount != 3)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: CreateCharacterResponse expected 3 fields, got %d"), FieldCount);
		return false;
	}
	
	// Read Result
	int32 ResultInt;
	if (!ReadInt(InBytes, ResultInt))
		return false;
	OutPacket.Result = static_cast<EResponseCode>(ResultInt);
	
	if (!ReadString(InBytes, OutPacket.Message))
		return false;
	
	// Read Character (may be null/nil on failure)
	uint8 NextByte;
	if (!PeekByte(InBytes, NextByte))
		return false;
	
	if (NextByte == MessagePackFormat::Nil)
	{
		// Character is null - consume the nil byte
		ReadByte(InBytes, NextByte);
		UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: CreateCharacterResponse - Result: %d, Message: %s, Character: null"),
			static_cast<int32>(OutPacket.Result), *OutPacket.Message);
	}
	else
	{
		// Read full CharacterData (array header will be read by ReadCharacterData)
		if (!ReadCharacterData(InBytes, OutPacket.Character))
			return false;
		
		UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: CreateCharacterResponse - Result: %d, CharacterId: %lld, Name: %s"),
			static_cast<int32>(OutPacket.Result), OutPacket.Character.CharacterId, *OutPacket.Character.Name);
	}
	
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
	
	// Read field array header (3 fields: Result, Message, Character)
	int32 FieldCount;
	if (!ReadArrayHeader(InBytes, FieldCount) || FieldCount != 3)
	{
		UE_LOG(LogTemp, Error, TEXT("PacketDeserializer: SelectCharacterResponse expected 3 fields, got %d"), FieldCount);
		return false;
	}
	
	// Read Result
	int32 ResultInt;
	if (!ReadInt(InBytes, ResultInt))
		return false;
	OutPacket.Result = static_cast<EResponseCode>(ResultInt);
	
	if (!ReadString(InBytes, OutPacket.Message))
		return false;
	
	// Read Character (may be null)
	uint8 NextByte;
	if (!PeekByte(InBytes, NextByte))
		return false;
	
	if (NextByte == MessagePackFormat::Nil)
	{
		// Character is null - consume the nil byte
		ReadByte(InBytes, NextByte);
		UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: SelectCharacterResponse - Result: %d, Message: %s, Character: null"),
			static_cast<int32>(OutPacket.Result), *OutPacket.Message);
	}
	else
	{
		// Read full CharacterData (array header will be read by ReadCharacterData)
		if (!ReadCharacterData(InBytes, OutPacket.Character))
			return false;
		
		UE_LOG(LogTemp, Log, TEXT("PacketDeserializer: SelectCharacterResponse - Result: %d, CharacterId: %lld, Name: %s"),
			static_cast<int32>(OutPacket.Result), OutPacket.Character.CharacterId, *OutPacket.Character.Name);
	}
	
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
