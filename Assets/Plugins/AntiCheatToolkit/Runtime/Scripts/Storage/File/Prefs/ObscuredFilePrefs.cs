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
	using System;
	using System.Collections.Generic;
	using Common;
	using UnityEngine;

	/// <summary>
	/// ObscuredPrefs analogue but uses File IO instead of PlayerPrefs as a backend, has more flexibility and can work from a background thread.
	/// </summary>
	/// Don't forget to call Save() when you wish to save prefs file. You can call it from background thread.
	/// It also will save automatically on non-abnormal application quit (relies on <a href="https://docs.unity3d.com/ScriptReference/Application-wantsToQuit.html">Application.wantsToQuit</a> API).<br/>
	/// Please call DeviceIdHolder.ForceLockToDeviceInit before accessing this class from background thread if you are using Device Lock feature without custom DeviceID.
	public static partial class ObscuredFilePrefs
	{
		/// <summary>
		/// Filename used by default, if other name or path was not specified in constructor.
		/// </summary>
		public const string DefaultFileName = "actkfileprefs";
		
		private const string LogPrefix = ACTk.LogPrefix + nameof(ObscuredFilePrefs) + ": ";
		
		/// <summary>
		/// Fires when saved data tampering detected. Will not fire when data is damaged and not readable.
		/// </summary>
		public static event Action NotGenuineDataDetected;
		
		/// <summary>
		/// Fires when saved data from some other device detected.
		/// </summary>
		/// May be helpful to ban potential cheaters, trying to use someone's purchased in-app goods for example.<br/>
		/// <strong>\htmlonly<font color="7030A0">NOTE:</font>\endhtmlonly Will fire if same device ID has
		/// changed (pretty rare case though). Read more at #DeviceLockLevel.</strong>
		public static event Action DataFromAnotherDeviceDetected;

		private static ObscuredFile prefsFile;
		private static Dictionary<string, ObscuredPrefsData> prefsCache;
		private static readonly object BusyLock = new object();

		/// <summary>
		/// Allows checking current settings.
		/// </summary>
		/// Use #Init() to set the initial settings.
		/// \sa #Init()
		public static IObscuredFileSettings CurrentSettings => PrefsFile.CurrentSettings;
		
		/// <summary>
		/// Allows checking if #Init() was called previously.
		/// </summary>
		public static bool IsInited { get; private set; }

		/// <summary>
		/// Allows checking if prefs cache was loaded \ initialized.
		/// It can be true while IsExists is false if new prefs was not saved yet.
		/// </summary>
		public static bool IsLoaded => prefsCache != null;
		
		/// <summary>
		/// Returns true if prefs file physically exists on disk. File may not exist until Save() is called.
		/// </summary>
		public static bool IsExists => PrefsFile.FileExists;
		
		/// <summary>
		/// Returns true if prefs file has unsaved changes.
		/// </summary>
		public static bool IsSaved { get; private set; }

		/// <summary>
		/// Returns true if prefs file is busy with long-running process such as loading or saving.
		/// </summary>
		public static bool IsBusy { get; private set; }

		/// <summary>
		/// Returns path to the prefs file.
		/// It's always not empty and valid even if prefs was not saved to the physical file yet.
		/// </summary>
		public static string FilePath => PrefsFile.FilePath;

		/// <summary>
		/// Contains reference to the last underlying ObscuredFile read operation result.
		/// </summary>
		/// Filled on prefs file read.<br/>
		/// May be invalid if no read operations were executed.
		/// Check ObscuredFileReadResult.IsValid property to figure this out.
		public static ObscuredFileReadResult LastFileReadResult { get; private set; }
		
		/// <summary>
		/// Contains reference to the last underlying ObscuredFile write operation result. 
		/// </summary>
		/// Filled on prefs file saving.<br/>
		/// May be invalid if no write operations were executed yet.
		/// Check ObscuredFileWriteResult.IsValid property to figure this out.
		public static ObscuredFileWriteResult LastFileWriteResult { get; private set; }

		// to be exposed in future
		private static SerializationSettings SerializationSettings { get; set; }
		
		private static ObscuredFile PrefsFile
		{
			get
			{
				if (IsInited)
					return prefsFile;

				Debug.LogError($"{LogPrefix}Please call {nameof(Init)} first!");
				return null;
			}
		}

#if UNITY_2019_2_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset()
		{
			IsInited = false;
			IsSaved = false;
			IsBusy = false;

			prefsFile = null;
			prefsCache?.Clear();
			prefsCache = null;
			NotGenuineDataDetected = null;
			DataFromAnotherDeviceDetected = null;
		}
#endif
		
		
		/// <summary>
		/// Initializes %ObscuredFilePrefs with file name set to #DefaultFileName and default ObscuredFileSettings.
		/// </summary>
		/// <param name="loadPrefs">Pass <c>true</c> to automatically call LoadPrefs().
		/// This may block calling thread, consider using asynchronously for large files.</param>
		public static void Init(bool loadPrefs = false)
		{
			Init(DefaultFileName, new ObscuredFileSettings(), loadPrefs);
		}
		
		/// <summary>
		/// Initializes %ObscuredFilePrefs with specified file name and default ObscuredFileSettings.
		/// </summary>
		/// <param name="fileName">Custom file name to place at ObscuredFileLocation.PersistentData.</param>
		/// <param name="loadPrefs">Pass <c>true</c> to automatically call LoadPrefs().
		/// This may block calling thread, consider using asynchronously for large files.</param>
		public static void Init(string fileName, bool loadPrefs)
		{
			Init(fileName, new ObscuredFileSettings(), loadPrefs);
		}

		/// <summary>
		/// Initializes %ObscuredFilePrefs with file name set to #DefaultFileName and custom specific settings.
		/// </summary>
		/// <param name="settings">Specific custom settings.</param>
		/// <param name="loadPrefs">Pass <c>true</c> to automatically call LoadPrefs().
		/// This may block calling thread, consider using asynchronously for large files.</param>
		public static void Init(IObscuredFileSettings settings, bool loadPrefs)
		{
			Init(DefaultFileName, settings, loadPrefs);
		}

		/// <summary>
		/// Initializes %ObscuredFilePrefs with specified file name or file path and custom specific settings.
		/// </summary>
		/// <param name="fileNameOrPath">File path if using ObscuredFileLocation.Custom, otherwise represents file name to use with set #ObscuredFileLocation kind.</param>
		/// <param name="settings">Specific custom settings.</param>
		/// <param name="loadPrefs">Pass <c>true</c> to automatically call LoadPrefs().
		/// This may block calling thread, consider using asynchronously for large files.</param>
		public static void Init(string fileNameOrPath, IObscuredFileSettings settings, bool loadPrefs)
		{
			if (IsInited)
			{
				Debug.LogWarning($"{LogPrefix}Already initialized!");
				return;
			}
			
			prefsFile = InitPrefsFile(fileNameOrPath, settings);
			
			if (settings.AutoSave)
				ObscuredFilePrefsAutoSaver.Init();
			
			IsInited = true;
			
			// for future use, currently using only custom binary serialization
			SerializationSettings = new SerializationSettings();

			if (loadPrefs)
				LoadPrefs();
		}
		
		/// <summary>
		/// Releases internal prefs cache, unsubscribes events and frees other used resources.
		/// </summary>
		/// Please call Init() again if you wish to re-use it.
		public static void UnInit()
		{
			IsSaved = false;
			IsInited = false;
			prefsCache?.Clear();
			prefsCache = null;
			prefsFile = null;

			NotGenuineDataDetected = null;
			DataFromAnotherDeviceDetected = null;
		}

		/// <summary>
		/// Loads prefs from existing file if it wasn't loaded before.
		/// </summary>
		/// This function will read from disk potentially causing a hiccup especially when
		/// you have lots of data in your prefs, therefore it is not recommended to call it
		/// synchronously during actual game play.
		/// Instead, call it from separate thread asynchronously or at loading screens and
		/// other stall moments of your app. 
		public static void LoadPrefs()
		{
			try
			{
				if (prefsCache != null) // already loaded
					return;

				lock (BusyLock)
				{
					if (IsBusy)
					{
						Debug.LogWarning($"{LogPrefix}Couldn't load prefs: I'm already busy.");
						return;
					}

					IsBusy = true;

					prefsCache = LoadAndDeserializePrefs();
				}
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Couldn't load prefs!", e);
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// <summary>
		/// Unloads cached prefs from memory. Optionally saves current prefs to the file before unloading.
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly Unsaved data will be lost!</strong>
		public static void UnloadPrefs(bool saveBeforeUnloading = true)
		{
			if (saveBeforeUnloading) 
				Save();
			
			prefsCache?.Clear();
			prefsCache = null;
		}
		
		/// <summary>
		/// Returns true if <c>key</c> exists in the %ObscuredFilePrefs.
		/// </summary>
		/// Calls LoadPrefs() internally.
		public static bool HasKey(string key)
		{
			LoadPrefs();
			return prefsCache.ContainsKey(key);
		}
		
		/// <summary>
		/// Returns all existing prefs keys in current %ObscuredFilePrefs.
		/// </summary>
		/// Calls LoadPrefs() internally.
		public static ICollection<string> GetKeys()
		{
			LoadPrefs();
			return prefsCache.Keys;
		}
		
		/// <summary>
		/// Removes <c>key</c> and its corresponding value from the %ObscuredFilePrefs.
		/// </summary>
		/// Calls LoadPrefs() internally.
		public static void DeleteKey(string key)
		{
			if (HasKey(key))
				prefsCache.Remove(key);
		}

		/// <summary>
		/// <strong>Use with caution!</strong> Removes all keys and values from the prefs.
		/// </summary>
		public static void DeleteAll()
		{
			if (!IsInited)
			{
				Debug.LogError($"{LogPrefix}Please call {nameof(Init)} first!");
				return;
			}
			
			prefsCache?.Clear();
			prefsCache = null;
			
			PrefsFile.Delete();
		}

		/// <summary>
		/// Writes all modified prefs to underlying ObscuredFile on disk.
		/// </summary>
		/// By default, prefs are saved to disk on Application Quit
		/// (relies on <a href="https://docs.unity3d.com/ScriptReference/Application-wantsToQuit.html">Application.wantsToQuit</a> API).<br/>
		/// In case when the app crashes or otherwise prematurely exits, you might
		/// want to write the prefs at sensible 'checkpoints' in your app.<br/>
		/// This function will write to disk potentially causing a hiccup especially
		/// when you have lots of data in your prefs, therefore it is not recommended
		/// to call it synchronously during actual game play.
		/// Instead, call it from separate thread asynchronously or at loading screens
		/// and other stall moments of your app.
		/// <returns> True if save was successful or wasn't needed and false if something went wrong.
		/// Check LastFileWriteResult for details if this method returns false. </returns>
		public static bool Save()
		{
			if (IsSaved)
				return true;

			if (prefsCache == null || prefsCache.Count == 0)
				return true;

			try
			{
				lock (BusyLock)
				{
					if (IsBusy)
					{
						Debug.LogWarning($"{LogPrefix}Couldn't save prefs: I'm already busy.");
						return false;
					}

					IsBusy = true;
					var result = SerializeAndSavePrefs();

					if (!result.Success)
						Debug.LogError($"{LogPrefix}Couldn't save prefs! See {nameof(LastFileWriteResult)} for details.");

					return result.Success;
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				IsBusy = false;
			}

			return false;
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
			
			WritePref(key, value);
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
			
			return ReadPref(key, defaultValue);
		}

		private static ObscuredFile InitPrefsFile(string fileNameOrPath, IObscuredFileSettings settings)
		{
			var result = new ObscuredFile(fileNameOrPath, settings);
			result.NotGenuineDataDetected += OnNotGenuineDataDetected;
			result.DataFromAnotherDeviceDetected += OnDataFromAnotherDeviceDetected;
			return result;
		}
		
		private static void OnNotGenuineDataDetected()
		{
			NotGenuineDataDetected?.Invoke();
		}

		private static void OnDataFromAnotherDeviceDetected()
		{
			DataFromAnotherDeviceDetected?.Invoke();
		}
		
		private static Dictionary<string, ObscuredPrefsData> LoadAndDeserializePrefs()
		{
			if (!PrefsFile.FileExists)
				return new Dictionary<string, ObscuredPrefsData>();
				
			LastFileReadResult = ReadAllBytesInternal();
			if (!LastFileReadResult.Success)
			{
				Debug.LogError($"{LogPrefix}Couldn't load prefs! Load operation result:\n" +
							   LastFileReadResult);
				return new Dictionary<string, ObscuredPrefsData>();
			}
			
			var result = PrefsSerialization.DeserializePrefsDictionary(LastFileReadResult.Data, SerializationSettings);
			if (result == null)
			{
				Debug.LogError($"{LogPrefix}Couldn't deserialize prefs!");
				return new Dictionary<string, ObscuredPrefsData>();
			}

			IsSaved = true;
			
			return result;
		}

		private static ObscuredFileWriteResult SerializeAndSavePrefs()
		{
			var data = PrefsSerialization.SerializePrefsDictionary(prefsCache, SerializationSettings);
			LastFileWriteResult = WriteAllBytesInternal(data);
			return LastFileWriteResult;
		}
		
		private static T ReadPref<T>(string key, T defaultValue)
		{
			if (!IsInited)
			{
				Debug.LogError($"{LogPrefix}Please call {nameof(Init)} first!");
				return defaultValue;
			}
			
			T result = defaultValue;
			
			if (HasKey(key))
				result = PrefsSerialization.DeserializeStorageDataType<T>(prefsCache[key], SerializationSettings);

			return result;
		}
		
		private static void WritePref<T>(string key, T value)
		{
			if (!IsInited)
			{
				Debug.LogError($"{LogPrefix}Please call {nameof(Init)} first!");
				return;
			}
			
			var data = PrefsSerialization.SerializeStorageDataType(value, SerializationSettings);

			if (HasKey(key))
			{
				var existingType = prefsCache[key].type;
				var newType = data.type;
				if (existingType != newType)
				{
					Debug.LogWarning($"{LogPrefix}Pref {key} type changed from " +
									 $"{existingType} to {newType}!\n" +
									 "This message is harmless if you know what you're doing.");	
				}
				
				prefsCache[key] = data;
			}
			else
			{
				prefsCache.Add(key, data);
			}
			
			IsSaved = false;
		}

		private static ObscuredFileReadResult ReadAllBytesInternal()
		{
			if (!IsInited)
			{
				Debug.LogError($"{LogPrefix}Please call {nameof(Init)} first!");
				return ObscuredFileReadResult.FromError(ObscuredFileErrorCode.NotInitialized);
			}
			
			return PrefsFile.ReadAllBytes();
		}

		private static ObscuredFileWriteResult WriteAllBytesInternal(byte[] data)
		{
			if (!IsInited)
			{
				Debug.LogError($"{LogPrefix}Please call {nameof(Init)} first!");
				return ObscuredFileWriteResult.FromError(ObscuredFileErrorCode.NotInitialized);
			}
			
			var result = PrefsFile.WriteAllBytes(data);
			IsSaved = result.Success;
			
			return result;
		}
	}
}
#endif