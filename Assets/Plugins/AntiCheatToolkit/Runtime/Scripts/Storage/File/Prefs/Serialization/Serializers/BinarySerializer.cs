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
	using System.IO;
	using Common;
	using UnityEngine;

	internal class BinarySerializer : IObscuredFilePrefsSerializer
	{
		private const byte Version = 0;
		
		private static BinarySerializer cachedInstance;
		
		public static IObscuredFilePrefsSerializer GetSerializer()
		{
			return cachedInstance ?? (cachedInstance = new BinarySerializer());
		}
		
		public ObscuredPrefsData SerializeStorageDataType<T>(T value)
		{
			return StorageDataConverter.GetPrefsDataFromValue(value);
		}
		
		public T DeserializeStorageDataType<T>(ObscuredPrefsData data)
		{
			return StorageDataConverter.GetValueFromPrefsData<T>(data);
		}

		public byte[] SerializePrefsDictionary(Dictionary<string, ObscuredPrefsData> dictionary)
		{
			using (var ms = new MemoryStream())
			{
				using (var bw = new BinaryWriter(ms))
				{
					// writing binary serialization version for future use
					bw.Write(Version);
					
					// writing length
					bw.Write(dictionary.Count);
					
					foreach (var prefsData in dictionary)
					{
						// writing each pref
						WritePref(bw, prefsData.Key, prefsData.Value);
					}
				}

				return ms.ToArray();
			}
		}

		public Dictionary<string, ObscuredPrefsData> DeserializePrefsDictionary(byte[] data)
		{
			using (var ms = new MemoryStream(data))
			{
				using (var br = new BinaryReader(ms))
				{
					// reading version, for future use
					var version = br.ReadByte();
					if (version != Version)
					{
						Debug.LogError($"{ACTk.LogPrefix}Incorrect {nameof(BinarySerializer)} version at the prefs dictionary data!" +
									   "Can't read data, something is wrong, please report.");
						return null;
					}

					// getting dictionary elements count
					var count = br.ReadInt32();
					var result = new Dictionary<string, ObscuredPrefsData>(count);

					for (var i = 0; i < count; i++)
					{
						// reading each pref accordingly to write format
						var (key, value) = ReadPref(br);
						result.Add(key, value);
					}

					return result;
				}
			}
		}
		
		private void WritePref(BinaryWriter writer, string key, ObscuredPrefsData value)
		{
			writer.Write(key);
			writer.Write((byte)value.type);
			writer.Write(value.data.Length);
			writer.Write(value.data);
		}

		private (string key, ObscuredPrefsData value) ReadPref(BinaryReader reader)
		{
			var key = reader.ReadString();
			var type = (StorageDataType)reader.ReadByte();
			var dataLength = reader.ReadInt32();
			var data = reader.ReadBytes(dataLength);

			return (key, new ObscuredPrefsData(type, data));
		}
	}
}

#endif