#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;
	using ObscuredTypes;
	using UnityEngine;
	using Utils;

	public static partial class ObscuredPrefs
	{
		private static char[] generatedCryptoKey;

		internal static char[] GetCryptoKey(string dynamicSuffix = null, string prefsKey = PrefsKey)
		{
			if (generatedCryptoKey == null)
			{
				var savedKey = PlayerPrefs.GetString(prefsKey);
				if (!string.IsNullOrEmpty(savedKey))
				{
					generatedCryptoKey = Base64Utils.FromBase64ToChars(savedKey);
				}
				else
				{
					generatedCryptoKey = ObscuredString.GenerateKey();
					var b64 = Base64Utils.ToBase64(generatedCryptoKey);
					PlayerPrefs.SetString(prefsKey, b64);
					PlayerPrefs.Save();
				}
			}

			if (string.IsNullOrEmpty(dynamicSuffix))
			{
				return generatedCryptoKey;
			}

			var suffixChars = dynamicSuffix.ToCharArray();
			var result = new char[generatedCryptoKey.Length + suffixChars.Length];
			Buffer.BlockCopy(generatedCryptoKey, 0, result, 0, generatedCryptoKey.Length);
			Buffer.BlockCopy(suffixChars, 0, result, generatedCryptoKey.Length + 1, suffixChars.Length);

			return result;
		}

		internal static string EncryptKey(string key)
		{
			var keyChars = ObscuredString.Encrypt(key.ToCharArray(), GetCryptoKey());
			key = Base64Utils.ToBase64(keyChars);
			return key;
		}

		internal static StorageDataType GetRawValueType(string value)
		{
			var result = StorageDataType.Unknown;
			byte[] inputBytes;

			try
			{
				inputBytes = Convert.FromBase64String(value);
			}
			catch (Exception)
			{
				return result;
			}

			if (inputBytes.Length < 7)
			{
				return result;
			}

			var inputLength = inputBytes.Length;

			result = (StorageDataType)inputBytes[inputLength - 7];

			var version = inputBytes[inputLength - 6];
			if (version > 10)
			{
				result = StorageDataType.Unknown;
			}

			return result;
		}

		internal static string EncryptValue<T>(string key, T value)
		{
			var prefsData = StorageDataConverter.GetPrefsDataFromValue(value);
			return EncryptData(key, prefsData.data, prefsData.type);
		}

		internal static T DecryptValue<T>(string key, string encryptedKey, T defaultValue, string encryptedInput = null)
		{
			if (encryptedInput == null)
				encryptedInput = GetEncryptedPrefsString(key, encryptedKey);

			if (encryptedInput == RawNotFound)
				return defaultValue;

			var cleanBytes = DecryptData(key, encryptedInput);
			if (cleanBytes == null)
				return defaultValue;

			var cleanValue = StorageDataConverter.GetValueFromData<T>(cleanBytes);
			return cleanValue;
		}
		
		private static byte[] DecryptByteArrayValue(string key, string encryptedInput, byte defaultValue, int defaultLength)
		{
			var cleanBytes = DecryptData(key, encryptedInput);
			return cleanBytes ?? ConstructByteArray(defaultValue, defaultLength);
		}
		
		private static string EncryptData(string key, byte[] cleanBytes, StorageDataType type)
		{
			var dataLength = cleanBytes.Length;
			var encryptedBytes = EncryptDecryptBytes(cleanBytes, dataLength, GetCryptoKey(key));

			var dataHash = xxHash.CalculateHash(cleanBytes, dataLength, 0);
			var dataHashBytes = new byte[4]; // replaces BitConverter.GetBytes(hash);
			dataHashBytes[0] = (byte)(dataHash & 0xFF);
			dataHashBytes[1] = (byte)((dataHash >> 8) & 0xFF);
			dataHashBytes[2] = (byte)((dataHash >> 16) & 0xFF);
			dataHashBytes[3] = (byte)((dataHash >> 24) & 0xFF);

			byte[] deviceHashBytes = null;
			int finalBytesLength;
			if (DeviceLockSettings.Level != DeviceLockLevel.None)
			{
				// 4 device id hash + 1 data type + 1 device lock mode + 1 version + 4 data hash
				finalBytesLength = dataLength + 11;
				var deviceHash = DeviceIdHolder.DeviceIdHash;
				deviceHashBytes = new byte[4]; // replaces BitConverter.GetBytes(hash);
				deviceHashBytes[0] = (byte)(deviceHash & 0xFF);
				deviceHashBytes[1] = (byte)((deviceHash >> 8) & 0xFF);
				deviceHashBytes[2] = (byte)((deviceHash >> 16) & 0xFF);
				deviceHashBytes[3] = (byte)((deviceHash >> 24) & 0xFF);
			}
			else
			{
				// 1 data type + 1 device lock mode + 1 version + 4 data hash
				finalBytesLength = dataLength + 7;
			}

			var finalBytes = new byte[finalBytesLength];

			Buffer.BlockCopy(encryptedBytes, 0, finalBytes, 0, dataLength);
			if (deviceHashBytes != null)
			{
				Buffer.BlockCopy(deviceHashBytes, 0, finalBytes, dataLength, 4);
			}

			finalBytes[finalBytesLength - 7] = (byte)type;
			finalBytes[finalBytesLength - 6] = Version;
			finalBytes[finalBytesLength - 5] = (byte)DeviceLockSettings.Level;
			Buffer.BlockCopy(dataHashBytes, 0, finalBytes, finalBytesLength - 4, 4);

			return Convert.ToBase64String(finalBytes);
		}

		private static byte[] DecryptData(string key, string encryptedInput)
		{
			byte[] inputBytes;

			try
			{
				inputBytes = Convert.FromBase64String(encryptedInput);
			}
			catch (Exception)
			{
				SavesTampered();
				return null;
			}

			if (inputBytes.Length <= 0)
			{
				SavesTampered();
				return null;
			}

			var inputLength = inputBytes.Length;

			if (inputLength < 7)
			{
				SavesTampered();
				return null;
			}
			
			var inputLockToDevice = (DeviceLockLevel)inputBytes[inputLength - 5];

			var dataHashBytes = new byte[4];
			Buffer.BlockCopy(inputBytes, inputLength - 4, dataHashBytes, 0, 4);
			var inputDataHash = (uint)(dataHashBytes[0] | dataHashBytes[1] << 8 | dataHashBytes[2] << 16 | dataHashBytes[3] << 24);

			int dataBytesLength;
			uint inputDeviceHash = 0;

			if (inputLockToDevice != DeviceLockLevel.None)
			{
				dataBytesLength = inputLength - 11;
				if (DeviceLockSettings.Level != DeviceLockLevel.None)
				{
					var deviceHashBytes = new byte[4];
					Buffer.BlockCopy(inputBytes, dataBytesLength, deviceHashBytes, 0, 4);
					inputDeviceHash = (uint)(deviceHashBytes[0] | deviceHashBytes[1] << 8 | deviceHashBytes[2] << 16 | deviceHashBytes[3] << 24);
				}
			}
			else
			{
				dataBytesLength = inputLength - 7;
			}

			if (dataBytesLength < 0)
			{
				SavesTampered();
				return null;
			}

			var encryptedBytes = new byte[dataBytesLength];
			Buffer.BlockCopy(inputBytes, 0, encryptedBytes, 0, dataBytesLength);

			byte[] cleanBytes;
			if (!DeviceIdHolder.MigratingFromACTkV1)
			{
				cleanBytes = EncryptDecryptBytes(encryptedBytes, dataBytesLength, GetCryptoKey(key));
			}
			else
			{
				cleanBytes = EncryptDecryptBytesObsolete(encryptedBytes, dataBytesLength, key + cryptoKeyObsoleteForMigration);
			}

			var realDataHash = xxHash.CalculateHash(cleanBytes, dataBytesLength, 0);
			if (realDataHash != inputDataHash)
			{
				SavesTampered();
				return null;
			}

			if (inputDeviceHash == 0 && 
				DeviceLockSettings.Level == DeviceLockLevel.Strict && 
				DeviceLockSettings.Sensitivity == DeviceLockTamperingSensitivity.Normal)
			{
				return null;
			}

			if (inputDeviceHash != 0 && DeviceLockSettings.Sensitivity != DeviceLockTamperingSensitivity.Disabled)
			{
				var realDeviceHash = DeviceIdHolder.DeviceIdHash;
				if (inputDeviceHash != realDeviceHash)
				{
					PossibleForeignSavesDetected();
					if (DeviceLockSettings.Sensitivity == DeviceLockTamperingSensitivity.Normal) return null;
				}
			}
			
			// reserved for future use
			// var type = (StorageDataType)inputBytes[inputLength - 7];

			var inputVersion = inputBytes[inputLength - 6];
			if (inputVersion != Version)
			{
				if (inputVersion == 2 || inputVersion == 3)
				{
					var type = (StorageDataType)inputBytes[inputLength - 7];
					if (type == StorageDataType.Color) // converting Color32 bytes format to new version
					{
						var color32 = StorageDataConverter.BytesToColor32Legacy(cleanBytes);
						cleanBytes = StorageDataConverter.GetPrefsDataFromValue(color32).data;
					}
					
					/*if (string.IsNullOrEmpty(cryptoKeyObsolete))
					{
						Debug.LogError(LogPrefix + "Data encrypted with obsolete version found but CryptoKey is not set! " +
						               "Can't decrypt it without a key.");
						return null;
					}*/
				}
				else if (inputVersion > Version) // impossible thing in normal conditions
				{
					SavesTampered();
					return null;
				}
			}

			return cleanBytes;
		}

		private static byte[] EncryptDecryptBytes(byte[] bytes, int dataLength, char[] key)
		{
			var encryptionKeyLength = key.Length;

			var result = new byte[dataLength];

			for (var i = 0; i < dataLength; i++)
			{
				result[i] = (byte)(bytes[i] ^ key[i % encryptionKeyLength]);
			}

			return result;
		}

		private static string GetEncryptedPrefsString(string key, string encryptedKey)
		{
			var result = PlayerPrefs.GetString(encryptedKey, RawNotFound);

			if (result == RawNotFound)
			{
				if (PlayerPrefs.HasKey(key))
				{
					Debug.LogWarning(LogPrefix + "Are you trying to read regular PlayerPrefs data using ObscuredPrefs (key = " + key + ")?");
				}
				else
				{
					MigrateFromACTkV1Internal(key, cryptoKeyObsolete);
					result = PlayerPrefs.GetString(encryptedKey, RawNotFound);
				}
			}
			return result;
		}

#pragma warning disable 618
		private static bool MigrateFromACTkV1Internal(string key, string cryptoKey)
		{
			var oldPrefsKey = EncryptKeyWithACTkV1Algorithm(key, cryptoKey);
			if (!PlayerPrefs.HasKey(oldPrefsKey))
			{
				return false;
			}

			DeviceIdHolder.SetMigrationMode(true);
			cryptoKeyObsoleteForMigration = cryptoKey;

			var encrypted = PlayerPrefs.GetString(oldPrefsKey);
			var type = GetRawValueType(encrypted);

			switch (type)
			{
				case StorageDataType.Int32:
				{
					var decrypted = DecryptValue(key, oldPrefsKey, 0, encrypted);
					DeviceIdHolder.SetMigrationMode(false);
					SetInt(key, decrypted);
					break;
				}
				case StorageDataType.UInt32:
				{
					var decrypted = DecryptValue(key, oldPrefsKey, 0u, encrypted);
					DeviceIdHolder.SetMigrationMode(false);
					SetUInt(key, decrypted);
					break;
				}
				case StorageDataType.String:
				{
					var decrypted = DecryptValue(key, oldPrefsKey, string.Empty, encrypted);
					DeviceIdHolder.SetMigrationMode(false);
					SetString(key, decrypted);
					break;
				}
				case StorageDataType.Single:
				{
					var decrypted = DecryptValue(key, oldPrefsKey, 0f, encrypted);
					DeviceIdHolder.SetMigrationMode(false);
					SetFloat(key, decrypted);
					break;
				}
				case StorageDataType.Double:
				{
					var decrypted = DecryptValue(key, oldPrefsKey, 0d, encrypted);
					DeviceIdHolder.SetMigrationMode(false);
					SetDouble(key, decrypted);
					break;
				}
				case StorageDataType.Decimal:
				{
					var decrypted = DecryptValue(key, oldPrefsKey, 0m, encrypted);
					DeviceIdHolder.SetMigrationMode(false);
					SetDecimal(key, decrypted);
					break;
				}
				case StorageDataType.Int64:
				{
					var decrypted = DecryptValue(key, oldPrefsKey, 0L, encrypted);
					DeviceIdHolder.SetMigrationMode(false);
					SetLong(key, decrypted);
					break;
				}
				case StorageDataType.UInt64:
				{
					var decrypted = DecryptValue(key, oldPrefsKey, 0ul, encrypted);
					DeviceIdHolder.SetMigrationMode(false);
					SetULong(key, decrypted);
					break;
				}
				case StorageDataType.Boolean:
				{
					var decrypted = DecryptValue(key, oldPrefsKey, false, encrypted);
					DeviceIdHolder.SetMigrationMode(false);
					SetBool(key, decrypted);
					break;
				}
				case StorageDataType.ByteArray:
				{
					var decrypted = DecryptByteArrayValue(key, encrypted, 0, 0);
					DeviceIdHolder.SetMigrationMode(false);
					SetByteArray(key, decrypted);
					break;
				}
				case StorageDataType.Vector2:
				{
					var decrypted = DecryptValue(key, encrypted, Vector2.zero);
					DeviceIdHolder.SetMigrationMode(false);
					SetVector2(key, decrypted);
					break;
				}
				case StorageDataType.Vector3:
				{
					var decrypted = DecryptValue(key, encrypted, Vector3.zero);
					DeviceIdHolder.SetMigrationMode(false);
					SetVector3(key, decrypted);
					break;
				}
				case StorageDataType.Quaternion:
				{
					var decrypted = DecryptValue(key, encrypted, Quaternion.identity);
					DeviceIdHolder.SetMigrationMode(false);
					SetQuaternion(key, decrypted);
					break;
				}
				case StorageDataType.Color: // in ACTk1, Color32 was saved with Color type
				{
					var decrypted = DecryptValue(key, encrypted, new Color32(0, 0, 0, 1));
					DeviceIdHolder.SetMigrationMode(false);
					SetColor(key, decrypted);
					break;
				}
				case StorageDataType.Rect:
				{
					var decrypted = DecryptValue(key, encrypted, new Rect(0,0,0,0));
					DeviceIdHolder.SetMigrationMode(false);
					SetRect(key, decrypted);
					break;
				}
				default:
					Debug.LogWarning(LogPrefix + "Couldn't migrate " + key + " key from ACTk v1 prefs since its type is unknown!");
					return false;
			}

			Debug.Log(LogPrefix + "Obscured pref " + key + " successfully migrated to the newer format.");

			cryptoKeyObsoleteForMigration = null;
			PlayerPrefs.DeleteKey(oldPrefsKey);
			return true;
		}
#pragma warning restore 618

		private static byte[] EncryptDecryptBytesObsolete(byte[] bytes, int dataLength, string key)
		{
			var encryptionKeyLength = key.Length;
			var result = new byte[dataLength];

			for (var i = 0; i < dataLength; i++)
			{
				result[i] = (byte)(bytes[i] ^ key[i % encryptionKeyLength]);
			}

			return result;
		}

		private static byte[] DeviceIdHashModifier(string input)
		{
			if (DeviceIdHolder.MigratingFromACTkV1)
			{
				return StringUtils.StringToBytes(input + cryptoKeyObsoleteForMigration);
			}
			
			return StringUtils.CharsToBytes(GetCryptoKey(input));
		}
	}
}