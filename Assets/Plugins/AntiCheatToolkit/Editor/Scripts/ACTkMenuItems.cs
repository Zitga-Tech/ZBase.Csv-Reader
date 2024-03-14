#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !UNITY_2019_1_OR_NEWER
#define ACTK_UWP_NO_IL2CPP
#endif

namespace CodeStage.AntiCheat.EditorCode
{
	using Detectors;
	using UnityEditor;

	using Common;
	using PostProcessors;
	using UnityEngine;

	internal static class ACTkMenuItems
	{
		// ---------------------------------------------------------------
		//  Main menu items
		// ---------------------------------------------------------------

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Settings...", false, 100)]
		private static void ShowSettingsWindow()
		{
			ACTkSettings.Show();
		}

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Injection Detector Whitelist Editor...", false, 1000)]
		private static void ShowAssembliesWhitelistWindow()
		{
			UserWhitelistEditor.ShowWindow();
		}

#if !ACTK_UWP_NO_IL2CPP
		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Calculate external build hashes", false, 1200)]
		private static void HashExternalBuild()
		{
			var buildHashes = CodeHashGeneratorPostprocessor.CalculateExternalBuildHashes();
			if (buildHashes == null || buildHashes.FileHashes.Length == 0)
			{
				Debug.LogError(ACTk.LogPrefix + "External build hashing was not successful. " +
				               "See previous log messages for possible details.");
				return;
			}

			buildHashes.PrintToConsole();
		}
#endif

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Migrate/Migrate obscured types on prefabs...", false, 1500)]
		private static void MigrateObscuredTypesOnPrefabs()
		{
			MigrateUtils.MigrateObscuredTypesOnPrefabs("ObscuredFloat", "ObscuredDouble", "ObscuredVector2", "ObscuredVector3", "ObscuredQuaternion");
		}

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Migrate/Migrate obscured types in opened scene(s)...", false, 1501)]
		private static void MigrateObscuredTypesInScene()
		{
			MigrateUtils.MigrateObscuredTypesInScene("ObscuredFloat", "ObscuredDouble", "ObscuredVector2", "ObscuredVector3", "ObscuredQuaternion");
		}

		/* will be needed when obsolete string internals will be deprecated along with automatic migration */

		//[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Migrate/Migrate ObscuredString on prefabs...", false, 1600)]
		private static void MigrateObscuredStringOnPrefabs()
		{
			MigrateUtils.MigrateObscuredTypesOnPrefabs("ObscuredString");
		}

		//[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Migrate/Migrate ObscuredString in opened scene(s)...", false, 1601)]
		private static void MigrateObscuredStringInScene()
		{
			MigrateUtils.MigrateObscuredTypesInScene("ObscuredString");
		}

		// ---------------------------------------------------------------
		//  GameObject menu items
		// ---------------------------------------------------------------

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + "All detectors", false, 0)]
		private static void AddAllDetectorsToScene()
		{
			AddInjectionDetectorToScene();
			AddObscuredCheatingDetectorToScene();
			AddSpeedHackDetectorToScene();
			AddWallHackDetectorToScene();
			AddTimeCheatingDetectorToScene();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + InjectionDetector.ComponentName, false, 1)]
		private static void AddInjectionDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<InjectionDetector>();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + ObscuredCheatingDetector.ComponentName, false, 1)]
		private static void AddObscuredCheatingDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<ObscuredCheatingDetector>();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + SpeedHackDetector.ComponentName, false, 1)]
		private static void AddSpeedHackDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<SpeedHackDetector>();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + WallHackDetector.ComponentName, false, 1)]
		private static void AddWallHackDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<WallHackDetector>();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + TimeCheatingDetector.ComponentName, false, 1)]
		private static void AddTimeCheatingDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<TimeCheatingDetector>();
		}
	}
}