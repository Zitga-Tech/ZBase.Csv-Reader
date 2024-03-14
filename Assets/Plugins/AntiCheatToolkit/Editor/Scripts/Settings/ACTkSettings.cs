#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using UnityEngine.Serialization;

namespace CodeStage.AntiCheat.EditorCode
{
	using Common;
	using System;
	using System.Collections.Generic;
	using System.IO;

	using UnityEditor;
	using UnityEngine;
	using Object = UnityEngine.Object;

	/// <summary>
	/// Represents settings scriptable object. Used mostly internally.
	/// </summary>
	/// Not intended for usage from user code,
	/// touch at your peril since API can change and break backwards compatibility!
	[Serializable]
	public class ACTkSettings : ScriptableObject
	{
		private const string Directory = "ProjectSettings";
		private const string Path = Directory + "/ACTkSettings.asset";

		[SerializeField]
		private bool injectionDetectorEnabled;

		[SerializeField]
		private bool preGenerateBuildHash;

		[SerializeField]
		private bool disableInjectionDetectorValidation;

		[FormerlySerializedAs("disableWallhackDetectorValidation")]
		[SerializeField]
		private bool disableWallhackDetectorShaderValidation;
		
		[SerializeField]
		private bool disableWallhackDetectorLinkXmlValidation;

		[SerializeField]
		private List<AllowedAssembly> injectionDetectorWhiteList = new List<AllowedAssembly>();

		[SerializeField]
		private string version = ACTk.Version;

		private static ACTkSettings instance;
		public static ACTkSettings Instance
		{
			get
			{
				if (instance != null) return instance;
				instance = LoadOrCreate();
				return instance;
			}
		}

		public static void Show()
		{
			SettingsService.OpenProjectSettings(ACTkEditorConstants.SettingsProviderPath);
		}

		public bool InjectionDetectorEnabled
		{
			get => injectionDetectorEnabled;
			set
			{
				injectionDetectorEnabled = value;
				Save();
			}
		}

		public bool PreGenerateBuildHash
		{
			get => preGenerateBuildHash;
			set
			{
				preGenerateBuildHash = value;
				Save();
			}
		}

		public bool DisableInjectionDetectorValidation
		{
			get => disableInjectionDetectorValidation;
			set
			{
				disableInjectionDetectorValidation = value;
				Save();
			}
		}

		public bool DisableWallhackDetectorShaderValidation
		{
			get => disableWallhackDetectorShaderValidation;
			set
			{
				disableWallhackDetectorShaderValidation = value;
				Save();
			}
		}
		
		public bool DisableWallhackDetectorLinkXmlValidation
		{
			get => disableWallhackDetectorLinkXmlValidation;
			set
			{
				disableWallhackDetectorLinkXmlValidation = value;
				Save();
			}
		}

		public List<AllowedAssembly> InjectionDetectorWhiteList
		{
			get => injectionDetectorWhiteList;
			set
			{
				injectionDetectorWhiteList = value;
				Save();
			}
		}

		public static void Delete()
		{
			instance = null;
			EditorTools.DeleteFile(Path);
		}

		public static void Save()
		{
			SaveInstance(Instance);
		}

		private static ACTkSettings LoadOrCreate()
		{
			ACTkSettings settings;

			if (!File.Exists(Path))
			{
				settings = CreateNewSettingsFile();
			}
			else
			{
				settings = LoadInstance();

				if (settings == null)
				{
					EditorTools.DeleteFile(Path);
					settings = CreateNewSettingsFile();
				}

				if (settings.version != ACTk.Version)
				{
					// for future migration reference
				}
			}

			settings.hideFlags = HideFlags.HideAndDontSave;
			settings.version = ACTk.Version;

			return settings;
		}

		private static ACTkSettings CreateNewSettingsFile()
		{
			var settingsInstance = CreateInstance();
			SaveInstance(settingsInstance);
			return settingsInstance;
		}

		private static void SaveInstance(ACTkSettings settingsInstance)
		{
			if (!System.IO.Directory.Exists(Directory)) 
				System.IO.Directory.CreateDirectory(Directory);

			try
			{
				UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { settingsInstance }, Path, true);
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Can't save settings!", e);
			}
		}

		private static ACTkSettings LoadInstance()
		{
			ACTkSettings settingsInstance;

			try
			{
				settingsInstance = (ACTkSettings)UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(Path)[0];
			}
			catch (Exception e)
			{
				Debug.Log($"{ACTk.LogPrefix}Can't read settings, resetting them to defaults.\n" +
						  "This message is harmless in most cases and can be ignored if not repeating.\n" +
						  $"Exception: {e}");
				settingsInstance = null;
			}

			return settingsInstance;
		}

		private static ACTkSettings CreateInstance()
		{
			var newInstance = CreateInstance<ACTkSettings>();
			return newInstance;
		}
	}
}