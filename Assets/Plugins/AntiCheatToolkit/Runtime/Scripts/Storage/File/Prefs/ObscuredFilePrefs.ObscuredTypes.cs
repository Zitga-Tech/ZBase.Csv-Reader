#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using ObscuredTypes;
	using UnityEngine;

	// these overloads allow seamless obscured types processing
	public static partial class ObscuredFilePrefs
	{
		public static void Set(string key, ObscuredBool value) { Set<bool>(key, value); }
		public static void Set(string key, ObscuredByte value) { Set<byte>(key, value); }
		public static void Set(string key, ObscuredChar value) { Set<char>(key, value); }
		public static void Set(string key, ObscuredDecimal value) { Set<decimal>(key, value); }
		public static void Set(string key, ObscuredDouble value) { Set<double>(key, value); }
		public static void Set(string key, ObscuredFloat value) { Set<float>(key, value); }
		public static void Set(string key, ObscuredInt value) { Set<int>(key, value); }
		public static void Set(string key, ObscuredLong value) { Set<long>(key, value); }
		public static void Set(string key, ObscuredQuaternion value) { Set<Quaternion>(key, value); }
		public static void Set(string key, ObscuredSByte value) { Set<sbyte>(key, value); }
		public static void Set(string key, ObscuredShort value) { Set<short>(key, value); }
		public static void Set(string key, ObscuredString value) { Set<string>(key, value); }
		public static void Set(string key, ObscuredUInt value) { Set<uint>(key, value); }
		public static void Set(string key, ObscuredULong value) { Set<ulong>(key, value); }
		public static void Set(string key, ObscuredUShort value) { Set<ushort>(key, value); }
		public static void Set(string key, ObscuredVector2 value) { Set<Vector2>(key, value); }
		public static void Set(string key, ObscuredVector2Int value) { Set<Vector2Int>(key, value); }
		public static void Set(string key, ObscuredVector3 value) { Set<Vector3>(key, value); }
		public static void Set(string key, ObscuredVector3Int value) { Set<Vector3Int>(key, value); }
		
		public static ObscuredBool Get(string key, ObscuredBool defaultValue = default) { return Get<bool>(key, defaultValue); }
		public static ObscuredByte Get(string key, ObscuredByte defaultValue = default) { return Get<byte>(key, defaultValue); }
		public static ObscuredChar Get(string key, ObscuredChar defaultValue = default) { return Get<char>(key, defaultValue); }
		public static ObscuredDecimal Get(string key, ObscuredDecimal defaultValue = default) { return Get<decimal>(key, defaultValue); }
		public static ObscuredDouble Get(string key, ObscuredDouble defaultValue = default) { return Get<double>(key, defaultValue); }
		public static ObscuredFloat Get(string key, ObscuredFloat defaultValue = default) { return Get<float>(key, defaultValue); }
		public static ObscuredInt Get(string key, ObscuredInt defaultValue = default) { return Get<int>(key, defaultValue); }
		public static ObscuredLong Get(string key, ObscuredLong defaultValue = default) { return Get<long>(key, defaultValue); }
		public static ObscuredQuaternion Get(string key, ObscuredQuaternion defaultValue = default) { return Get<Quaternion>(key, defaultValue); }
		public static ObscuredSByte Get(string key, ObscuredSByte defaultValue = default) { return Get<sbyte>(key, defaultValue); }
		public static ObscuredShort Get(string key, ObscuredShort defaultValue = default) { return Get<short>(key, defaultValue); }
		public static ObscuredString Get(string key, ObscuredString defaultValue = default) { return Get<string>(key, defaultValue); }
		public static ObscuredUInt Get(string key, ObscuredUInt defaultValue = default) { return Get<uint>(key, defaultValue); }
		public static ObscuredULong Get(string key, ObscuredULong defaultValue = default) { return Get<ulong>(key, defaultValue); }
		public static ObscuredUShort Get(string key, ObscuredUShort defaultValue = default) { return Get<ushort>(key, defaultValue); }
		public static ObscuredVector2 Get(string key, ObscuredVector2 defaultValue = default) { return Get<Vector2>(key, defaultValue); }
		public static ObscuredVector2Int Get(string key, ObscuredVector2Int defaultValue = default) { return Get<Vector2Int>(key, defaultValue); }
		public static ObscuredVector3 Get(string key, ObscuredVector3 defaultValue = default) { return Get<Vector3>(key, defaultValue); }
		public static ObscuredVector3Int Get(string key, ObscuredVector3Int defaultValue = default) { return Get<Vector3Int>(key, defaultValue); }
	}
}