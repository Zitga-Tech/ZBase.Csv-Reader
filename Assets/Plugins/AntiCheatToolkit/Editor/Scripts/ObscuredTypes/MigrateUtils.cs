#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using Common;
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using ObscuredTypes;
	using PropertyDrawers;
	using UnityEditor;
	using UnityEngine;
	using Object = UnityEngine.Object;

	/// <summary>
	/// Class with utility functions to help with ACTk migrations after updates.
	/// </summary>
	public static partial class MigrateUtils
	{
		private const string MigrationVersion = "2";

		/// <summary>
		/// Checks all prefabs in project for old version of obscured types and tries to migrate values to the new version.
		/// </summary>
		public static void MigrateObscuredTypesOnPrefabs(params string[] typesToMigrate)
		{
			if (!EditorUtility.DisplayDialog("ACTk Obscured types migration",
				"Are you sure you wish to scan all prefabs in your project and automatically migrate values to the new format?\n" +
				GetWhatMigratesString(typesToMigrate),
				"Yes", "No"))
			{
				Debug.Log(ACTk.LogPrefix + "Obscured types migration was canceled by user.");
				return;
			}

			AssetDatabase.SaveAssets();

			var touchedCount = 0;
			try
			{
				var objectsToMigrate = new List<Object>();

				EditorUtility.DisplayProgressBar("ACTk: Looking through objects", "Collecting data...", 0);

				var assets = AssetDatabase.FindAssets("t:ScriptableObject");
				var count = assets.Length;
				for (var i = 0; i < count; i++)
				{
					var guid = assets[i];
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (!path.StartsWith("assets", StringComparison.OrdinalIgnoreCase)) continue;
					var objects = AssetDatabase.LoadAllAssetsAtPath(path);
					foreach (var unityObject in objects)
					{
						if (unityObject == null) continue;
						if (unityObject.name == "Deprecated EditorExtensionImpl") continue;
						objectsToMigrate.Add(unityObject);
					}
				}

				assets = AssetDatabase.FindAssets("t:Prefab");
				count = assets.Length;
				for (var i = 0; i < count; i++)
				{
					var guid = assets[i];
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (!path.StartsWith("assets", StringComparison.OrdinalIgnoreCase)) continue;
					var prefabRoot = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
					if (prefabRoot == null) continue;
					var components = prefabRoot.GetComponentsInChildren<Component>();

					foreach (var component in components)
					{
						if (component == null) continue;
						objectsToMigrate.Add(component);
					}
				}

				count = objectsToMigrate.Count;
				for (var i = 0; i < count; i++)
				{
					if (EditorUtility.DisplayCancelableProgressBar("Looking through objects",
						"Object " + (i + 1) + " from " + count,
						i / (float)count))
					{
						Debug.Log(ACTk.LogPrefix + "Obscured types migration was canceled by user.");
						break;
					}

					var unityObject = objectsToMigrate[i];

					var so = new SerializedObject(unityObject);
					var modified = MigrateObject(so, unityObject.name, typesToMigrate);

					if (modified)
					{
						touchedCount++;
						so.ApplyModifiedProperties();
					}
				}
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Something went wrong while migrating obscured types on prefabs!", e);
			}
			finally
			{
				AssetDatabase.SaveAssets();
				EditorUtility.ClearProgressBar();
			}

			if (touchedCount > 0)
				Debug.Log(ACTk.LogPrefix + "Migrated obscured types on " + touchedCount + " objects.");
			else
				Debug.Log(ACTk.LogPrefix + "No objects were found for obscured types migration.");
		}

		/// <summary>
		/// Checks all scenes in project for old version of obscured types and tries to migrate values to the new version.
		/// </summary>
		public static void MigrateObscuredTypesInScene(params string[] typesToMigrate)
		{
			if (!EditorUtility.DisplayDialog("ACTk Obscured types migration",
				"Are you sure you wish to scan all opened scenes and automatically migrate values to the new format?\n" +
				GetWhatMigratesString(typesToMigrate),
				"Yes", "No"))
			{
				Debug.Log(ACTk.LogPrefix + "Obscured types migration was canceled by user.");
				return;
			}

			var touchedCount = 0;
			try
			{
				var allTransformsInOpenedScenes = Resources.FindObjectsOfTypeAll<Transform>();
				var count = allTransformsInOpenedScenes.Length;
				var updateStep = Math.Max(count / 10, 1);

				for (var i = 0; i < count; i++)
				{
					var transform = allTransformsInOpenedScenes[i];
					if (i % updateStep == 0 && EditorUtility.DisplayCancelableProgressBar("Looking through objects",
						"Object " + (i + 1) + " from " + count,
						i / (float)count))
					{
						Debug.Log(ACTk.LogPrefix + "Obscured types migration was canceled by user.");
						break;
					}

					if (transform == null) continue;

					var components = transform.GetComponents<Component>();
					foreach (var component in components)
					{
						if (component == null) continue;

						var so = new SerializedObject(component);
						var modified = MigrateObject(so, transform.name, typesToMigrate);

						if (modified)
						{
							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
							touchedCount++;
							so.ApplyModifiedProperties();
						}
					}
				}
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Something went wrong while migrating obscured types in scene!", e);
			}
			finally
			{
				if (touchedCount > 0)
				{
					EditorUtility.DisplayDialog(touchedCount + " objects migrated",
						"Objects with old obscured types migrated: " + touchedCount +
						".\nPlease save your scenes to keep the changes.", "Fine");
					UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				}

				AssetDatabase.SaveAssets();
				EditorUtility.ClearProgressBar();
			}

			if (touchedCount > 0)
				Debug.Log(ACTk.LogPrefix + "Migrated obscured types on " + touchedCount +
						  " objects in opened scene(s).");
			else
				Debug.Log(ACTk.LogPrefix +
						  "No objects were found in opened scene(s) for obscured types migration.");
		}

		private static bool MigrateObject(SerializedObject so, string label, string[] typesToMigrate)
		{
			var modified = false;

			var sp = so.GetIterator();
			if (sp == null) return false;

			while (sp.NextVisible(true))
			{
				if (sp.propertyType != SerializedPropertyType.Generic) continue;

				var type = sp.type;

				if (Array.IndexOf(typesToMigrate, type) == -1) continue;

				modified |= ProcessProperty(sp, label, type);
			}

			return modified;
		}

		private static bool ProcessProperty(SerializedProperty sp, string label, string type)
		{
			var modified = false;

			switch (type)
			{
				case "ObscuredDouble":
				{
					modified = MigrateObscuredDouble(sp);
					break;
				}
				case "ObscuredFloat":
				{
					modified = MigrateObscuredFloat(sp);
					break;
				}
				case "ObscuredVector2":
				{
					modified = MigrateObscuredVector2(sp);
					break;
				}
				case "ObscuredVector3":
				{
					modified = MigrateObscuredVector3(sp);
					break;
				}
				case "ObscuredQuaternion":
				{
					modified = MigrateObscuredQuaternion(sp);
					break;
				}
				case "ObscuredString":
				{
					modified = MigrateObscuredStringIfNecessary(sp);
					break;
				}
			}
			
			if (modified)
				Debug.Log($"{ACTk.LogPrefix}Migrated property {sp.displayName}:{type} at the object {label}");

			return modified;
		}

		internal static bool MigrateObscuredStringIfNecessary(SerializedProperty sp)
		{
			var hiddenValueProperty = sp.FindPropertyRelative("hiddenValue");
			if (hiddenValueProperty == null) return false;

			var currentCryptoKeyOldProperty = sp.FindPropertyRelative("currentCryptoKey");
			if (currentCryptoKeyOldProperty == null) return false;

			var currentCryptoKeyOld = currentCryptoKeyOldProperty.stringValue;
			if (string.IsNullOrEmpty(currentCryptoKeyOld)) return false;

			var hiddenCharsProperty = sp.FindPropertyRelative("hiddenChars");
			if (hiddenCharsProperty == null) return false;

			var hiddenValue = ObscuredStringDrawer.GetBytesObsolete(hiddenValueProperty);

			var decrypted =
				ObscuredString.EncryptDecryptObsolete(ObscuredString.GetStringObsolete(hiddenValue),
					currentCryptoKeyOld);

			var currentCryptoKey = ObscuredString.GenerateKey();
			var hiddenChars = ObscuredString.InternalEncryptDecrypt(decrypted.ToCharArray(), currentCryptoKey);

			ObscuredStringDrawer.SetChars(hiddenCharsProperty, hiddenChars);
			var currentCryptoKeyProperty = sp.FindPropertyRelative("cryptoKey");
			ObscuredStringDrawer.SetChars(currentCryptoKeyProperty, currentCryptoKey);

			hiddenValueProperty.arraySize = 0;
			currentCryptoKeyOldProperty.stringValue = null;

			return true;
		}

		private static string GetWhatMigratesString(string[] typesToMigrate)
		{
			return string.Join(", ", typesToMigrate) + " will migrated.";
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct LongBytesUnion
		{
			[FieldOffset(0)]
			public readonly long l;

			[FieldOffset(0)]
			public ACTkByte8 b8;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct FloatIntBytesUnion
		{
			[FieldOffset(0)]
			public int i;

			[FieldOffset(0)]
			public ACTkByte4 b4;
		}
	}
}