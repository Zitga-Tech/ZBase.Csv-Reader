#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;
	using System.Collections.Generic;

	// Currently not implemented. To be decided: to be or not to be =)
	internal class JsonSerializer : IObscuredFilePrefsSerializer
	{
		private static JsonSerializer cachedInstance;
		
		public static IObscuredFilePrefsSerializer GetSerializer()
		{
			return cachedInstance ?? (cachedInstance = new JsonSerializer());
		}
		
		public ObscuredPrefsData SerializeStorageDataType<T>(T value)
		{
			throw new NotImplementedException();
		}

		public T DeserializeStorageDataType<T>(ObscuredPrefsData data)
		{
			throw new NotImplementedException();
		}

		public byte[] SerializePrefsDictionary(Dictionary<string, ObscuredPrefsData> dictionary)
		{
			throw new NotImplementedException();
		}

		public Dictionary<string, ObscuredPrefsData> DeserializePrefsDictionary(byte[] data)
		{
			throw new NotImplementedException();
		}
	}
}