#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !UNITY_2019_1_OR_NEWER
#define ACTK_UWP_NO_IL2CPP
#endif

#if !ACTK_UWP_NO_IL2CPP

namespace CodeStage.AntiCheat.Storage
{
	using System.Collections.Generic;

	internal static class PrefsSerialization
	{
		public static ObscuredPrefsData SerializeStorageDataType<T>(T value, SerializationSettings settings)
		{
			var serializer = GetSerializer(settings);
			return serializer.SerializeStorageDataType(value);
		}

		public static T DeserializeStorageDataType<T>(ObscuredPrefsData data, SerializationSettings settings)
		{
			var serializer = GetSerializer(settings);
			return serializer.DeserializeStorageDataType<T>(data);
		}

		public static byte[] SerializePrefsDictionary(Dictionary<string, ObscuredPrefsData> value, SerializationSettings settings)
		{
			var serializer = GetSerializer(settings);
			return serializer.SerializePrefsDictionary(value);
		}
		
		public static Dictionary<string, ObscuredPrefsData> DeserializePrefsDictionary(byte[] data, SerializationSettings settings)
		{
			var serializer = GetSerializer(settings);
			return serializer.DeserializePrefsDictionary(data);
		}
		
		private static IObscuredFilePrefsSerializer GetSerializer(SerializationSettings settings)
		{
			return settings.SerializationKind == ACTkSerializationKind.Binary ? 
				BinarySerializer.GetSerializer() : JsonSerializer.GetSerializer();
		}
	}
}
#endif