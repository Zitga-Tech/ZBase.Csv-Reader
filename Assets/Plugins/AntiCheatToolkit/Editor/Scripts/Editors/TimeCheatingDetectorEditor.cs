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

	[CustomEditor(typeof(TimeCheatingDetector))]
	internal class TimeCheatingDetectorEditor : KeepAliveBehaviourEditor<TimeCheatingDetector>
	{
#if !ACTK_PREVENT_INTERNET_PERMISSION
		private SerializedProperty requestUrl;
		private SerializedProperty requestMethod;
		private SerializedProperty timeoutSeconds;
		private SerializedProperty interval;
		private SerializedProperty realCheatThreshold;
		private SerializedProperty wrongTimeThreshold;
		private SerializedProperty ignoreSetCorrectTime;

		protected override void FindUniqueDetectorProperties()
		{
			requestUrl = serializedObject.FindProperty("requestUrl");
			requestMethod = serializedObject.FindProperty("requestMethod");
			timeoutSeconds = serializedObject.FindProperty("timeoutSeconds");
			interval = serializedObject.FindProperty("interval");
			realCheatThreshold = serializedObject.FindProperty("realCheatThreshold");
			wrongTimeThreshold = serializedObject.FindProperty("wrongTimeThreshold");
			ignoreSetCorrectTime = serializedObject.FindProperty("ignoreSetCorrectTime");
		}

		protected override bool DrawUniqueDetectorProperties()
		{
			DrawHeader("Specific settings");

			EditorGUIUtility.labelWidth += 10;
			EditorGUILayout.PropertyField(ignoreSetCorrectTime);
			EditorGUIUtility.labelWidth -= 10;

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(requestUrl, new GUIContent("URL", requestUrl.tooltip));
			if (EditorGUI.EndChangeCheck())
			{
				self.RequestUrl = requestUrl.stringValue;
			}

#if UNITY_WEBGL
			GUILayout.Label("<b>To avoid CORS limitations while running in WebGL, URL will be changed to the current domain, if it does points to any other domain</b>", GUITools.RichMiniLabel);
			EditorGUILayout.Space();
#endif

			EditorGUILayout.PropertyField(requestMethod, new GUIContent("Method", requestMethod.tooltip));

			EditorGUILayout.PropertyField(timeoutSeconds);
			EditorGUILayout.PropertyField(interval);
			EditorGUILayout.PropertyField(realCheatThreshold);
			EditorGUILayout.PropertyField(wrongTimeThreshold);

			return true;
		}
#else
		protected override bool DrawUniqueDetectorProperties()
		{
			GUILayout.Label("<b>Detector disabled with ACTK_PREVENT_INTERNET_PERMISSION conditional symbol</b>", GUITools.RichLabel);
			return true;
		}
#endif
	}
}