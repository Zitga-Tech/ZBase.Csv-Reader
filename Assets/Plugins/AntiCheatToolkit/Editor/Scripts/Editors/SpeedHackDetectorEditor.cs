#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode.Editors
{
	using Detectors;

	using UnityEditor;

	[CustomEditor(typeof (SpeedHackDetector))]
	internal class SpeedHackDetectorEditor : KeepAliveBehaviourEditor<SpeedHackDetector>
	{
		private SerializedProperty interval;
		private SerializedProperty threshold;
		private SerializedProperty maxFalsePositives;
		private SerializedProperty coolDown;

		protected override void FindUniqueDetectorProperties()
		{
			interval = serializedObject.FindProperty("interval");
			threshold = serializedObject.FindProperty("threshold");
			maxFalsePositives = serializedObject.FindProperty("maxFalsePositives");
			coolDown = serializedObject.FindProperty("coolDown");
		}

		protected override bool DrawUniqueDetectorProperties()
		{
			DrawHeader("Specific settings");

			EditorGUILayout.PropertyField(interval);
			EditorGUILayout.PropertyField(threshold);
			EditorGUILayout.PropertyField(maxFalsePositives);
			EditorGUILayout.PropertyField(coolDown);

			return true;
		}
	}
}