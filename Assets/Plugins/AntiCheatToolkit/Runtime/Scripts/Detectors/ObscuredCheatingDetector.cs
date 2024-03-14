#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Detectors
{
	using Common;

	using System;
	using UnityEngine;

	/// <summary>
	/// Detects CodeStage.AntiCheat.ObscuredTypes cheating.
	/// </summary>
	/// It allows cheaters to find desired (fake) values in memory and change them, keeping original values secure.<br/>
	/// It's like a cheese in the mouse trap - cheater tries to change some obscured value and get caught on it.
	///
	/// Just add it to any GameObject as usual or through the "GameObject > Create Other > Code Stage > Anti-Cheat Toolkit"
	/// menu to get started.<br/>
	/// You can use detector completely from inspector without writing any code except the actual reaction on cheating.
	///
	/// Avoid using detectors from code at the Awake phase.
	[AddComponentMenu(MenuPath + ComponentName)]
	[DisallowMultipleComponent]
	[HelpURL(ACTk.DocsRootUrl + "class_code_stage_1_1_anti_cheat_1_1_detectors_1_1_obscured_cheating_detector.html")]
	public class ObscuredCheatingDetector : ACTkDetectorBase<ObscuredCheatingDetector>
	{
		public const string ComponentName = "Obscured Cheating Detector";
		internal const string LogPrefix = ACTk.LogPrefix + ComponentName + ": ";

		#region public fields

		/// <summary>
		/// Max allowed difference between encrypted and fake values in \link ObscuredTypes.ObscuredDouble ObscuredDouble\endlink. Increase in case of false positives.
		/// </summary>
		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredDouble. Increase in case of false positives.")]
		public double doubleEpsilon = 0.0001d;

		/// <summary>
		/// Max allowed difference between encrypted and fake values in \link ObscuredTypes.ObscuredFloat ObscuredFloat\endlink. Increase in case of false positives.
		/// </summary>
		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredFloat. Increase in case of false positives.")]
		public float floatEpsilon = 0.0001f;

		/// <summary>
		/// Max allowed difference between encrypted and fake values in \link ObscuredTypes.ObscuredVector2 ObscuredVector2\endlink. Increase in case of false positives.
		/// </summary>
		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredVector2. Increase in case of false positives.")]
		public float vector2Epsilon = 0.1f;

		/// <summary>
		/// Max allowed difference between encrypted and fake values in \link ObscuredTypes.ObscuredVector3 ObscuredVector3\endlink. Increase in case of false positives.
		/// </summary>
		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredVector3. Increase in case of false positives.")]
		public float vector3Epsilon = 0.1f;

		/// <summary>
		/// Max allowed difference between encrypted and fake values in \link ObscuredTypes.ObscuredQuaternion ObscuredQuaternion\endlink. Increase in case of false positives.
		/// </summary>
		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredQuaternion. Increase in case of false positives.")]
		public float quaternionEpsilon = 0.1f;
		#endregion

		#region public static methods
		/// <summary>
		/// Creates new instance of the detector at scene if it doesn't exists. Make sure to call NOT from Awake phase.
		/// </summary>
		/// <returns>New or existing instance of the detector.</returns>
		public static ObscuredCheatingDetector AddToSceneOrGetExisting()
		{
			return GetOrCreateInstance;
		}

		/// <summary>
		/// Starts all Obscured types cheating detection for detector you have in scene.
		/// </summary>
		/// Make sure you have properly configured detector in scene with #autoStart disabled before using this method.
		public static ObscuredCheatingDetector StartDetection()
		{
			if (Instance != null)
			{
				return Instance.StartDetectionInternal(null);
			}

			Debug.LogError(LogPrefix + "can't be started since it doesn't exists in scene or not yet initialized!");
			return null;
		}

		/// <summary>
		/// Starts all Obscured types cheating detection with specified callback.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		public static ObscuredCheatingDetector StartDetection(Action callback)
		{
			return GetOrCreateInstance.StartDetectionInternal(callback);
		}

		/// <summary>
		/// Stops detector. Detector's component remains in the scene. Use Dispose() to completely remove detector.
		/// </summary>
		public static void StopDetection()
		{
			if (Instance != null) Instance.StopDetectionInternal();
		}

		/// <summary>
		/// Stops and completely disposes detector component.
		/// </summary>
		/// On dispose Detector follows 2 rules:
		/// - if Game Object's name is "Anti-Cheat Toolkit Detectors": it will be automatically
		/// destroyed if no other Detectors left attached regardless of any other components or children;<br/>
		/// - if Game Object's name is NOT "Anti-Cheat Toolkit Detectors": it will be automatically destroyed only
		/// if it has neither other components nor children attached;
		public static void Dispose()
		{
			if (Instance != null) Instance.DisposeInternal();
		}
		#endregion

		internal static bool ExistsAndIsRunning
		{
			get
			{
                return (object)Instance != null && Instance.IsRunning;
			}
		}

		private ObscuredCheatingDetector() {} // prevents direct instantiation

		private ObscuredCheatingDetector StartDetectionInternal(Action callback)
		{
			if (IsRunning)
			{
				Debug.LogWarning(LogPrefix + "already running!", this);
				return this;
			}

			if (!enabled)
			{
				Debug.LogWarning($"{LogPrefix}disabled but {nameof(StartDetection)} still called from somewhere (see stack trace for this message)!", this);
				return this;
			}

			if (callback != null && DetectorHasListeners())
			{
				Debug.LogWarning(LogPrefix + $"has properly configured Detection Event in the inspector or {nameof(CheatDetected)} event subscriber, but still get started with Action callback." +
								 $"Action will be called at the same time with Detection Event or {nameof(CheatDetected)} on detection." +
								 "Are you sure you wish to do this?", this);
			}
			
			if (callback == null && !DetectorHasListeners())
			{
				Debug.LogWarning($"{LogPrefix}was started without Detection Event, Callback or {nameof(CheatDetected)} event subscription." +
								 $"Cheat will not be detected until you subscribe to {nameof(CheatDetected)} event.", this);
			}

			if (callback != null)
				CheatDetected += callback;
			
			IsStarted = true;
			IsRunning = true;

			return this;
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null);
		}

		protected override string GetComponentName()
		{
			return ComponentName;
		}
    }
}