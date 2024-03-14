#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Common
{
	using System;
	using UnityEngine;
	
	/// <summary>
	/// Contains ACTk version in case you need to know it in your scripts.
	/// </summary>
	public static class ACTk
	{
		/// <summary>
		/// Current version in X.Y.Z format (using Semantic Versioning 2.0 scheme).
		/// </summary>
		public const string Version = "2021.1.1";
		
#if UNITY_EDITOR
		public const string LogPrefix = "<b>[ACTk]</b> ";
#else
		public const string LogPrefix = "[ACTk] ";
#endif
		
		// used at external integrations, thus it's public
		public const string DocsRootUrl = "https://codestage.net/uas_files/actk/api/";
		internal const string SupportContact = "https://codestage.net/contacts";
		internal static readonly char[] StringKey = {'\x69', '\x108', '\x105', '\x110', '\x97'};

		private static string unityVersion;
		private static string applicationPlatform;

#if UNITY_EDITOR
		private static string buildTarget;
#endif

		// getting this information here because Unity APIs
		// can't be accessed from background threads but this info used at ConstructErrorForSupport
		// which potentially can be called from the background thread
		[RuntimeInitializeOnLoadMethod]
		private static void GetUnityInfo()
		{
			unityVersion = Application.unityVersion;
			applicationPlatform = Application.platform.ToString();
#if UNITY_EDITOR
			buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
#endif
		}
		
		internal static void PrintExceptionForSupport(string errorText, Exception exception = null)
		{
			PrintExceptionForSupport(errorText, null, exception);
		}
		
		internal static void PrintExceptionForSupport(string errorText, string prefix = null, Exception exception = null)
		{
			Debug.LogError(ConstructErrorForSupport(errorText, prefix, exception));
			Debug.LogException(exception);
		}
		
		internal static string ConstructErrorForSupport(string text, string prefix = null, Exception exception = null)
		{
			var prefixLog = string.IsNullOrEmpty(prefix) ? LogPrefix : prefix;
			var logText =  $"{prefixLog}{text}\n" +
						   $"Please report at: {SupportContact}\n"+
						   $"Also please include this information:\n{GenerateBugReport(exception)}";

			return logText;
		}

		internal static string GenerateBugReport(Exception exception = null)
		{
			var result = $"Unity version: {unityVersion}\n" +
						 $"Asset version: {Version}\n" +
						 $"Current platform: {applicationPlatform}";
#if UNITY_EDITOR
			result += $"\nTarget platform: {buildTarget}";
#endif
			
			if (exception != null)
			{
				result += $"\n{exception}";
			}
			
			return result;
		}
	}
}