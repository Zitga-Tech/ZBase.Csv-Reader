#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode.Editors
{
	using Common;
	using UnityEditor;
	using UnityEngine;

	internal class KeepAliveBehaviourEditor<T> : Editor where T: KeepAliveBehaviour<T>
	{
		protected T self;
		
		private SerializedProperty autoStart;
		private SerializedProperty autoDispose;
		private SerializedProperty keepAlive;
		private SerializedProperty detectionEvent;
		private SerializedProperty detectionEventHasListener;

		public virtual void OnEnable()
		{
			autoStart = serializedObject.FindProperty("autoStart");
			autoDispose = serializedObject.FindProperty("autoDispose");
			keepAlive = serializedObject.FindProperty("keepAlive");
			detectionEvent = serializedObject.FindProperty("detectionEvent");
			detectionEventHasListener = serializedObject.FindProperty("detectionEventHasListener");

			self = (T)target;

			FindUniqueDetectorProperties();
		}

		public override void OnInspectorGUI()
		{
			if (self == null) return;

			serializedObject.Update();

			EditorGUIUtility.labelWidth = 140;
			EditorGUILayout.Space();
			DrawHeader("Base settings");

			EditorGUILayout.PropertyField(autoStart);
			detectionEventHasListener.boolValue = EditorTools.CheckUnityEventHasActivePersistentListener(detectionEvent);

			CheckAdditionalEventsForListeners();

			if (autoStart.boolValue && !detectionEventHasListener.boolValue && !AdditionalEventsHasListeners())
			{
				EditorGUILayout.LabelField(new GUIContent("You need to add at least one active item to the Events in order to use Auto Start feature!"), GUITools.BoldLabel);
			}
			else if (!autoStart.boolValue)
			{
				EditorGUILayout.LabelField(new GUIContent("Don't forget to start detection!", "You should start detector from code using ObscuredCheatingDetector.StartDetection() method. See readme for details."), GUITools.BoldLabel);
				EditorGUILayout.Separator();
			}
			EditorGUILayout.PropertyField(autoDispose);
			EditorGUILayout.PropertyField(keepAlive);

			EditorGUILayout.Separator();

			if (DrawUniqueDetectorProperties())
			{
				EditorGUILayout.Separator();
			}

			//DrawHeader("Events");

			EditorGUILayout.PropertyField(detectionEvent);
			DrawAdditionalEvents();
			serializedObject.ApplyModifiedProperties();

			EditorGUIUtility.labelWidth = 0;
		}

		protected virtual void DrawHeader(string text)
		{
			GUITools.DrawHeader(text);
		}

		protected virtual bool AdditionalEventsHasListeners()
		{
			return true;
		}

		protected virtual void FindUniqueDetectorProperties() {}
		protected virtual bool DrawUniqueDetectorProperties() { return false; }
		protected virtual void CheckAdditionalEventsForListeners() {}
		protected virtual void DrawAdditionalEvents() {}
	}
}