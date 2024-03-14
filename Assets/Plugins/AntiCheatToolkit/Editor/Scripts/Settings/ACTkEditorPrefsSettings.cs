#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using System;
	using UnityEditor;

	internal static class ACTkEditorPrefsSettings
	{
		private class FoldoutStorage
		{
			private bool il2cpp;
			public bool Il2cpp
			{
				get => il2cpp;
				set
				{
					il2cpp = value;
					EditorPrefs.SetBool(IL2CPPFoldoutPref, value);
				}
			}
			
			private bool obscuredFilePrefs;
			public bool ObscuredFilePrefs
			{
				get => obscuredFilePrefs;
				set
				{
					obscuredFilePrefs = value;
					EditorPrefs.SetBool(ObscuredFilePrefsPref, value);
				}
			}
			
			private bool injection;
			public bool Injection
			{
				get => injection;
				set
				{
					injection = value;
					EditorPrefs.SetBool(InjectionFoldoutPref, value);
				}
			}
			
			private bool hash;
			public bool Hash
			{
				get => hash;
				set
				{
					hash = value;
					EditorPrefs.SetBool(HashFoldoutPref, value);
				}
			}
			
			private bool wallHack;
			public bool WallHack
			{
				get => wallHack;
				set
				{
					wallHack = value;
					EditorPrefs.SetBool(WallHackFoldoutPref, value);
				}
			}
			
			private bool conditional;
			public bool Conditional
			{
				get => conditional;
				set
				{
					conditional = value;
					EditorPrefs.SetBool(ConditionalFoldoutPref, value);
				}
			}

			public FoldoutStorage()
			{
				il2cpp = EditorPrefs.GetBool(IL2CPPFoldoutPref);
				obscuredFilePrefs = EditorPrefs.GetBool(ObscuredFilePrefsPref);
				injection = EditorPrefs.GetBool(InjectionFoldoutPref);
				hash = EditorPrefs.GetBool(HashFoldoutPref);
				wallHack = EditorPrefs.GetBool(WallHackFoldoutPref);
				conditional = EditorPrefs.GetBool(ConditionalFoldoutPref);
			}
		}
		
		private const string PrefsPrefix = "ACTkSettings_";

		private const string IL2CPPFoldoutPref = PrefsPrefix + "IL2CPPFoldout";
		private const string ObscuredFilePrefsPref = PrefsPrefix + "obscuredFilePrefsFoldout";
		private const string InjectionFoldoutPref = PrefsPrefix + "injectionFoldout";
		private const string HashFoldoutPref = PrefsPrefix + "hashFoldout";
		private const string WallHackFoldoutPref = PrefsPrefix + "wallHackFoldout";
		private const string ConditionalFoldoutPref = PrefsPrefix + "conditionalFoldout";

		private static readonly Lazy<FoldoutStorage> Storage = new Lazy<FoldoutStorage>(() => new FoldoutStorage());

		public static bool IL2CPPFoldout
		{
			get => Storage.Value.Il2cpp;
			set => Storage.Value.Il2cpp = value;
		}
		
		public static bool ObscuredFilePrefsFoldout
		{
			get => Storage.Value.ObscuredFilePrefs;
			set => Storage.Value.ObscuredFilePrefs = value;
		}

		public static bool InjectionFoldout
		{
			get => Storage.Value.Injection;
			set => Storage.Value.Injection = value;
		}
		
		public static bool HashFoldout
		{
			get => Storage.Value.Hash;
			set => Storage.Value.Hash = value;
		}
		
		public static bool WallHackFoldout
		{
			get => Storage.Value.WallHack;
			set => Storage.Value.WallHack = value;
		}
		
		public static bool ConditionalFoldout
		{
			get => Storage.Value.Conditional;
			set => Storage.Value.Conditional = value;
		}
		
		public static void FocusWallhackFoldout()
		{
			ConditionalFoldout = false;
			HashFoldout = false;
			InjectionFoldout = false;
			IL2CPPFoldout = false;
			
			WallHackFoldout = true;
		}
	}
}