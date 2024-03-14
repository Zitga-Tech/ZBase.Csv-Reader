#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Use it to guess current directory of the Anti-Cheat Toolkit.
	/// </summary>
	public class ACTkMarker : ScriptableObject
	{
		/// <summary>
		/// Returns raw path of the ACTkMarker script for further reference.
		/// </summary>
		/// <returns>Path of the ACTkMarker ScriptableObject asset.</returns>
		public static string GetAssetPath()
		{
			string result;

			var tempInstance = CreateInstance<ACTkMarker>();
			var script = MonoScript.FromScriptableObject(tempInstance);
			if (script != null)
			{
				result = AssetDatabase.GetAssetPath(script);
			}
			else
			{
				result = AssetDatabase.FindAssets("ACTkMarker")[0];
				result = AssetDatabase.GUIDToAssetPath(result);
			}

			DestroyImmediate(tempInstance);
			return result;
		}
	}
}