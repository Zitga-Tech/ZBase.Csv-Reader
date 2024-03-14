#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode.Editors
{
	using Detectors;

	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof (InjectionDetector))]
	internal class InjectionDetectorEditor : KeepAliveBehaviourEditor<InjectionDetector>
	{
		protected override bool DrawUniqueDetectorProperties()
		{
			if (!ACTkSettings.Instance.InjectionDetectorEnabled)
			{
				using (GUITools.Vertical(GUITools.PanelWithBackground))
				{
					EditorGUILayout.Separator();
					EditorGUILayout.HelpBox("Injection Detector support is not enabled! Injection Detector will not work properly",
						MessageType.Error, true);

					using (new GUILayout.HorizontalScope())
					{
						if (GUILayout.Button("Enable Now"))
						{
							ACTkSettings.Instance.InjectionDetectorEnabled = true;
						}
						if (GUILayout.Button("Enable In settings..."))
						{
							ACTkSettings.Show();
						}
					}

					EditorGUILayout.Separator();
				}

				return true;
			}

			if (SettingsUtils.IsIL2CPPEnabled())
			{
				EditorGUILayout.HelpBox("Mono Injections are not possible in IL2CPP, this detector is not needed in IL2CPP builds",
					MessageType.Info, true);

				return true;
			}

			if (!InjectionRoutines.IsTargetPlatformCompatible())
			{
				EditorGUILayout.HelpBox("Injection Detection is only supported in Standalone and Android builds",
					MessageType.Warning, true);

				return true;
			}

			return false;
		}
	}
}