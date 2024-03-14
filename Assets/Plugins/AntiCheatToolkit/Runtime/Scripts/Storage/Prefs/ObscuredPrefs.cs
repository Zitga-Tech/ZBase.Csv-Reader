#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using Common;
	using Utils;

	using System;
	using ObscuredTypes;
	using UnityEngine;

	/// <summary>
	/// This is an Obscured analogue of the <a href="http://docs.unity3d.com/Documentation/ScriptReference/PlayerPrefs.html">PlayerPrefs</a> class.
	/// </summary>
	/// Saves data in encrypted state, optionally locking it to the current device.<br/>
	/// Automatically encrypts PlayerPrefs on first read (auto migration), has tampering detection and more.<br/>
	/// Check out ObscuredFilePrefs if you wish to save big data amounts.
	public static partial class ObscuredPrefs
	{
		// JFF: MD2 for "ElonShotMarsWithACar" (yes, MD2, not MD5)
		internal const string PrefsKey = "9978e9f39c218d674463dab9dc728bd6";
		private const string RawNotFound = "{not_found}";
		private const string LogPrefix = ACTk.LogPrefix + "ObscuredPrefs: ";
		private const byte Version = 4;

		private static bool alterationReported;
		private static bool foreignSavesReported;

		private static string cryptoKeyObsolete = "e806f6";
		private static string cryptoKeyObsoleteForMigration;

		[Obsolete("Custom crypto key is now obsolete, use only for data recovery from prefs saved with previous version. " +
		          "This property will be removed in future versions.")]
		public static string CryptoKey
		{
			set => cryptoKeyObsolete = value;
			get => cryptoKeyObsolete;
		}

		[Obsolete("Please use DeviceIdHolder.DeviceId instead.", false)]
		public static string DeviceId
		{
			get => DeviceIdHolder.DeviceId;
			set => DeviceIdHolder.DeviceId = value;
		}

		private static DeviceIdHolder deviceIdHolder;

		private static DeviceIdHolder DeviceIdHolder
		{
			get
			{
				if (deviceIdHolder == null)
				{
					deviceIdHolder = new DeviceIdHolder();
					deviceIdHolder.SetHashCheckSumModifierDelegate(DeviceIdHashModifier);
				}

				return deviceIdHolder;
			}
		}
		
		[Obsolete("Please use NotGenuineDataDetected event instead.", false)]
		public static event Action OnAlterationDetected;
		
		[Obsolete("Please use DataFromAnotherDeviceDetected event instead.", false)]
		public static event Action OnPossibleForeignSavesDetected;
		
		/// <summary>
		/// Allows reacting on saves alteration. May be helpful for banning potential cheaters.
		/// </summary>
		/// Fires only once.
		public static event Action NotGenuineDataDetected;

		/// <summary>
		/// Allows reacting on detection of possible saves from some other device.
		/// </summary>
		/// May be helpful to ban potential cheaters, trying to use someone's purchased in-app goods for example.<br/>
		/// May fire on same device in case cheater manipulates saved data in some special way.<br/>
		/// Fires only once.
		///
		/// <strong>\htmlonly<font color="7030A0">NOTE:</font>\endhtmlonly May be called if same device ID was changed (pretty rare case though).</strong>
		public static event Action DataFromAnotherDeviceDetected;

		/// <summary>
		/// Allows saving original PlayerPrefs values while migrating to ObscuredPrefs.
		/// </summary>
		/// In such case, original value still will be readable after switching from PlayerPrefs to
		/// ObscuredPrefs and it should be removed manually as it became unneeded.<br/>
		/// Original PlayerPrefs value will be automatically removed after read by default.
		public static bool preservePlayerPrefs = false;

		/// <summary>
		/// Controls DeviceLock feature settings. Read more at #DeviceLockSettings docs.
		/// </summary>
		public static DeviceLockSettings DeviceLockSettings { get; } = new DeviceLockSettings();
	
		[Obsolete("Please use DeviceLockSettings.DeviceLockLevel property instead.", false)]
		public static DeviceLockLevel lockToDevice
		{
			get => DeviceLockSettings.Level;
			set => DeviceLockSettings.Level = value;
		}

		[Obsolete("Please use DeviceLockSettings.DeviceLockTamperingSensitivity property instead.", false)]
		public static bool readForeignSaves
		{
			get => DeviceLockSettings.Sensitivity <= DeviceLockTamperingSensitivity.Low;

			set => DeviceLockSettings.Sensitivity = value ? DeviceLockTamperingSensitivity.Low : DeviceLockTamperingSensitivity.Normal;
		}

		[Obsolete("Please use DeviceLockSettings.DeviceLockTamperingSensitivity property instead.", false)]
		public static bool emergencyMode
		{
			get => DeviceLockSettings.Sensitivity <= DeviceLockTamperingSensitivity.Disabled;
			set => DeviceLockSettings.Sensitivity = value ? DeviceLockTamperingSensitivity.Disabled : DeviceLockTamperingSensitivity.Normal;
		}

		[Obsolete("Please use DeviceIdHolder.ForceLockToDeviceInit() instead.", false)]
		public static void ForceLockToDeviceInit()
		{
			DeviceIdHolder.ForceLockToDeviceInit();
		}

		/// <summary>
		/// Allows to set the raw encrypted key and value.
		/// </summary>
		public static void SetRawValue(string encryptedKey, string encryptedValue)
		{
			SetStringPref(encryptedKey, encryptedValue);
		}

		/// <summary>
		/// Allows to get the raw encrypted key and value for the specified key.
		/// </summary>
		/// <returns>True if key was found and false otherwise.</returns>
		public static bool GetRawValue(string key, out string encryptedKey, out string encryptedValue)
		{
			encryptedValue = null;
			encryptedKey = EncryptKey(key);

			if (!PlayerPrefs.HasKey(encryptedKey))
			{
				return false;
			}

			encryptedValue = PlayerPrefs.GetString(encryptedKey);
			return true;
		}

		/// <summary>
		/// Returns true if <c>key</c> exists in the ObscuredPrefs or in regular PlayerPrefs.
		/// </summary>
		public static bool HasKey(string key)
		{
			return PlayerPrefs.HasKey(key) || PlayerPrefs.HasKey(EncryptKey(key)) || PlayerPrefs.HasKey(EncryptKeyWithACTkV1Algorithm(key));
		}

		/// <summary>
		/// Removes <c>key</c> and its corresponding value from the ObscuredPrefs and regular PlayerPrefs.
		/// </summary>
		public static void DeleteKey(string key)
		{
			PlayerPrefs.DeleteKey(EncryptKey(key));
			if (!preservePlayerPrefs) PlayerPrefs.DeleteKey(key);
		}

		/// <summary>
		/// Removes saved crypto key. Use only when you wish to completely remove all obscured prefs!
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly Any existing obscured prefs will be lost after this action.</strong>
		public static void DeleteCryptoKey()
		{
			PlayerPrefs.DeleteKey(PrefsKey);

			generatedCryptoKey = null;
			DeviceIdHolder.ResetHash();
		}

		/// <summary>
		/// Removes all keys and values from the preferences, including anything saved with regular PlayerPrefs. <strong>Use with caution!</strong>
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly Please use this method to remove all prefs instead of PlayerPrefs.DeleteAll() to properly clear internals and avoid any data loss when saving new obscured prefs after DeleteAll() call.</strong>
		public static void DeleteAll()
		{
			PlayerPrefs.DeleteAll();

			generatedCryptoKey = null;
			DeviceIdHolder.ResetHash();
		}

		/// <summary>
		/// Writes all modified preferences to disk.
		/// </summary>
		/// By default, Unity writes preferences to disk on Application Quit.<br/>
		/// In case when the game crashes or otherwise prematurely exits, you might want to write the preferences at sensible 'checkpoints' in your game.<br/>
		/// This function will write to disk potentially causing a small hiccup, therefore it is not recommended to call during actual game play.
		public static void Save()
		{
			try
			{
				PlayerPrefs.Save();
			}
			catch (PlayerPrefsException e)
			{
#if UNITY_WEBGL
				Debug.LogError($"{LogPrefix}Couldn't save PlayerPrefs, looks like WebGL PlayerPrefs size exceeds 1 MB limit!");
#endif
				Debug.LogException(e);
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Couldn't save PlayerPrefs for unknown reason!", e);
			}
		}
		
		/// <summary>
		/// Sets the <c>value</c> of the preference identified by <c>key</c>.
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly Not all types are supported, see ::StorageDataType for list of supported types.</strong>
		public static void Set<T>(string key, T value)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key), $"{LogPrefix}You've passed null as key for type {typeof(T)}!");
			
			if (value == null)
				throw new ArgumentNullException(nameof(value),$"{LogPrefix}You've passed null as value for type {typeof(T)}!");
			
			SetStringPref(EncryptKey(key), EncryptValue(key, value));
		}

		/// <summary>
		/// Returns the value corresponding to <c>key</c> in the preference file if it exists.
		/// If it doesn't exist, it will return <c>defaultValue</c>.
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly Not all types are supported, see ::StorageDataType for list of supported types.</strong>
		public static T Get<T>(string key, T defaultValue = default)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			
			var encryptedKey = EncryptKey(key);

			if (!PlayerPrefs.HasKey(encryptedKey))
			{
				if (PlayerPrefs.HasKey(key))
				{
					return ReadFromRegularPrefs(key, defaultValue);
				}
				MigrateFromACTkV1Internal(key, cryptoKeyObsolete);
			}

			return DecryptValue(key, encryptedKey, defaultValue);
		}

		private static T ReadFromRegularPrefs<T>(string key, T defaultValue)
		{
			T unencrypted = defaultValue;
			
			var type = StorageDataTypeClassifier.GetStorageDataType<T>();
			
			if (type == StorageDataType.Int32)
			{
				unencrypted = (T)(object)PlayerPrefs.GetInt(key, (int)(object)defaultValue);
			}
			else if (type == StorageDataType.Single)
			{
				unencrypted = (T)(object)PlayerPrefs.GetFloat(key, (float)(object)defaultValue);
			}
			else if (type == StorageDataType.String)
			{
				unencrypted = (T)(object)PlayerPrefs.GetString(key, defaultValue as string);
			}
					
			if (!preservePlayerPrefs)
			{
				if (type == StorageDataType.Int32 ||
					type == StorageDataType.Single ||
					type == StorageDataType.String) 
				{
					Set(key, unencrypted);
				}

				PlayerPrefs.DeleteKey(key);
			}
			return unencrypted;
		}

		private static void SetStringPref(string encryptedKey, string encryptedValue)
		{
			try
			{
				PlayerPrefs.SetString(encryptedKey, encryptedValue);
			}
			catch (PlayerPrefsException e)
			{
#if UNITY_WEBGL
				Debug.LogError($"{LogPrefix}Couldn't write PlayerPrefs value, looks like WebGL PlayerPrefs size exceeds 1 MB limit!");
#endif
				Debug.LogException(e);
			}
		}

		/// <summary>
		/// Use to migrate ACTk v1.* prefs to the newer format.
		/// </summary>
		/// <param name="key">Prefs key you wish to migrate.</param>
		/// <param name="cryptoKey">Custom crypto key you used for ObscuredPrefs, if any.
		/// Don't use this argument to utilize default key from ACTk v1.</param>
		/// <returns>True if migration was successful, false otherwise.</returns>
		public static bool MigrateFromACTkV1(string key, string cryptoKey = "e806f6")
		{
			return MigrateFromACTkV1Internal(key, cryptoKey);
		}

		/// <summary>
		/// Use to encrypt ACTkv1's value key for later use with SetRawValue to let it migrate.
		/// </summary>
		/// <param name="key">Prefs key.</param>
		/// <param name="cryptoKey">Crypto key you used with ACTk v1, if any.</param>
		/// <returns>Prefs key, encrypted with old ACTk v1 encryption.</returns>
		public static string EncryptKeyWithACTkV1Algorithm(string key, string cryptoKey = "e806f6")
		{
			return Base64Utils.ToBase64(ObscuredString.EncryptDecryptObsolete(key, cryptoKey));
		}

		private static void SavesTampered()
		{
#pragma warning disable 618
			if ((NotGenuineDataDetected != null || OnAlterationDetected != null) && !alterationReported)
			{
				alterationReported = true;
				NotGenuineDataDetected?.Invoke();
				OnAlterationDetected?.Invoke();
			}
#pragma warning restore 618
		}

		private static void PossibleForeignSavesDetected()
		{
#pragma warning disable 618
			if ((DataFromAnotherDeviceDetected != null || OnPossibleForeignSavesDetected != null) && !foreignSavesReported)
			{
				foreignSavesReported = true;
				DataFromAnotherDeviceDetected?.Invoke();
				OnPossibleForeignSavesDetected?.Invoke();
			}
#pragma warning restore 618
		}
	}
}