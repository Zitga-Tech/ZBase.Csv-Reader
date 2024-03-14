#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Time
{
	using Common;
	using Detectors;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using Utils;

	/// <summary>
	/// Speed-hack resistant %Time.* alternative.
	/// Does proxies to the regular %Time.* APIs until actual speed hack is detected.
	/// </summary>
	/// Requires running \ref CodeStage.AntiCheat.Detectors.SpeedHackDetector "SpeedHackDetector" to operate properly. Make sure to start SpeedHackDetector before calling Init(). <br/>
	/// Uses Unity's %Time.* APIs until speed hack is detected and switches to the speed-hack resistant time since then.
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	public class SpeedHackProofTime : KeepAliveBehaviour<SpeedHackProofTime>
	{
		private static bool inited;
		private static bool speedHackDetected;

		private static float reliableTime;
		private static float reliableDeltaTime;
		private static float reliableUnscaledTime;
		private static float reliableUnscaledDeltaTime;
		private static float reliableRealtimeSinceStartup;
		private static float reliableTimeSinceLevelLoad;

		private static bool warningShot;

		private long currentReliableTicks;
		private long lastFrameReliableTicks;
		private long reliableTicksDelta;
		
		protected override string GetComponentName()
		{
			return "SpeedHackProofTime";
		}

		#region Unity Events

		private void Update()
		{
			if (!speedHackDetected)
			{
				UpdateTimeValuesFromUnityTime();
			}
			else
			{
				currentReliableTicks = TimeUtils.GetReliableTicks();
				reliableTicksDelta = currentReliableTicks - lastFrameReliableTicks;

				UpdateReliableTimeValues();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			
			if (!selfDestroying)
				TimeUtils.Uninit();
		}

		#endregion

		/// <summary>
		/// Call to add to the scene and force internal initialization. Gets called automatically when necessary if not initialized.
		/// </summary>
		/// Make sure to call it after you setup and run \ref CodeStage.AntiCheat.Detectors.SpeedHackDetector "SpeedHackDetector".
		public static void Init()
		{
			inited = GetOrCreateInstance.InitInternal();
		}

		/// <summary>
		/// Call to remove from scene and clean internal resources.
		/// </summary>
		public static void Dispose()
		{
			inited = false;

			if (Instance == null)
			{
				return;
			}

			var detectorInstance = SpeedHackDetector.Instance;
			if (detectorInstance != null)
			{
				detectorInstance.CheatDetected -= Instance.OnSpeedHackDetected;
			}

			Destroy(Instance.gameObject);
		}

		/// <summary>
		/// Speed-hack resistant analogue on Unity's %Time.time API.
		/// </summary>
		public static float time
		{
			get
			{
				if (!inited)
				{
					Init();
				}

				return speedHackDetected ? reliableTime : Time.time;
			}
		}

		/// <summary>
		/// Speed-hack resistant analogue on Unity's %Time.unscaledTime API.
		/// </summary>
		public static float unscaledTime
		{
			get
			{
				if (!inited)
				{
					Init();
				}

				return speedHackDetected ? reliableUnscaledTime : Time.unscaledTime;
			}
		}

		/// <summary>
		/// Speed-hack resistant analogue on Unity's %Time.deltaTime API.
		/// </summary>
		public static float deltaTime
		{
			get
			{
				if (!inited)
				{
					Init();
				}

				return speedHackDetected ? reliableDeltaTime : Time.deltaTime;
			}
		}

		/// <summary>
		/// Speed-hack resistant analogue on Unity's %Time.unscaledDeltaTime API.
		/// </summary>
		public static float unscaledDeltaTime
		{
			get
			{
				if (!inited)
				{
					Init();
				}

				return speedHackDetected ? reliableUnscaledDeltaTime : Time.unscaledDeltaTime;
			}
		}

		/// <summary>
		/// Speed-hack resistant analogue on Unity's %Time.realtimeSinceStartup API.
		/// </summary>
		public static float realtimeSinceStartup
		{
			get
			{
				if (!inited)
				{
					Init();
				}

				return speedHackDetected ? reliableRealtimeSinceStartup : Time.realtimeSinceStartup;
			}
		}

		/// <summary>
		/// Speed-hack resistant analogue on Unity's %Time.timeSinceLevelLoad API.
		/// </summary>
		public static float timeSinceLevelLoad
		{
			get
			{
				if (!inited)
				{
					Init();
				}

				return speedHackDetected ? reliableTimeSinceLevelLoad : Time.timeSinceLevelLoad;
			}
		}

		private bool InitInternal()
		{
			var detectorInstance = SpeedHackDetector.Instance;
			if (detectorInstance == null)
			{
				if (!warningShot)
				{
					Debug.LogWarning(ACTk.LogPrefix +
					                 "Can't initialize SpeedHackProofTime class since it requires running SpeedHackDetector instance which was not found. " +
					                 "Did you started SpeedHackDetector before using SpeedHackProofTime?\n" +
					                 "SpeedHackProofTime will use unreliable vanilla Time.* APIs until you start SpeedHackDetector.");
					warningShot = true;
				}

				return false;
			}

			if (!detectorInstance.IsRunning)
			{
				if (!warningShot)
				{
					Debug.LogWarning(ACTk.LogPrefix +
					                 "Can't initialize SpeedHackProofTime class since it requires running SpeedHackDetector instance but only idle instance was found. " +
					                 "Did you started SpeedHackDetector before using SpeedHackProofTime?\n" +
					                 "SpeedHackProofTime will use unreliable vanilla Time.* APIs until you start SpeedHackDetector.");
					warningShot = true;
				}

				return false;
			}

			detectorInstance.CheatDetected += OnSpeedHackDetected;
			return true;
		}

		private void UpdateTimeValuesFromUnityTime()
		{
			reliableTime = Time.time;
			reliableDeltaTime = Time.deltaTime;

			reliableUnscaledTime = Time.unscaledTime;
			reliableUnscaledDeltaTime = Time.unscaledDeltaTime;

			reliableTimeSinceLevelLoad = Time.timeSinceLevelLoad;
			reliableRealtimeSinceStartup = Time.realtimeSinceStartup;
		}

		private void UpdateReliableTimeValues()
		{
			lastFrameReliableTicks = currentReliableTicks;

			reliableUnscaledDeltaTime = (float)reliableTicksDelta / TimeUtils.TicksPerSecond;
			reliableDeltaTime = reliableUnscaledDeltaTime * Time.timeScale;

			reliableTime += reliableDeltaTime;
			reliableUnscaledTime += reliableUnscaledDeltaTime;
			reliableRealtimeSinceStartup += reliableUnscaledDeltaTime;
			reliableTimeSinceLevelLoad += reliableDeltaTime;
		}

		private void OnSpeedHackDetected()
		{
			speedHackDetected = true;
			lastFrameReliableTicks = TimeUtils.GetReliableTicks();
		}

		protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			base.OnSceneLoaded(scene, mode);

			reliableTimeSinceLevelLoad = 0;
		}
	}
}