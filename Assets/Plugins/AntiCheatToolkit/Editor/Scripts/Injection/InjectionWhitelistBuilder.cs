#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

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
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using Common;
	using Detectors;
	using ObscuredTypes;
	using PostProcessors;
	using UnityEditor;
	using Debug = UnityEngine.Debug;

#if ACTK_DEBUG
	using System.Diagnostics;
#endif

	internal static class InjectionWhitelistBuilder
	{
		private const string ProgressCaption = "ACTk: Building InjectionDetector Whitelist";

#if ACTK_DEBUG
		private static Stopwatch sw;
#endif

		public static void GenerateWhitelist()
		{
			try
			{
				GenerateWhitelistInternal();
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport($"Something went wrong while building {nameof(InjectionDetector)} whitelist!", e);
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

		private static void GenerateWhitelistInternal()
		{
#if ACTK_DEBUG
			sw = Stopwatch.StartNew();
			sw.Stop();
			Debug.Log("=== Injection Detector Whitelist Build Start ===");
			sw.Start();
#endif
			EditorUtility.DisplayProgressBar(ProgressCaption, "Gathering assemblies", 0);

			var assembliesInBuild = GetAssembliesInBuild();
			if (assembliesInBuild.Length == 0)
			{
				Debug.LogError(ACTk.ConstructErrorForSupport("Can't find any assemblies in build!"));
			}

			var assembliesAllowedByUser = GetUserWhiteListAssemblies();
			var allAllowedAssemblies = InjectionRoutines.MergeAllowedAssemblies(assembliesInBuild, assembliesAllowedByUser);

			EditorUtility.DisplayProgressBar(ProgressCaption, "Writing assemblies hashes", 0);

			WriteAllowedAssemblies(allAllowedAssemblies);

#if ACTK_DEBUG
			sw.Stop();
			Debug.Log(ACTk.LogPrefix + "WhiteList build duration: " + sw.ElapsedMilliseconds + " ms.");
#endif

			AssetDatabase.Refresh();
		}

		private static AllowedAssembly[] GetAssembliesInBuild()
		{
#if ACTK_DEBUG_VERBOSE
			sw.Stop();
			Debug.Log(ACTk.LogPrefix + "Trying to guess which assemblies can get into the build...");
			sw.Start();
#endif
			var libraries = BuildPostProcessor.GetGuessedLibrariesForBuild();

#if ACTK_DEBUG_VERBOSE
			sw.Stop();
			Debug.Log(ACTk.LogPrefix + "Total libraries candidates: " + libraries.Length);
			sw.Start();

			var invalidAssemblies = string.Empty;
#endif

			var result = new List<AllowedAssembly>();

			foreach (var libraryPath in libraries)
			{
#if ACTK_DEBUG_PARANIOD
				sw.Stop();
				Debug.Log(ACTk.LogPrefix + "Checking library at the path: " + libraryPath);
				sw.Start();
#endif
				try
				{
					var assName = AssemblyName.GetAssemblyName(libraryPath);
					var name = assName.Name;
					var hash = InjectionRoutines.GetAssemblyHash(assName);

					var allowed = result.FirstOrDefault(allowedAssembly => allowedAssembly.Name == name);
					if (allowed != null)
					{
						allowed.AddHash(hash);
					}
					else
					{
						allowed = new AllowedAssembly(name, new[] { hash });
						result.Add(allowed);
					}
				}
				catch
				{
					// not a valid IL assembly, skipping
#if ACTK_DEBUG_VERBOSE
					invalidAssemblies += libraryPath + "\n";
#endif
				}
			}

#if ACTK_DEBUG_VERBOSE
			if (!string.IsNullOrEmpty(invalidAssemblies))
			{
				sw.Stop();
				Debug.Log(ACTk.LogPrefix + "Not valid assemblies:\n" + invalidAssemblies);
				sw.Start();
			}
#endif

#if ACTK_DEBUG
			sw.Stop();
			var trace = ACTk.LogPrefix + "Found assemblies in build (" + result.Count + ", " + sw.ElapsedMilliseconds + " ms):\n";

			foreach (var allowedAssembly in result)
			{
				trace += "  Name: " + allowedAssembly.name + "\n";
				trace = allowedAssembly.hashes.Aggregate(trace, (current, hash) => current + ("    Hash: " + hash + "\n"));
			}
			Debug.Log(trace);
			sw.Start();
#endif
			return result.ToArray();
		}

		private static AllowedAssembly[] GetUserWhiteListAssemblies()
		{
			var userWhiteList = ACTkSettings.Instance.InjectionDetectorWhiteList;
#if ACTK_DEBUG
			sw.Stop();
			var trace = ACTk.LogPrefix + "User White List assemblies (" + userWhiteList.Count + "):\n";

			foreach (var allowedAssembly in userWhiteList)
			{
				trace += "  Name: " + allowedAssembly.name + "\n";
				trace = allowedAssembly.hashes.Aggregate(trace, (current, hash) => current + ("    Hash: " + hash + "\n"));
			}
			Debug.Log(trace);
			sw.Start();
#endif
			return userWhiteList.ToArray();
		}

		private static void WriteAllowedAssemblies(List<AllowedAssembly> assemblies)
		{
			Directory.CreateDirectory(InjectionConstants.ResourcesFolder);
			var bw = new BinaryWriter(new FileStream(InjectionConstants.DataFilePath, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.Unicode);

			bw.Write(assemblies.Count);

#if ACTK_DEBUG_VERBOSE
			sw.Stop();
			Debug.Log(ACTk.LogPrefix + "Writing assemblies data, count: " + assemblies.Count);
			sw.Start();
#endif

			foreach (var assembly in assemblies)
			{
				var name = assembly.Name;
				var hashes = "";

				for (var j = 0; j < assembly.Hashes.Length; j++)
				{
					hashes += assembly.Hashes[j].ToString(CultureInfo.InvariantCulture);
					if (j < assembly.Hashes.Length - 1)
					{
						hashes += InjectionConstants.DataSeparator;
					}
				}

				var line = ObscuredString.Encrypt(name + InjectionConstants.DataSeparator + hashes, ACTk.StringKey);

#if ACTK_DEBUG_PARANIOD
				sw.Stop();
				Debug.Log(ACTk.LogPrefix + "Writing assembly:\n" + name + InjectionConstants.DataSeparator + hashes + "\n" +
						  new string(line) + ", length: " + line.Length);
				sw.Start();
#endif
				bw.Write(line.Length);
				bw.Write(line, 0, line.Length);
			}

			bw.Close();
		}
	}
}