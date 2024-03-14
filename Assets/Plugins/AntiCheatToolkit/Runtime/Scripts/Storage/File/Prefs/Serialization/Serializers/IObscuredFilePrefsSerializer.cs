#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System.Collections.Generic;

	internal interface IObscuredFilePrefsSerializer
	{
		ObscuredPrefsData SerializeStorageDataType<T>(T value);
		T DeserializeStorageDataType<T>(ObscuredPrefsData data);
		
		byte[] SerializePrefsDictionary(Dictionary<string, ObscuredPrefsData> dictionary);
		Dictionary<string, ObscuredPrefsData> DeserializePrefsDictionary(byte[] data);
	}
}