#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define ACTK_DEBUG_ENABLED
#endif

namespace CodeStage.AntiCheat.Detectors
{
	using Common;

	using System;
	using UnityEngine;
	using Utils;

	/// <summary>
	/// Allows to detect Cheat Engine's speed hack (and maybe some other speed hack tools) usage.
	/// </summary>
	/// Just add it to any GameObject as usual or through the "GameObject > Create Other > Code Stage > Anti-Cheat Toolkit"
	/// menu to get started.<br/>
	/// You can use detector completely from inspector without writing any code except the actual reaction on cheating.
	///
	/// Avoid using detectors from code at the Awake phase.
	[AddComponentMenu(MenuPath + ComponentName)]
	[DisallowMultipleComponent]
	[HelpURL(ACTk.DocsRootUrl + "class_code_stage_1_1_anti_cheat_1_1_detectors_1_1_speed_hack_detector.html")]
	public class SpeedHackDetector : ACTkDetectorBase<SpeedHackDetector>
	{
		public const string ComponentName = "Speed Hack Detector";
		internal const string LogPrefix = ACTk.LogPrefix + ComponentName + ": ";

		#region public fields
		/// <summary>
		/// Time (in seconds) between detector checks.
		/// </summary>
		[Tooltip("Time (in seconds) between detector checks.")]
		public float interval = 1f;

		/// <summary>
		/// Allowed speed multiplier threshold. Do not set to too low values (e.g. 0 or 0.00*) since there are timer fluctuations on different hardware.
		/// </summary>
		[Tooltip("Allowed speed multiplier threshold. Do not set to too low values (e.g. 0 or 0.00*) since there are timer fluctuations on different hardware.")]
		[Range(0.05f, 5f)]
		public float threshold = 0.2f;

		/// <summary>
		/// Maximum false positives count allowed before registering speed hack.
		/// </summary>
		[Tooltip("Maximum false positives count allowed before registering speed hack.")]
		public byte maxFalsePositives = 3;

		/// <summary>
		/// Amount of sequential successful checks before clearing internal false positives counter.<br/>
		/// Set 0 to disable Cool Down feature.
		/// </summary>
		[Tooltip("Amount of sequential successful checks before clearing internal false positives counter.\nSet 0 to disable Cool Down feature.")]
		public int coolDown = 30;
		#endregion

		#region private variables
		private byte currentFalsePositives;
		private int currentCooldownShots;
		private long previousReliableTicks;

		private long previousVulnerableEnvironmentTicks;
		private long previousVulnerableRealtimeTicks;
		#endregion

		#region public static methods
		/// <summary>
		/// Creates new instance of the detector at scene if it doesn't exists. Make sure to call NOT from Awake phase.
		/// </summary>
		/// <returns>New or existing instance of the detector.</returns>
		public static SpeedHackDetector AddToSceneOrGetExisting()
		{
			return GetOrCreateInstance;
		}

		/// <summary>
		/// Starts speed hack detection for detector you have in scene.
		/// </summary>
		/// Make sure you have properly configured detector in scene with #autoStart disabled before using this method.
		public static SpeedHackDetector StartDetection()
		{
			if (Instance != null)
			{
				return Instance.StartDetectionInternal(null, Instance.interval, Instance.maxFalsePositives, Instance.coolDown);
			}

			Debug.LogError(LogPrefix + "can't be started since it doesn't exists in scene or not yet initialized!");
			return null;
		}

		/// <summary>
		/// Starts speed hack detection with specified callback.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		public static SpeedHackDetector StartDetection(Action callback)
		{
			return StartDetection(callback, GetOrCreateInstance.interval);
		}

		/// <summary>
		/// Starts speed hack detection with specified callback using passed interval.<br/>
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in seconds between speed hack checks. Overrides #interval property.</param>
		public static SpeedHackDetector StartDetection(Action callback, float interval)
		{
			return StartDetection(callback, interval, GetOrCreateInstance.maxFalsePositives);
		}

		/// <summary>
		/// Starts speed hack detection with specified callback using passed interval and maxFalsePositives.<br/>
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in seconds between speed hack checks. Overrides #interval property.</param>
		/// <param name="maxFalsePositives">Amount of possible false positives. Overrides #maxFalsePositives property.</param>
		public static SpeedHackDetector StartDetection(Action callback, float interval, byte maxFalsePositives)
		{
			return StartDetection(callback, interval, maxFalsePositives, GetOrCreateInstance.coolDown);
		}

		/// <summary>
		/// Starts speed hack detection with specified callback using passed interval, maxFalsePositives and coolDown.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in seconds between speed hack checks. Overrides #interval property.</param>
		/// <param name="maxFalsePositives">Amount of possible false positives. Overrides #maxFalsePositives property.</param>
		/// <param name="coolDown">Amount of sequential successful checks before resetting false positives counter. Overrides #coolDown property.</param>
		public static SpeedHackDetector StartDetection(Action callback, float interval, byte maxFalsePositives, int coolDown)
		{
			return GetOrCreateInstance.StartDetectionInternal(callback, interval, maxFalsePositives, coolDown);
		}

		/// <summary>
		/// Stops detector. Detector's component remains in the scene. Use Dispose() to completely remove detector.
		/// </summary>
		public static void StopDetection()
		{
			if (Instance != null)
			{
				Instance.StopDetectionInternal();
			}
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
			if (Instance != null)
			{
				Instance.DisposeInternal();
			}
		}
		#endregion

		private SpeedHackDetector() { } // prevents direct instantiation

		#region unity messages

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void OnApplicationPause(bool pause)
		{
			if (!pause && IsStarted)
			{
				ResetLastTicks();
			}
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void Update()
		{
			if (!IsRunning)
			{
				return;
			}

			var reliableTicks = TimeUtils.GetReliableTicks();
			var intervalTicks = (long)(interval * TimeUtils.TicksPerSecond);

			// return if configured interval is not passed yet
			if (reliableTicks - previousReliableTicks < intervalTicks)
			{
				return;
			}

			var vulnerableEnvironmentTicks = TimeUtils.GetEnvironmentTicks();
			var vulnerableRealtimeTicks = TimeUtils.GetRealtimeTicks();

			var reliableDelta = reliableTicks - previousReliableTicks;

			var vulnerableEnvironmentDelta = vulnerableEnvironmentTicks - previousVulnerableEnvironmentTicks;
			var vulnerableEnvironmentMultiplier = Math.Abs(1 - (double)vulnerableEnvironmentDelta / reliableDelta);

			var vulnerableRealtimeDelta = vulnerableRealtimeTicks - previousVulnerableRealtimeTicks;
			var vulnerableRealtimeMultiplier = Math.Abs(1 - (double)vulnerableRealtimeDelta / reliableDelta);

			var cheatedEnvironment = vulnerableEnvironmentMultiplier > threshold;
			var cheatedRealtime = vulnerableRealtimeMultiplier > threshold;

			if (cheatedEnvironment || cheatedRealtime)
			{
#if ACTK_DETECTION_BACKLOGS
				Debug.LogWarning(LogPrefix + "Detection backlog:\n" +
				                 "cheatedEnvironment: " + cheatedEnvironment + "\n" +
				                 "cheatedRealtime: " + cheatedRealtime + "\n" +
				                 "reliableDelta: " + reliableDelta + "\n" +
				                 "vulnerableEnvironmentDelta: " + vulnerableEnvironmentDelta + "\n" +
				                 "vulnerableRealtimeDelta: " + vulnerableRealtimeDelta + "\n" +
				                 "vulnerableEnvironmentMultiplier: " + vulnerableEnvironmentMultiplier + "\n" +
				                 "vulnerableRealtimeMultiplier: " + vulnerableRealtimeMultiplier + "\n" +
				                 "reliableTicks: " + reliableTicks + "\n" +
				                 "vulnerableEnvironmentTicks: " + vulnerableEnvironmentTicks + "\n" +
				                 "vulnerableRealtimeTicks: " + vulnerableRealtimeTicks);
#endif

				currentFalsePositives++;
				if (currentFalsePositives > maxFalsePositives)
				{
#if ACTK_DEBUG_ENABLED
					Debug.LogWarning(LogPrefix + "final detection!", this);
#endif
					OnCheatingDetected();
				}
				else
				{
#if ACTK_DEBUG_ENABLED
					Debug.LogWarning(LogPrefix + "detection! Allowed false positives left: " + (maxFalsePositives - currentFalsePositives), this);
#endif
					currentCooldownShots = 0;
					ResetLastTicks();
				}
			}
			else if (currentFalsePositives > 0 && coolDown > 0)
			{
#if ACTK_DEBUG_ENABLED
				Debug.Log(LogPrefix + "success shot! Shots till cool down: " + (coolDown - currentCooldownShots), this);
#endif
				currentCooldownShots++;
				if (currentCooldownShots >= coolDown)
				{
#if ACTK_DEBUG_ENABLED
					Debug.Log(LogPrefix + "cool down!", this);
#endif
					currentFalsePositives = 0;
				}
			}

			previousReliableTicks = reliableTicks;
			previousVulnerableEnvironmentTicks = vulnerableEnvironmentTicks;
			previousVulnerableRealtimeTicks = vulnerableRealtimeTicks;
		}
		
		protected override void OnDestroy()
		{
			base.OnDestroy();
			
			if (!selfDestroying)
				TimeUtils.Uninit();
		}

		#endregion

		private SpeedHackDetector StartDetectionInternal(Action callback, float checkInterval, byte falsePositives, int shotsTillCooldown)
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
			
			interval = checkInterval;
			maxFalsePositives = falsePositives;
			coolDown = shotsTillCooldown;

			ResetLastTicks();
			currentFalsePositives = 0;
			currentCooldownShots = 0;

			IsStarted = true;
			IsRunning = true;

			return this;
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null, interval, maxFalsePositives, coolDown);
		}

		protected override string GetComponentName()
		{
			return ComponentName;
		}

		private void ResetLastTicks()
		{
			previousReliableTicks = TimeUtils.GetReliableTicks();
			previousVulnerableEnvironmentTicks = TimeUtils.GetEnvironmentTicks();
			previousVulnerableRealtimeTicks = TimeUtils.GetRealtimeTicks();
		}
	}
}