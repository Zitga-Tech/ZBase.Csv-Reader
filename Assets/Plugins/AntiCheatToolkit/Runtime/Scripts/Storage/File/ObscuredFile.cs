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
	using System.IO;
	using System.Security.Cryptography;
	using Common;
	using UnityEngine;
	
	/// <summary>
	/// Allows saving any binary data into the file either with or without encryption,
	/// with or without device locking and always with integrity check to make sure file is genuine.
	/// </summary>
	/// Uses File IO which may cause main thread hiccups when operating with big amounts of data thus
	/// it's recommended to use it from background thread or at the stall moments of your app
	/// (like loading screens etc).<br/>
	/// Please call DeviceIdHolder.ForceLockToDeviceInit() before accessing this class from background thread if you are using Device Lock feature without custom DeviceID.
	public class ObscuredFile
	{
		private const string LogPrefix = ACTk.LogPrefix + nameof(ObscuredFile) + ": ";

		/// <summary>
		/// Filename used by default, if other name or path was not specified in constructor.
		/// </summary>
		public const string DefaultFileName = "actkfile";

		private static readonly DeviceIdHolder DeviceIdHolder = new DeviceIdHolder(24052013);
		
		/// <summary>
		/// Fires when saved data tampering detected. Will not fire when data is damaged and not readable.
		/// </summary>
		public event Action NotGenuineDataDetected;
		
		/// <summary>
		/// Fires when saved data from some other device detected.
		/// </summary>
		/// May be helpful to ban potential cheaters, trying to use someone's purchased in-app goods for example.<br/>
		/// <strong>\htmlonly<font color="7030A0">NOTE:</font>\endhtmlonly Will fire if same device ID has
		/// changed (pretty rare case though). Read more at #DeviceLockLevel.</strong>
		public event Action DataFromAnotherDeviceDetected;

		/// <summary>
		/// Allows reading current settings.
		/// </summary>
		public IObscuredFileSettings CurrentSettings { get; }
		
		/// <summary>
		/// Returns true if file at #FilePath physically exists on disk.
		/// </summary>
		public bool FileExists => File.Exists(FilePath);
		
		/// <summary>
		/// Returns path to the file.
		/// It's always not empty and valid even if file was not saved to the physical disk yet.
		/// </summary>
		public string FilePath { get; }
		
		[System.Diagnostics.Conditional("ACTK_DEV_LOGS")]
		private static void DevLog(string log)
		{
#if UNITY_EDITOR
			Debug.Log("<b>[DEVLOG]</b> " + LogPrefix + log);
#else
			Debug.Log("[DEVLOG] " + LogPrefix + log);
#endif
		}
		
		/// <summary>
		/// Creates instance with file name set to #DefaultFileName and default ObscuredFileSettings.
		/// </summary>
		public ObscuredFile() : this(DefaultFileName, new ObscuredFileSettings()) { }
		
		/// <summary>
		/// Creates instance with specified file name and default ObscuredFileSettings.
		/// </summary>
		/// <param name="fileName">Custom file name to place at ObscuredFileLocation.PersistentData.</param>
		public ObscuredFile(string fileName) : this(fileName, new ObscuredFileSettings()) { }

		/// <summary>
		/// Creates instance with file name set to #DefaultFileName and custom specific settings.
		/// </summary>
		/// <param name="settings">Custom settings to use with this instance.</param>
		public ObscuredFile(ObscuredFileSettings settings) : this(DefaultFileName, settings) { }
		
		/// <summary>
		/// Creates instance with specified file name or file path and custom specific settings.
		/// </summary>
		/// <param name="fileNameOrPath">File path if using ObscuredFileLocation.Custom, otherwise represents file name to use with set #ObscuredFileLocation kind.</param>
		/// <param name="settings">Custom settings to use with this instance.</param>
		public ObscuredFile(string fileNameOrPath, IObscuredFileSettings settings)
        {
			CurrentSettings = settings;
			FilePath = ConstructFilePath(fileNameOrPath);
        }
		
		/// <summary>
		/// Deletes physical file assigned with this instance. Does nothing if file does not exists.
		/// </summary>
		public void Delete()
		{
			if (!FileExists)
				return;
			
			File.Delete(FilePath);
		}

		/// <summary>
		/// Writes passed <c>data</c> to the physical file on disk.
		/// Consider calling from background thread for the large data amount.
		/// </summary>
		/// <param name="data">Custom byte array to write.</param>
		/// <returns>Operation result.</returns>
		public ObscuredFileWriteResult WriteAllBytes(byte[] data)
		{
			return WriteAllBytesInternal(data);
		}

		/// <summary>
		/// Reads all bytes from the physical file on disk.
		/// Consider calling from background thread for the large data amount.
		/// </summary>
		/// <returns>Operation result structure allowing both to get read data and
		/// figure out possible violations and errors if data could not be read for some reason.</returns>
		public ObscuredFileReadResult ReadAllBytes()
		{
			return ReadAllBytesInternal();
		}

		private ObscuredFileReadResult ReadAllBytesInternal()
		{
			byte[] data;
			var dataIsNotGenuine = false;
			var dataFromAnotherDevice = false;

			try
			{
				if (!File.Exists(FilePath))
				{
					Debug.LogError($"{LogPrefix}File {FilePath} does not exists!");
					return ObscuredFileReadResult.FromError(ObscuredFileErrorCode.FileNotFound);
				}

				using (var output = new MemoryStream())
				{
					uint hash;
					uint actualHash = 0;

					using (var reader = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						// getting saved file hash
						hash = ObscuredFileCrypto.ReadHash(reader);

						// decrypting rest of the file
						ObscuredFileCrypto.Decrypt(reader, output, CurrentSettings.EncryptionSettings);
						
						if (CurrentSettings.ValidateDataIntegrity)
							actualHash = ObscuredFileCrypto.CalculateHash(output);
					}
					
					DevLog("Hashes\n" +
						   $"Hash from file: {hash}\n" +
						   $"Calculated hash: {actualHash}\n" +
						   $"Data integrity validation: {CurrentSettings.ValidateDataIntegrity}");
					
					// read file header and exit if it's damaged
					var header = ReadHeader(output);
					if (!header.IsValid())
					{
						Debug.LogError($"{LogPrefix}Header is invalid. Looks like data is damaged, can't read it properly!");
						return ObscuredFileReadResult.FromError(ObscuredFileErrorCode.FileDamaged);
					}

					if (CurrentSettings.ValidateDataIntegrity)
					{
						// it's important to check hashes after header consistency check
						// to avoid false positives due to real data damage
						dataIsNotGenuine = hash != actualHash;
						if (dataIsNotGenuine)
						{
							NotGenuineDataDetected?.Invoke();
						}
					}
					
					var readVersion = header.Version;
					var readObscurationMode = header.ObscurationMode;
					var savedDeviceIdHash = ObscuredFileCrypto.ReadHash(output);

					DevLog("Read file header:\n" +
						   $"Format version: {readVersion}\n" +
						   $"{nameof(ObscurationMode)}: {readObscurationMode}\n" +
						   $"Device Id hash: {savedDeviceIdHash}");
					
					DevLog("Current encryption settings:\n" +
						   $"{nameof(DeviceLockSettings.Level)}: {CurrentSettings.DeviceLockSettings.Level}\n" +
						   $"{nameof(DeviceLockSettings.Sensitivity)}: {CurrentSettings.DeviceLockSettings.Sensitivity}\n" +
						   $"{nameof(ObscurationMode)}: {CurrentSettings.EncryptionSettings.ObscurationMode}");
					
					if (CurrentSettings.DeviceLockSettings.Level != DeviceLockLevel.None)
					{
						var currentDeviceIdHash = DeviceIdHolder.DeviceIdHash;
						
						DevLog("Device hashes:\n" +
							   $"Device Id hash: {savedDeviceIdHash}\n" +
							   $"Current Device Id hash: {currentDeviceIdHash}");
						
						var savedDataIsLocked = savedDeviceIdHash != 0;
						dataFromAnotherDevice = savedDataIsLocked && savedDeviceIdHash != currentDeviceIdHash;
						
						DevLog($"Data locked to any device: {savedDataIsLocked}");
						DevLog($"Data from another device: {dataFromAnotherDevice}");

						if (savedDataIsLocked)
						{
							if (dataFromAnotherDevice)
							{
								switch (CurrentSettings.DeviceLockSettings.Sensitivity)
								{
									case DeviceLockTamperingSensitivity.Disabled:
										break;
									case DeviceLockTamperingSensitivity.Low:
										DataFromAnotherDeviceDetected?.Invoke();
										break;
									case DeviceLockTamperingSensitivity.Normal:
										DataFromAnotherDeviceDetected?.Invoke();
										return new ObscuredFileReadResult(null, dataIsNotGenuine, true);
									default:
										throw new ArgumentOutOfRangeException();
								}
							}
						}
						else
						{
							if (CurrentSettings.DeviceLockSettings.Level == DeviceLockLevel.Strict)
							{
								Debug.LogError($"{LogPrefix}Can't read data since it is not locked to any device but current {nameof(DeviceLockLevel)} is set to {nameof(DeviceLockLevel.Strict)}.");
								return ObscuredFileReadResult.FromError(ObscuredFileErrorCode.DataIsNotLocked);
							}
						}
					}

					var dataLength = output.Length - output.Position;
					data = new byte[dataLength];
					output.Read(data, 0, (int)dataLength);
					
					DevLog($"Loaded data is genuine: {!dataIsNotGenuine}");
				}
			}
			catch (DeviceUniqueIdentifierException e)
			{
				Debug.LogError($"{LogPrefix}Please make sure to call " +
							   $"{nameof(CodeStage.AntiCheat.Storage.DeviceIdHolder)}.{nameof(DeviceIdHolder.ForceLockToDeviceInit)}() " +
							   $"or {nameof(UnityApiResultsHolder)}.{nameof(UnityApiResultsHolder.InitForAsyncUsage)}(true) " +
							   $"before using {nameof(ObscuredFile)} or {nameof(ObscuredFilePrefs)} from async context " +
							   $"with Lock To Device feature enabled while keeping default " +
							   $"{nameof(CodeStage.AntiCheat.Storage.DeviceIdHolder)}.{nameof(DeviceIdHolder.DeviceId)}!");
				return ObscuredFileReadResult.FromError(e);
			}
			catch (CryptographicException e)
			{
				Debug.LogError($"{LogPrefix}Couldn't read bytes because of {nameof(CryptographicException)}! " +
							   $"Are you trying to read plain file with {nameof(ObscurationMode)}.{nameof(ObscurationMode.Encrypted)} or vice versa?");
				Debug.LogException(e);
				return ObscuredFileReadResult.FromError(e);
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Unexpected exception occured!", LogPrefix, e);
				return ObscuredFileReadResult.FromError(e);
			}

			return new ObscuredFileReadResult(data, dataIsNotGenuine, dataFromAnotherDevice);
		}

		private ObscuredFileHeader ReadHeader(Stream output)
		{
			output.Position = 0;
			
			var result = new ObscuredFileHeader();
			result.ReadFrom(output);
			return result;
		}

		private ObscuredFileWriteResult WriteAllBytesInternal(byte[] data)
		{
			try
			{
				using (var memory = new MemoryStream())
				{
					WriteHeader(memory);
					ApplyDeviceLockIfNeeded(memory, CurrentSettings.DeviceLockSettings.Level);
					memory.Write(data, 0, data.Length);
					using (var writer = File.Open(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						uint hash = 0;
						
						if (CurrentSettings.ValidateDataIntegrity)
							hash = ObscuredFileCrypto.CalculateHash(memory);
						
						DevLog($"Writing file hash: {hash}\n" +
							   $"Data integrity validation: {CurrentSettings.ValidateDataIntegrity}");
						ObscuredFileCrypto.WriteHash(writer, hash);
						ObscuredFileCrypto.Encrypt(memory, writer, CurrentSettings.EncryptionSettings);
					}
				}
			}
			catch (DeviceUniqueIdentifierException e)
			{
				Debug.LogError($"{LogPrefix}Please make sure to call " +
							   $"{nameof(CodeStage.AntiCheat.Storage.DeviceIdHolder)}.{nameof(DeviceIdHolder.ForceLockToDeviceInit)}() " +
							   $"or {nameof(UnityApiResultsHolder)}.{nameof(UnityApiResultsHolder.InitForAsyncUsage)}(true) " +
							   $"before using {nameof(ObscuredFile)} or {nameof(ObscuredFilePrefs)} from async context " +
							   $"with Lock To Device feature enabled while keeping default " +
							   $"{nameof(CodeStage.AntiCheat.Storage.DeviceIdHolder)}.{nameof(DeviceIdHolder.DeviceId)}!");
				return ObscuredFileWriteResult.FromError(e);
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Unexpected exception occured!", LogPrefix, e);
				return ObscuredFileWriteResult.FromError(e);
			}

			return new ObscuredFileWriteResult(ObscuredFileErrorCode.NoError);
		}
		
		private void WriteHeader(Stream memory)
		{
			ObscuredFileHeader.WriteTo(memory, CurrentSettings.EncryptionSettings.ObscurationMode);
		}

		private void ApplyDeviceLockIfNeeded(Stream writer, DeviceLockLevel deviceLockLevel)
		{
			var hash = deviceLockLevel == DeviceLockLevel.None ? 0 : DeviceIdHolder.DeviceIdHash;
			DevLog($"Writing device id hash: {hash}");
			ObscuredFileCrypto.WriteHash(writer, hash);
		}

		private string ConstructFilePath(string fileName)
        {
            string filePath;
            if (!Path.HasExtension(fileName))
	            fileName = Path.ChangeExtension(fileName, "actk");

			try
			{
				switch (CurrentSettings.LocationKind)
				{
					case ObscuredFileLocation.PersistentData:
					{
						filePath = Path.Combine(UnityApiResultsHolder.GetPersistentDataPath(), fileName);
						break;
					}
					case ObscuredFileLocation.Custom:
					{
						filePath = Path.GetFullPath(fileName);

#if UNITY_ANDROID || UNITY_WEBGL
						var streamingPath = Path.GetFullPath(Application.streamingAssetsPath);
						if (filePath.StartsWith(streamingPath, StringComparison.InvariantCulture))
						{
							var log = $"{LogPrefix}Can't access StreamingAssets data at the " +
									  $"{filePath} on " +
									  $"{nameof(RuntimePlatform.Android)} and {nameof(RuntimePlatform.WebGLPlayer)} platforms!\n" +
									  "Consider moving data to the supported location first. " +
									  "See 'Troubleshooting' in User Manual for more details.";
	#if UNITY_EDITOR
							Debug.LogWarning(log);
	#else
							Debug.LogError(log);
	#endif
							
						}
#endif
						
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			catch (PersistentDataPathException)
			{
				Debug.LogError($"{LogPrefix}Please make sure to call " +
							   $"{nameof(UnityApiResultsHolder)}.{nameof(UnityApiResultsHolder.InitForAsyncUsage)}() " +
							   $"before using {nameof(ObscuredFile)} or " +
							   $"{nameof(ObscuredFilePrefs)} from async context with " +
							   $"{nameof(ObscuredFileLocation)}.{nameof(ObscuredFileLocation.PersistentData)} {nameof(ObscuredFileSettings.LocationKind)}!");
				filePath = fileName;
			}

			return filePath;
        }
	}
}
#endif