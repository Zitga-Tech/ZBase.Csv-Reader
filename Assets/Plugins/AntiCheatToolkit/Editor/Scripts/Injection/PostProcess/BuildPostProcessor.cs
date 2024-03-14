#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode.PostProcessors
{
	using System;
	using System.IO;
	using System.Text;
	using Common;
	using UnityEditor;
	using UnityEditor.Build;
	using UnityEditor.Build.Reporting;
	using UnityEngine;

	internal class BuildPostProcessor : IPreprocessBuildWithReport, IPostBuildPlayerScriptDLLs, IPostprocessBuildWithReport
	{
		int IOrderedCallback.callbackOrder => int.MaxValue - 1;

		public void OnPreprocessBuild(BuildReport report)
		{
			if (!ACTkSettings.Instance.InjectionDetectorEnabled ||
			    !InjectionRoutines.IsInjectionPossible())
			{
				return;
			}

			Prepare();
		}

		public void OnPostBuildPlayerScriptDLLs(BuildReport report)
		{
			if (!ACTkSettings.Instance.InjectionDetectorEnabled ||
			    !InjectionRoutines.IsInjectionPossible())
			{
				return;
			}

			InjectionWhitelistBuilder.GenerateWhitelist();
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			InjectionRoutines.Cleanup();
		}

		public static string[] GetGuessedLibrariesForBuild()
		{
			var stagingAreaFolder = Path.Combine(ACTkEditorConstants.ProjectTempFolder, "StagingArea");
			return EditorTools.FindLibrariesAt(stagingAreaFolder);
		}

		private static void Prepare()
		{
			try
			{
				EditorApplication.LockReloadAssemblies();

				if (!Directory.Exists(InjectionConstants.ResourcesFolder))
				{
					Directory.CreateDirectory(InjectionConstants.ResourcesFolder);
				}

				File.WriteAllText(InjectionConstants.DataFilePath, "please remove me", Encoding.Unicode);
				AssetDatabase.Refresh();
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Injection Detector preparation failed!", e);
			}
			finally
			{
				EditorApplication.UnlockReloadAssemblies();
			}
		}
	}
}