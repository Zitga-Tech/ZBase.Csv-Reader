#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if UNITY_ANDROID && !UNITY_EDITOR
#define ACTK_ANDROID_DEVICE
#endif

namespace CodeStage.AntiCheat.Genuine.CodeHash
{
	using System.Collections;
	using System.Collections.Generic;
	using Common;
	using UnityEngine;

	/// <summary>
	/// Generates current application runtime code hash to let you validate it against previously generated runtime code hash to detect external code manipulations.
	/// </summary>
	/// Calculation is done on the separate threads where possible to prevent noticeable CPU spikes and performance impact.<br/>
	/// Supported platforms: Windows PC, Android (more to come)<br/>
	/// Resulting hash in most cases should match value you get from the \ref CodeStage.AntiCheat.EditorCode.PostProcessors.CodeHashGeneratorPostprocessor "CodeHashGeneratorPostprocessor".
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	public class CodeHashGenerator : KeepAliveBehaviour<CodeHashGenerator>, ICodeHashGenerator
	{
		/// <summary>
		/// Subscribe to get resulting hash right after it gets calculated.
		/// </summary>
		public static event HashGeneratorResultHandler HashGenerated;

		/// <summary>
		/// Stores previously calculated result.
		/// Can be null if Generate() wasn't called yet or if it was called but calculation is still in process.
		/// </summary>
		/// \sa #IsBusy
		public HashGeneratorResult LastResult { get; private set; }

		private readonly WaitForSeconds cachedWaitForSeconds = new WaitForSeconds(0.3f);
		private BaseWorker currentWorker;

		/// <summary>
		/// Call to make sure current platform is compatible before calling Generate().
		/// </summary>
		/// <returns>True if current platform is supported by the CodeHashGenerator, otherwise returns false.</returns>
		public static bool IsTargetPlatformCompatible()
		{
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
			return true;
#else
			return false;
#endif
		}

		/// <summary>
		/// Creates new instance of the CodeHashGenerator at scene if it doesn't exists. Make sure to call NOT from Awake phase.
		/// </summary>
		/// <returns>New or existing instance of the detector.</returns>
		public static CodeHashGenerator AddToSceneOrGetExisting()
		{
			return GetOrCreateInstance;
		}

		/// <summary>
		/// Call to start current runtime code hash generation. Automatically adds instance to the scene if necessary.
		/// </summary>
		public static ICodeHashGenerator Generate()
		{
			return GetOrCreateInstance.GenerateInternal();
		}

		/// <summary>
		/// Indicates if hash generation is currently in process.
		/// </summary>
		public bool IsBusy
		{
			get
			{
				return currentWorker != null && currentWorker.IsBusy;
			}
		}

		internal static FileFilter[] GetFileFiltersAndroid(bool il2Cpp)
		{
			var result = new List<FileFilter>
			{
				new FileFilter
				{
					filterFileName = "classes",
					filterExtension = "dex"
				},
				new FileFilter
				{
					filterFileName = "libunity",
					filterExtension = "so"
				},
				new FileFilter
				{
					filterFileName = "libil2cpp",
					filterExtension = "so"
				},
				new FileFilter
				{
					filterFileName = "libmain",
					filterExtension = "so"
				},
				new FileFilter
				{
					filterFileName = "libMonoPosixHelper",
					filterExtension = "so"
				},
				new FileFilter
				{
					filterFileName = "libmonobdwgc",
					filterExtension = "so"
				},
				new FileFilter
				{
					filterFileName = "global-metadata",
					filterExtension = "dat"
				}
			};

			if (!il2Cpp)
			{
				result.Add(new FileFilter
				{
					filterExtension = "dll"
				});
			}

			return result.ToArray();
		}

		internal static FileFilter[] GetFileFiltersStandaloneWindows(bool il2Cpp)
		{
			var result = new List<FileFilter>
			{
				new FileFilter
				{
					filterExtension = "dll"
				},
				new FileFilter
				{
					filterExtension = "exe"
				},
				/*new FileFilter
				{
					filterPath = "_Data\\",
					caseSensitive = true,
					exactFolderMatch = false,
				},*/
			};

			return result.ToArray();
		}

		ICodeHashGenerator ICodeHashGenerator.Generate()
		{
			return Generate();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			HashGenerated = null;
		}

		protected override string GetComponentName()
		{
			return "CodeHashGenerator";
		}

		private ICodeHashGenerator GenerateInternal()
		{
			if (LastResult != null)
			{
				if (HashGenerated != null)
				{
					HashGenerated.Invoke(LastResult);
				}
				return this;
			}

			currentWorker = null;

#if UNITY_EDITOR
	#if UNITY_ANDROID || UNITY_STANDALONE_WIN
			Debug.LogWarning(ACTk.LogPrefix + "CodeHashGenerator does not work in Editor. Please use it at runtime only.\n" +
			                 "This message is harmless.");
	#else
			Debug.LogError(ACTk.LogPrefix + "CodeHashGenerator works only in Android and Windows Standalone runtimes (both Mono and IL2CPP).");
	#endif
			return this;
#else

	#if ACTK_ANDROID_DEVICE
			currentWorker = new AndroidWorker();
	#elif UNITY_STANDALONE_WIN
			currentWorker = new StandaloneWindowsWorker();
	#endif
			currentWorker.Execute();
			StartCoroutine(CalculationAwaiter());

			return this;
#endif
		}

		private IEnumerator CalculationAwaiter()
		{
			while (currentWorker.IsBusy)
			{
				yield return cachedWaitForSeconds;
			}

			LastResult = currentWorker.Result;

			if (HashGenerated != null)
			{
				HashGenerated.Invoke(LastResult);
			}
		}
	}
}