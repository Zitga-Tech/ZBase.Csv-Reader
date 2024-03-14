#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Common
{
	using UnityEngine;
	using UnityEngine.SceneManagement;

	internal static class ContainerHolder
	{
		public const string ContainerName = "Anti-Cheat Toolkit";
		public static GameObject container;
	}

	/// <summary>
	/// Base class for ACTk in-scene objects which able to survive scene switch.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class KeepAliveBehaviour<T> : MonoBehaviour where T: KeepAliveBehaviour<T>
	{
		/// <summary>
		/// Will survive new level (scene) load if checked. Otherwise it will be destroyed.
		/// </summary>
		/// On dispose Behaviour follows 2 rules:
		/// - if Game Object's name is "Anti-Cheat Toolkit": it will be automatically
		/// destroyed if no other Behaviours left attached regardless of any other components or children;<br/>
		/// - if Game Object's name is NOT "Anti-Cheat Toolkit": it will be automatically destroyed only
		/// if it has neither other components nor children attached;
		[Tooltip("Detector will survive new level (scene) load if checked.")]
		public bool keepAlive = true;

		protected int instancesInScene;
		protected bool selfDestroying;

		#region static instance
		/// <summary>
		/// Allows reaching public properties from code.
		/// Can be null if behaviour does not exist in scene or if accessed at or before Awake phase.
		/// </summary>
		public static T Instance { get; protected set; }

		protected static T GetOrCreateInstance
		{
			get
			{
				if (Instance != null)
				{
					return Instance;
				}

				if (ContainerHolder.container == null)
				{
					ContainerHolder.container = new GameObject(ContainerHolder.ContainerName);
				}
				Instance = ContainerHolder.container.AddComponent<T>();
				return Instance;
			}
		}
		#endregion

		#region unity messages

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		protected virtual void Awake()
		{
			selfDestroying = false;
			
			instancesInScene++;
			if (Init(Instance, GetComponentName()))
			{
				Instance = (T)this;
			}
		}

		protected virtual void Start()
		{
			if (ContainerHolder.container == null && gameObject.name == ContainerHolder.ContainerName)
			{
				ContainerHolder.container = gameObject;
			}

			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		protected virtual void OnDestroy()
		{
			var componentsCount = GetComponentsInChildren<Component>().Length;
			if (transform.childCount == 0 && componentsCount <= 2)
			{
				Destroy(gameObject);
			}
			else if (name == ContainerHolder.ContainerName && componentsCount <= 2)
			{
				Destroy(gameObject);
			}

			instancesInScene--;

			SceneManager.sceneLoaded -= OnSceneLoaded;

			if (Instance == this)
			{
				Instance = null;
			}
		}

		#endregion

		protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (instancesInScene < 2)
			{
				if (!keepAlive)
				{
					DisposeInternal();
				}
			}
			else
			{
				if (!keepAlive && Instance != this)
				{
					DisposeInternal();
				}
			}
		}

		protected virtual bool Init(T instance, string detectorName)
		{
			if (instance != null && instance != this && instance.keepAlive)
			{
				selfDestroying = true;
				DisposeInternal();
				return false;
			}

			DontDestroyOnLoad(transform.parent != null ? transform.root.gameObject : gameObject);

			return true;
		}

		protected virtual void DisposeInternal()
		{
			Destroy(this);
		}

		protected abstract string GetComponentName();
	}
}