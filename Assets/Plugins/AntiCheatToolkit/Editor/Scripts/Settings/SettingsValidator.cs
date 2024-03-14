#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode.Validation
{
	using Detectors;
	using UnityEditor;

	[InitializeOnLoad]
	internal static class SettingsValidator
	{
		private static bool injectionValidated;
		private static bool wallhackValidated;

		static SettingsValidator()
		{
			EditorApplication.hierarchyChanged += OnHierarchyChanged;
		}

		private static void OnHierarchyChanged()
		{
			if (!injectionValidated && !ACTkSettings.Instance.DisableInjectionDetectorValidation)
			{
				var instance = InjectionDetector.Instance;
				if (instance != null)
				{
					if (InjectionRoutines.IsInjectionPossible())
					{
						if (!ACTkSettings.Instance.InjectionDetectorEnabled)
						{
							var result = EditorUtility.DisplayDialogComplex("Anti-Cheat Toolkit Validation",
								"ACTk noticed you're using Injection Detector but you have build detection support disabled.\n" +
								"Injection Detector needs it enabled in order to work properly.\nWould you like to enable it now?",
								"Yes", "Open Settings", "No, never ask again");

							switch (result)
							{
								case 0:
									ACTkSettings.Instance.InjectionDetectorEnabled = true;
									break;
								case 1:
									ACTkSettings.Show();
									return;
								default:
									ACTkSettings.Instance.DisableInjectionDetectorValidation = true;
									break;
							}
						}
					}
				}
				injectionValidated = true;
			}

			if (!wallhackValidated && 
				(!ACTkSettings.Instance.DisableWallhackDetectorShaderValidation ||
				!ACTkSettings.Instance.DisableWallhackDetectorLinkXmlValidation))
			{
				var instance = WallHackDetector.Instance;
				if (instance != null)
				{
					if (!ACTkSettings.Instance.DisableWallhackDetectorShaderValidation && 
						instance.CheckWireframe && 
						!SettingsGUI.IsWallhackDetectorShaderIncluded())
					{
						var result = EditorUtility.DisplayDialog("Anti-Cheat Toolkit Validation",
							"ACTk noticed you're using Wallhack Detector with Wireframe option enabled but you have no required shader added" +
							" to the Always Included Shaders.\n" +
							"Would you like to exit Play Mode and open Settings to include it now?",
							"Yes", "No, never ask again");

						if (result)
						{
							EditorApplication.isPlaying = false;
							ACTkEditorPrefsSettings.FocusWallhackFoldout();
							ACTkSettings.Show();
							return;
						}
						
						ACTkSettings.Instance.DisableWallhackDetectorShaderValidation = true;
					}
					
					if (!ACTkSettings.Instance.DisableWallhackDetectorLinkXmlValidation &&
						SettingsUtils.IsLinkXmlRequired() && 
						!SettingsUtils.IsLinkXmlEnabled())
					{
						var result = EditorUtility.DisplayDialog("Anti-Cheat Toolkit Validation",
							"ACTk noticed you're using Wallhack Detector while having IL2CPP's " +
							"Strip Engine Code setting enabled which can lead to stripping of components " +
							"required by WallHack Detector causing false positives. " +
							"To prevent such stripping, components should be added to the link.xml so linker could exclude them from stripping.\n" +
							"Would you like to exit Play Mode and open Settings to enable automatic link.xml generation?",
							"Yes", "No, never ask again");

						if (result)
						{
							EditorApplication.isPlaying = false;
							ACTkEditorPrefsSettings.FocusWallhackFoldout();
							ACTkSettings.Show();
							
							return;
						}
						
						ACTkSettings.Instance.DisableWallhackDetectorLinkXmlValidation = true;
					}
				}
				wallhackValidated = true;
			}
		}
	}
}