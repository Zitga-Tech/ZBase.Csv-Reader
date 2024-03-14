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

	[CustomEditor(typeof (ObscuredCheatingDetector))]
	internal class ObscuredCheatingDetectorEditor : KeepAliveBehaviourEditor<ObscuredCheatingDetector>
	{
		private SerializedProperty doubleEpsilon;
		private SerializedProperty floatEpsilon;
		private SerializedProperty vector2Epsilon;
		private SerializedProperty vector3Epsilon;
		private SerializedProperty quaternionEpsilon;

		protected override void FindUniqueDetectorProperties()
		{
			doubleEpsilon = serializedObject.FindProperty("doubleEpsilon");
			floatEpsilon = serializedObject.FindProperty("floatEpsilon");
			vector2Epsilon = serializedObject.FindProperty("vector2Epsilon");
			vector3Epsilon = serializedObject.FindProperty("vector3Epsilon");
			quaternionEpsilon = serializedObject.FindProperty("quaternionEpsilon");
		}

		protected override bool DrawUniqueDetectorProperties()
		{
			DrawHeader("Specific settings");

			EditorGUILayout.PropertyField(doubleEpsilon);
			EditorGUILayout.PropertyField(floatEpsilon);
			EditorGUILayout.PropertyField(vector2Epsilon, new GUIContent("Vector2 Epsilon"));
			EditorGUILayout.PropertyField(vector3Epsilon, new GUIContent("Vector3 Epsilon"));
			EditorGUILayout.PropertyField(quaternionEpsilon);

			return true;
		}
	}
}