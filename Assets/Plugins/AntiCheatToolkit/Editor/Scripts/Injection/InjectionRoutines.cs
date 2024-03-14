#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if (UNITY_STANDALONE || UNITY_ANDROID)
#define UNITY_SUPPORTED_PLATFORM
#endif

#define ACTK_DEBUG
#undef ACTK_DEBUG

#define ACTK_DEBUG_VERBOSE
#undef ACTK_DEBUG_VERBOSE

#define ACTK_DEBUG_PARANIOD
#undef ACTK_DEBUG_PARANIOD

#if ACTK_DEBUG_PARANIOD
#define ACTK_DEBUG
#define ACTK_DEBUG_VERBOSE
#endif

#if ACTK_DEBUG_VERBOSE
#define ACTK_DEBUG
#endif

namespace CodeStage.AntiCheat.EditorCode
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Common;
	using ObscuredTypes;
	using UnityEditor;
	using UnityEngine;

	internal static class InjectionRoutines
	{
		private static bool cleanupDone;

		public static bool IsInjectionPossible()
		{
			return IsTargetPlatformCompatible() && !SettingsUtils.IsIL2CPPEnabled();
		}

		public static bool IsTargetPlatformCompatible()
		{
#if UNITY_SUPPORTED_PLATFORM
			return true;
#else
			return false;
#endif
		}

		public static int GetAssemblyHash(AssemblyName ass)
		{
			var hashInfo = ass.Name;

			var bytes = ass.GetPublicKeyToken();
			if (bytes != null && bytes.Length == 8)
			{
				hashInfo += PublicKeyTokenToString(bytes);
			}

			// Jenkins hash function (http://en.wikipedia.org/wiki/Jenkins_hash_function)
			var result = 0;
			var len = hashInfo.Length;

			for (var i = 0; i < len; ++i)
			{
				result += hashInfo[i];
				result += (result << 10);
				result ^= (result >> 6);
			}
			result += (result << 3);
			result ^= (result >> 11);
			result += (result << 15);

			return result;
		}

		public static List<AllowedAssembly> MergeAllowedAssemblies(IEnumerable<AllowedAssembly> collection1, IEnumerable<AllowedAssembly> collection2)
		{
			var result = new List<AllowedAssembly>(collection1);

			foreach (var whiteListedAssembly in collection2)
			{
				var exists = false;
				foreach (var assembly in result)
				{
					if (assembly.Name != whiteListedAssembly.Name) continue;

					exists = true;
					foreach (var hash in whiteListedAssembly.Hashes)
					{
						if (Array.IndexOf(assembly.Hashes, hash) == -1)
						{
							assembly.AddHash(hash);
						}
					}

					break;
				}

				if (!exists)
				{
					result.Add(whiteListedAssembly);
				}
			}

			return result;
		}

		public static void Cleanup()
		{
			if (cleanupDone) return;
			cleanupDone = true;

			TryMigrateSettings();
			TryMigrateLegacyInjectionWhitelist();

			if (File.Exists(InjectionConstants.DataFilePath))
			{
#if ACTK_DEBUG
				Debug.Log(ACTk.LogPrefix + "Data file found and going to be removed: " + InjectionConstants.DataFilePath);
#endif
				EditorTools.DeleteFile(InjectionConstants.DataFilePath);
				EditorTools.DeleteFile(AssetDatabase.GetTextMetaFilePathFromAssetPath(InjectionConstants.DataFilePath));
				EditorTools.RemoveDirectoryIfEmpty(InjectionConstants.ResourcesFolder);

				AssetDatabase.Refresh();
			}
		}
		
		private static string PublicKeyTokenToString(byte[] bytes)
		{
			var result = "";

			// AssemblyName.GetPublicKeyToken() returns 8 bytes
			for (var i = 0; i < 8; i++)
			{
				result += ACTkEditorConstants.HexTable[bytes[i]];
			}

			return result;
		}

		private static void TryMigrateSettings()
		{
			if (!EditorPrefs.HasKey(InjectionConstants.PrefsKey)) return;

			ACTkSettings.Instance.InjectionDetectorEnabled = EditorPrefs.GetBool(InjectionConstants.PrefsKey);
			EditorPrefs.DeleteKey(InjectionConstants.PrefsKey);
		}

		private static void TryMigrateLegacyInjectionWhitelist()
		{
			if (!File.Exists(InjectionConstants.LegacyWhitelistRelativePath)) return;
			var whitelistedAssemblies = LoadAndParseLegacyWhitelist();
			ACTkSettings.Instance.InjectionDetectorWhiteList = MergeAllowedAssemblies(ACTkSettings.Instance.InjectionDetectorWhiteList, whitelistedAssemblies);
		}

		private static IEnumerable<AllowedAssembly> LoadAndParseLegacyWhitelist()
		{
			var result = new List<AllowedAssembly>();
			string[] separator = { InjectionConstants.DataSeparator };

			var fs = new FileStream(InjectionConstants.LegacyWhitelistRelativePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			var br = new BinaryReader(fs);

			try
			{
				var count = br.ReadInt32();

				for (var i = 0; i < count; i++)
				{
					var line = br.ReadString();
					line = new string(ObscuredString.Encrypt(line, ACTk.StringKey));
					var strArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					var stringsCount = strArr.Length;
					if (stringsCount > 1)
					{
						var assemblyName = strArr[0];

						var hashes = new int[stringsCount - 1];
						for (var j = 1; j < stringsCount; j++)
						{
							var success = int.TryParse(strArr[j], out var parseResult);
							if (success)
							{
								hashes[j - 1] = parseResult;
							}
							else
							{
								Debug.LogError(ACTk.LogPrefix + "Could not parse value: " + strArr[j] +
								               ", line:\n" + line);
							}
						}

						result.Add(new AllowedAssembly(assemblyName, hashes));
					}
					else
					{
						throw new Exception("Error parsing whitelist file line!");
					}
				}
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Error while reading legacy whitelist!", e);
			}
			finally
			{
				br.Close();
				fs.Close();
			}

			return result;
		}
	}
}