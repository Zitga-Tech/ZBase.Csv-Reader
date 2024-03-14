#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if !UNITY_WEBGL
#define ACTK_ASYNC
#endif

namespace CodeStage.AntiCheat.Storage
{
	using System;
	using Common;
	using UnityEngine;
	using Utils;

	/// <summary>
	/// Used by ObscuredPrefs and ObscuredFile for the device lock feature.
	/// </summary>
	public class DeviceIdHolder
	{
		internal delegate byte[] HashCheckSumModifierDelegate(string input);
		
		private static string deviceId;
		
		/// <summary>
		/// Allows getting current device ID or setting custom device ID to lock data to the device.
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly All data saved with previous device ID will be considered foreign!</strong>
		public static string DeviceId
		{
			get
			{
				if (string.IsNullOrEmpty(deviceId))
				{
					deviceId = GetDeviceId();
				}
				return deviceId;
			}
			
			set => deviceId = value;
		}
		
		/// <summary>
		/// Allows forcing device id obtaining on demand. Otherwise, it will be obtained automatically on first usage.
		/// </summary>
		/// Device id obtaining process may be noticeably slow when called first time on some devices.<br/>
		/// This method allows you to force this process at comfortable time (while splash screen is showing for example).<br/><br/>
		/// Call this if you going to use ObscuredFile or ObscuredFilePrefs from non-main thread while using
		/// Lock To Device feature without custom #DeviceId to avoid exception due to Unity API access
		/// from non-main thread.
		public static void ForceLockToDeviceInit()
		{
			if (string.IsNullOrEmpty(deviceId))
			{
				deviceId = GetDeviceId();
			}
			else
			{
				Debug.LogWarning(ACTk.LogPrefix + "ForceLockToDeviceInit() is called, but device ID is already obtained!");
			}
		}
		
		private static string GetDeviceId()
		{
			var id = string.Empty;
#if !ACTK_PREVENT_READ_PHONE_STATE
			if (string.IsNullOrEmpty(id))
				id = UnityApiResultsHolder.GetDeviceUniqueIdentifier();
#else
			Debug.LogError(ACTk.LogPrefix + "Looks like you forced ACTK_PREVENT_READ_PHONE_STATE flag, but still use LockToDevice feature. It will work incorrectly!");
#endif
			return id;
		}

		private readonly uint deviceIdHashSeed;
		private uint deviceIdHash;
		private HashCheckSumModifierDelegate hashCheckSumModifierDelegate;

		internal DeviceIdHolder(uint deviceIdHashSeed = 0)
		{
			this.deviceIdHashSeed = deviceIdHashSeed;
		}
		
		internal bool MigratingFromACTkV1 { get; private set; }

		internal uint DeviceIdHash
		{
			get
			{
				if (deviceIdHash == 0)
				{
					deviceIdHash = CalculateChecksum(DeviceId);
				}
				return deviceIdHash;
			}
		}

		internal void SetHashCheckSumModifierDelegate(HashCheckSumModifierDelegate method)
		{
			hashCheckSumModifierDelegate = method;
		}
		
		internal void SetMigrationMode(bool enabled)
		{
			MigratingFromACTkV1 = enabled;
			ResetHash(); // to force hash recalculation
		}
		
		internal void ResetHash()
		{
			deviceIdHash = 0;
		}
		
		private uint CalculateChecksum(string input)
		{
			if (string.IsNullOrEmpty(input))
				throw new ArgumentNullException(nameof(input));
			
			byte[] inputBytes;

			if (hashCheckSumModifierDelegate != null)
			{
				inputBytes = hashCheckSumModifierDelegate(input);
			}
			else
			{
				inputBytes = StringUtils.StringToBytes(input);
			}

			var hash = xxHash.CalculateHash(inputBytes, inputBytes.Length, deviceIdHashSeed);
			return hash;
		}
	}
}