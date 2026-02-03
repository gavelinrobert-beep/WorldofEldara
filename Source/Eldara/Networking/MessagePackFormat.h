#pragma once

#include "CoreMinimal.h"

/**
 * MessagePack format byte constants
 * Used by both PacketSerializer and PacketDeserializer
 * 
 * Spec: https://github.com/msgpack/msgpack/blob/master/spec.md
 */
namespace MessagePackFormat
{
	// Positive FixInt: 0x00 - 0x7f
	constexpr uint8 FixIntMin = 0x00;
	constexpr uint8 FixIntMax = 0x7f;
	
	// Negative FixInt: 0xe0 - 0xff
	constexpr uint8 NegativeFixIntMin = 0xe0;
	
	// Unsigned Integers
	constexpr uint8 Uint8 = 0xcc;
	constexpr uint8 Uint16 = 0xcd;
	constexpr uint8 Uint32 = 0xce;
	constexpr uint8 Uint64 = 0xcf;
	
	// Signed Integers
	constexpr uint8 Int8 = 0xd0;
	constexpr uint8 Int16 = 0xd1;
	constexpr uint8 Int32 = 0xd2;
	constexpr uint8 Int64 = 0xd3;
	
	// Floating Point
	constexpr uint8 Float32 = 0xca;
	constexpr uint8 Float64 = 0xcb;
	
	// Strings
	constexpr uint8 FixStrMask = 0xa0;  // 0xa0 - 0xbf (0 to 31 bytes)
	constexpr uint8 Str8 = 0xd9;
	constexpr uint8 Str16 = 0xda;
	constexpr uint8 Str32 = 0xdb;
	
	// Arrays
	constexpr uint8 FixArrayMask = 0x90;  // 0x90 - 0x9f (0 to 15 elements)
	constexpr uint8 Array16 = 0xdc;
	constexpr uint8 Array32 = 0xdd;
	
	// Maps
	constexpr uint8 FixMapMask = 0x80;  // 0x80 - 0x8f (0 to 15 key-value pairs)
	constexpr uint8 Map16 = 0xde;
	constexpr uint8 Map32 = 0xdf;
	
	// Boolean
	constexpr uint8 False = 0xc2;
	constexpr uint8 True = 0xc3;
	
	// Nil
	constexpr uint8 Nil = 0xc0;
	
	// Extension types (timestamps, custom types)
	constexpr uint8 FixExt1 = 0xd4;
	constexpr uint8 FixExt2 = 0xd5;
	constexpr uint8 FixExt4 = 0xd6;
	constexpr uint8 FixExt8 = 0xd7;  // DateTime/timestamp (8-byte)
	constexpr uint8 FixExt16 = 0xd8;
	constexpr uint8 Ext8 = 0xc7;
	constexpr uint8 Ext16 = 0xc8;
	constexpr uint8 Ext32 = 0xc9;
}
