#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

#if !UNITY_WEBGL
#define ACTK_ASYNC
#endif

namespace CodeStage.AntiCheat.Common
{
	using Storage;
	using UnityEngine;
	using Utils;

	/// <summary>
	/// User-friendly wrapper around few internally used Unity APIs which can't be accessed from background threads.
	/// </summary>
	/// You only need to touch this if you are going to use ObscuredFile / ObscuredFilePrefs from the background threads.
	public static class UnityApiResultsHolder
	{
#if UNITY_IPHONE
		private static string deviceVendorIdentifier;
#endif
		
		private static string deviceUniqueIdentifier;
		private static string persistentDataPath;
		
		private static bool? isMobilePlatform;

		/// <summary>
		/// Call this from the main thread before using ObscuredFile / ObscuredFilePrefs from the background threads.
		/// </summary>
		/// Calling this method avoids getting exceptions while working with ObscuredFile / ObscuredFilePrefs from the
		/// background threads.
		/// <param name="warmUpDeviceIdentifier">Pass true to init API needed for the Lock To Device feature
		/// (SystemInfo.deviceUniqueIdentifier).
		/// You need this to be true only when using ObscuredFile / ObscuredFilePrefs from the background threads
		/// with DeviceLock enabled and without custom DeviceID set.
		/// Passing true is similar to the DeviceIdHolder.ForceLockToDeviceInit() call, please read that API docs for
		/// more information about possible side effects.</param>
		public static void InitForAsyncUsage(bool warmUpDeviceIdentifier)
		{
#if ACTK_ASYNC
			if (System.Threading.SynchronizationContext.Current == null)
			{
				Debug.LogError($"Please call {nameof(InitForAsyncUsage)} from main thread!");
				return;
			}
#endif

			GetPersistentDataPath();

			if (warmUpDeviceIdentifier)
				GetDeviceUniqueIdentifier();
			
			_ = AppEventsDispatcher.Instance;
			IsMobilePlatform();
		}

		internal static string GetDeviceUniqueIdentifier()
		{
			if (string.IsNullOrEmpty(deviceUniqueIdentifier))
			{
#if ACTK_ASYNC
				if (System.Threading.SynchronizationContext.Current != null)
					deviceUniqueIdentifier = GetDeviceID();
				else
					throw new DeviceUniqueIdentifierException();
#else
				deviceUniqueIdentifier = GetDeviceID();
#endif
			}
			
			return deviceUniqueIdentifier;
			
			string GetDeviceID()
			{
				var result = string.Empty;
#if UNITY_IPHONE
				result = UnityEngine.iOS.Device.vendorIdentifier;
#endif
				if (string.IsNullOrEmpty(result))
					result = SystemInfo.deviceUniqueIdentifier;

				return result;
			}
		}

		internal static string GetPersistentDataPath()
		{
			if (string.IsNullOrEmpty(persistentDataPath))
			{
#if ACTK_ASYNC
				if (System.Threading.SynchronizationContext.Current != null)
					persistentDataPath = Application.persistentDataPath;
				else
					throw new PersistentDataPathException();
#else
				persistentDataPath = Application.persistentDataPath;
#endif
			}
			
			return persistentDataPath;
		}
		
#if !ACTK_DISABLE_FILEPREFS_AUTOSAVE
		internal static bool IsMobilePlatform()
		{
			if (isMobilePlatform == null)
			{
#if UNITY_EDITOR
				isMobilePlatform = UnityEditorInternal.InternalEditorUtility.IsMobilePlatform(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
#else
				isMobilePlatform = Application.isMobilePlatform;
#endif
			}

			return isMobilePlatform.Value;
		}
#endif
	}
}