namespace CodeStage.AntiCheat.EditorCode
{
	using System;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using Common;
	using ObscuredTypes;
	using Storage;
	using UnityEditor;
	using UnityEngine;
	using Utils;

	[Serializable]
	internal class PrefsRecord
	{
		protected const string DefaultString = "[^_; = ElinaKristinaMyGirlsLoveYou'16 = ;_^]";
		protected const float DefaultFloat = float.MinValue + 2016.0122f;
		protected const int DefaultInt = int.MinValue + 20130524;
		protected const string CantDecrypt = "Can't decrypt with specified key";

		internal PrefsType prefType = PrefsType.Unknown;
		internal StorageDataType obscuredType = StorageDataType.Unknown;

		internal bool dirtyKey;
		internal bool dirtyValue;

		[SerializeField]
		private string savedKey;

		[SerializeField]
		private string key;

		internal string Key
		{
			get => key;
			set
			{
				if (value == key) return;
				key = value;

				dirtyKey = true;
			}
		}

		[SerializeField]
		private string stringValue;

		internal string StringValue
		{
			get => stringValue;
			set
			{
				if (value == stringValue) return;

				stringValue = value;
				dirtyValue = true;
			}
		}

		[SerializeField]
		private int intValue;

		internal int IntValue
		{
			get => intValue;
			set
			{
				if (value == intValue) return;

				intValue = value;
				dirtyValue = true;
			}
		}

		[SerializeField]
		private float floatValue;

		internal float FloatValue
		{
			get => floatValue;
			set
			{
				if (Math.Abs(value - floatValue) < 0.0000001f) return;

				floatValue = value;
				dirtyValue = true;
			}
		}

		internal string DisplayValue
		{
			get
			{
				switch (prefType)
				{
					case PrefsType.Unknown:
						return ACTkPrefsEditor.UnknownValueDescription;
					case PrefsType.String:
						return IsEditableObscuredValue() || !Obscured ? stringValue : ACTkPrefsEditor.UnsupportedValueDescription;
					case PrefsType.Int:
						return intValue.ToString();
					case PrefsType.Float:
						return floatValue.ToString(CultureInfo.InvariantCulture);
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		internal string DisplayType => Obscured ? obscuredType.ToString() : prefType.ToString();

		internal static int SortByNameAscending(PrefsRecord n1, PrefsRecord n2)
		{
			return string.CompareOrdinal(n1.key, n2.key);
		}

		internal static int SortByNameDescending(PrefsRecord n1, PrefsRecord n2)
		{
			var result = string.CompareOrdinal(n2.key, n1.key);
			return result;
		}

		internal static int SortByType(PrefsRecord n1, PrefsRecord n2)
		{
			var result = string.CompareOrdinal(n1.DisplayType, n2.DisplayType);
			return result == 0 ? SortByNameAscending(n1, n2) : result;
		}

		internal static int SortByObscurance(PrefsRecord n1, PrefsRecord n2)
		{
			var result = n1.Obscured.CompareTo(n2.Obscured);
			return result == 0 ? SortByNameAscending(n1, n2) : result;
		}

		internal bool Obscured { get; set; }

		internal PrefsRecord(string newKey, string value, bool encrypted)
		{
			key = savedKey = newKey;
			stringValue = value;

			prefType = PrefsType.String;

			if (encrypted)
			{
				obscuredType = StorageDataType.String;
				Obscured = true;
			}
		}

		internal PrefsRecord(string newKey, int value, bool encrypted)
		{
			key = savedKey = newKey;
			intValue = value;

			if (encrypted)
			{
				prefType = PrefsType.String;
				obscuredType = StorageDataType.Int32;
				Obscured = true;
			}
			else
			{
				prefType = PrefsType.Int;
			}
		}

		internal PrefsRecord(string newKey, float value, bool encrypted)
		{
			key = savedKey = newKey;
			floatValue = value;

			if (encrypted)
			{
				prefType = PrefsType.String;
				obscuredType = StorageDataType.Single;
				Obscured = true;
			}
			else
			{
				prefType = PrefsType.Float;
			}
		}

		internal PrefsRecord(string originalKey)
		{
			key = savedKey = originalKey;

			ReadValue();

			// only string prefs may be obscured
			if (prefType == PrefsType.String)
			{
				Obscured = IsValueObscured(stringValue);

				if (Obscured)
				{
					key = DecryptKey(key);

					if (obscuredType == StorageDataType.String)
					{
						stringValue = ObscuredPrefs.DecryptValue(key, null, DefaultString, stringValue);
						if (stringValue == DefaultString) stringValue = CantDecrypt;
					}
					else if (obscuredType == StorageDataType.Int32)
					{
						intValue = ObscuredPrefs.DecryptValue(key, null, DefaultInt, stringValue);
						if (intValue == DefaultInt)
						{
							obscuredType = StorageDataType.String;
							stringValue = CantDecrypt;
						}
					}
					else if (obscuredType == StorageDataType.Single)
					{
						floatValue = ObscuredPrefs.DecryptValue(key, null, DefaultFloat, stringValue);
						if (Math.Abs(floatValue - DefaultFloat) < 0.00001f)
						{
							obscuredType = StorageDataType.String;
							stringValue = CantDecrypt;
						}
					}
				}
			}
		}

		internal bool Save(bool newRecord = false)
		{
			var savedString = stringValue;
			string newSavedKey;

			if (Obscured)
			{
				savedString = GetEncryptedValue();
				newSavedKey = GetEncryptedKey();
			}
			else
			{
				newSavedKey = key;
			}

			if (newSavedKey != savedKey && PlayerPrefs.HasKey(newSavedKey))
			{
				if (!EditorUtility.DisplayDialog("Pref overwrite",
					"Pref with name " + key + " already exists!\n" + "Are you sure you wish to overwrite it?", "Yes",
					"No"))
				{
					return false;
				}
			}

			if (dirtyKey)
			{
				PlayerPrefs.DeleteKey(savedKey);
			}

			switch (prefType)
			{
				case PrefsType.Unknown:
					Debug.LogError(ACTk.LogPrefix + "Can't save Pref of unknown type!");
					break;
				case PrefsType.String:
					PlayerPrefs.SetString(newSavedKey, savedString);
					break;
				case PrefsType.Int:
					PlayerPrefs.SetInt(newSavedKey, intValue);
					break;
				case PrefsType.Float:
					PlayerPrefs.SetFloat(newSavedKey, floatValue);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			savedKey = newSavedKey;

			dirtyKey = false;
			dirtyValue = false;

			PlayerPrefs.Save();

			return true;
		}

		internal void Delete()
		{
			PlayerPrefs.DeleteKey(savedKey);
			PlayerPrefs.Save();
		}

		internal void Encrypt()
		{
			if (Obscured) return;

			var success = true;

			switch (prefType)
			{
				case PrefsType.Unknown:
					success = false;
					Debug.LogError(ACTk.LogPrefix + "Can't encrypt pref of unknown type!");
					break;
				case PrefsType.String:
					obscuredType = StorageDataType.String;
					break;
				case PrefsType.Int:
					obscuredType = StorageDataType.Int32;
					break;
				case PrefsType.Float:
					obscuredType = StorageDataType.Single;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (success)
			{
				prefType = PrefsType.String;
				Obscured = true;
				dirtyValue = dirtyKey = true;
			}
		}

		internal void Decrypt()
		{
			if (!Obscured) return;
			if (!IsEditableObscuredValue()) return;

			var success = true;

			switch (obscuredType)
			{
				case StorageDataType.Int32:
					prefType = PrefsType.Int;
					break;
				case StorageDataType.String:
					prefType = PrefsType.String;
					break;
				case StorageDataType.Single:
					prefType = PrefsType.Float;
					break;
				case StorageDataType.UInt32:
				case StorageDataType.Double:
				case StorageDataType.Int64:
				case StorageDataType.Boolean:
				case StorageDataType.ByteArray:
				case StorageDataType.Vector2:
				case StorageDataType.Vector3:
				case StorageDataType.Quaternion:
				case StorageDataType.Color:
				case StorageDataType.Color32:
				case StorageDataType.Rect:
					ACTkPrefsEditor.instance.ShowNotification(new GUIContent("Type " + obscuredType + " isn't supported"));
					success = false;
					break;
				case StorageDataType.Unknown:
					ACTkPrefsEditor.instance.ShowNotification(new GUIContent("Can't decrypt " + key));
					success = false;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (success)
			{
				Obscured = false;
				obscuredType = StorageDataType.Unknown;
				dirtyValue = dirtyKey = true;
			}
		}

		internal string GetEncryptedKey()
		{
			return ObscuredPrefs.EncryptKey(key);
		}

		internal string GetEncryptedValue()
		{
			string savedString;

			switch (obscuredType)
			{
				case StorageDataType.Int32:
					savedString = ObscuredPrefs.EncryptValue(key, intValue);
					break;
				case StorageDataType.String:
					savedString = ObscuredPrefs.EncryptValue(key, stringValue);
					break;
				case StorageDataType.Single:
					savedString = ObscuredPrefs.EncryptValue(key, floatValue);
					break;
				default:
					savedString = stringValue;
					break;;
			}

			return savedString;
		}

		internal bool IsEditableObscuredValue()
		{
			return obscuredType == StorageDataType.Int32 || 
				   obscuredType == StorageDataType.String ||
			       obscuredType == StorageDataType.Single;
		}

		internal string ToString(bool raw = false)
		{
			string result;

			if (raw)
			{
				result = "Key: " + GetEncryptedKey() + Environment.NewLine + "Value: " + GetEncryptedValue();
			}
			else
			{
				result = "Key: " + key + Environment.NewLine + "Value: " + DisplayValue;
			}

			return result;
		}

		private void ReadValue()
		{
			var stringTry = PlayerPrefs.GetString(key, DefaultString);
			if (stringTry != DefaultString)
			{
				prefType = PrefsType.String;
				stringValue = stringTry;
				return;
			}

			var floatTry = PlayerPrefs.GetFloat(key, DefaultFloat);
			if (Math.Abs(floatTry - DefaultFloat) > 0.0000001f)
			{
				prefType = PrefsType.Float;
				floatValue = floatTry;
				return;
			}

			var intTry = PlayerPrefs.GetInt(key, DefaultInt);
			if (intTry != DefaultInt)
			{
				prefType = PrefsType.Int;
				intValue = intTry;
			}
		}

		private string DecryptKey(string encryptedKey)
		{
			string decryptedKey;

			try
			{
				var decryptedKeyChars = Base64Utils.FromBase64ToChars(encryptedKey);
				decryptedKey = ObscuredString.Decrypt(decryptedKeyChars, ObscuredPrefs.GetCryptoKey());
			}
			catch
			{
				decryptedKey = string.Empty;
			}

			return decryptedKey;
		}

		private bool IsValueObscured(string value)
		{
			var validBase64String = (value.Length % 4 == 0) &&
			                        Regex.IsMatch(value, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
			if (!validBase64String) return false;

			var dataType = ObscuredPrefs.GetRawValueType(value);
			if (!Enum.IsDefined(typeof(StorageDataType), dataType) || dataType == StorageDataType.Unknown)
			{
				return false;
			}

			obscuredType = dataType;

			return true;
		}

		internal enum PrefsType : byte
		{
			Unknown,
			String,
			Int,
			Float
		}
	}
}