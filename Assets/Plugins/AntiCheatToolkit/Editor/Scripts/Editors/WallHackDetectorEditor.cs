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

	[CustomEditor(typeof (WallHackDetector))]
	internal class WallHackDetectorEditor : KeepAliveBehaviourEditor<WallHackDetector>
	{
		private SerializedProperty wireframeDelay;
		private SerializedProperty raycastDelay;
		private SerializedProperty spawnPosition;
		private SerializedProperty maxFalsePositives;

		private SerializedProperty checkRigidbody;
		private SerializedProperty checkController;
		private SerializedProperty checkWireframe;
		private SerializedProperty checkRaycast;

		protected override void FindUniqueDetectorProperties()
		{
			raycastDelay = serializedObject.FindProperty("raycastDelay");
			wireframeDelay = serializedObject.FindProperty("wireframeDelay");
			spawnPosition = serializedObject.FindProperty("spawnPosition");
			maxFalsePositives = serializedObject.FindProperty("maxFalsePositives");

			checkRigidbody = serializedObject.FindProperty("checkRigidbody");
			checkController = serializedObject.FindProperty("checkController");
			checkWireframe = serializedObject.FindProperty("checkWireframe");
			checkRaycast = serializedObject.FindProperty("checkRaycast");
		}

		protected override bool DrawUniqueDetectorProperties()
		{
			var detector = self;
			if (detector == null) return false;

			DrawHeader("Specific settings");

			if (PropertyFieldChanged(checkRigidbody, new GUIContent("Rigidbody")))
			{
				detector.CheckRigidbody = checkRigidbody.boolValue;
			}

			if (PropertyFieldChanged(checkController, new GUIContent("Character Controller")))
			{
				detector.CheckController = checkController.boolValue;
			}

			if (PropertyFieldChanged(checkWireframe, new GUIContent("Wireframe")))
			{
				detector.CheckWireframe = checkWireframe.boolValue;
			}
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(wireframeDelay, new GUIContent("Delay"));
			EditorGUI.indentLevel--;

			if (PropertyFieldChanged(checkRaycast, new GUIContent("Raycast")))
			{
				detector.CheckRaycast = checkRaycast.boolValue;
			}
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(raycastDelay, new GUIContent("Delay"));
			EditorGUI.indentLevel--;

			EditorGUILayout.Separator();

			EditorGUILayout.PropertyField(spawnPosition);
			if (Vector3.Distance(spawnPosition.vector3Value, Vector3.zero) <= 0.001f)
			{
				EditorGUILayout.HelpBox("Please consider placing spawn position as far from your moving objects as possible to avoid false positives", MessageType.Warning);
				EditorGUILayout.Space();
			}
			EditorGUILayout.PropertyField(maxFalsePositives);

			EditorGUILayout.Separator();

			if (checkWireframe.boolValue && !SettingsGUI.IsWallhackDetectorShaderIncluded())
			{
				using (GUITools.Vertical(GUITools.PanelWithBackground))
				{
					EditorGUILayout.Separator();
					EditorGUILayout.HelpBox("Wallhack Detector shader for Wireframe checks is not included into the build! Detector may work incorrectly",
						MessageType.Error, true);

					if (GUILayout.Button("Include in Settings..."))
					{
						ACTkSettings.Show();
					}

					EditorGUILayout.Separator();
				}
			}

			if (checkRaycast.boolValue || checkController.boolValue || checkRigidbody.boolValue)
			{
				var layerId = LayerMask.NameToLayer("Ignore Raycast");
				if (Physics.GetIgnoreLayerCollision(layerId, layerId))
				{
					EditorGUILayout.LabelField("IgnoreRaycast physics layer should collide with itself to avoid false positives! See readme's troubleshooting section for details.", EditorStyles.wordWrappedLabel);
					if (GUILayout.Button("Edit in Physics settings"))
					{
						EditorApplication.ExecuteMenuItem("Edit/Project Settings/Physics");
                    }
				}
			}

			return true;
		}

		private static bool PropertyFieldChanged(SerializedProperty property, GUIContent content, params GUILayoutOption[] options)
		{
			var result = false;

			EditorGUI.BeginChangeCheck();

			if (content == null)
			{
				EditorGUILayout.PropertyField(property, options);
			}
			else
			{
				EditorGUILayout.PropertyField(property, content, options);
			}

			if (EditorGUI.EndChangeCheck())
			{
				result = true;
			}
			return result;
		}
	}
}