#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Utils
{
	using UnityEngine;

	internal class AppEventsDispatcher : MonoBehaviour
	{
		public delegate void ApplicationFocusEventHandler(bool hasFocus);
		public delegate void ApplicationPauseEventHandler(bool pauseStatus);

		public event ApplicationFocusEventHandler ApplicationFocused;
		public event ApplicationPauseEventHandler ApplicationPaused;

		private static AppEventsDispatcher instance;
		public static AppEventsDispatcher Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new GameObject("[ACTk] " + nameof(AppEventsDispatcher)).AddComponent<AppEventsDispatcher>();
				}

				return instance;
			}
		}

		private void Awake()
		{
			gameObject.hideFlags = HideFlags.HideInHierarchy;
			DontDestroyOnLoad(transform.root.gameObject);
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			ApplicationFocused?.Invoke(hasFocus);
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			ApplicationPaused?.Invoke(pauseStatus);
		}
	}
}