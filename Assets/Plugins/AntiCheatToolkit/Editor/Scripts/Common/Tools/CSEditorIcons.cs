#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.EditorCommon.Tools
{
	using UnityEngine;

	internal static class CSEditorIcons
	{
		public static Texture AssetStore { get { return CSTextureLoader.GetIconTexture("Asset Store", ImageKind.InternalIcon); } }
		public static Texture Error { get { return CSTextureLoader.GetIconTexture("console.erroricon", ImageKind.InternalIcon); } }
		public static Texture ErrorSmall { get { return CSTextureLoader.GetIconTexture("console.erroricon.sml", ImageKind.InternalIcon); } }
		public static Texture Favorite { get { return CSTextureLoader.GetIconTexture("Favorite", ImageKind.InternalIcon); } }
		public static Texture FavoriteIcon { get { return CSTextureLoader.GetIconTexture("Favorite Icon", ImageKind.InternalIcon); } }
		public static Texture FilterByType { get { return CSTextureLoader.GetIconTexture("FilterByType", ImageKind.InternalIcon); } }
		public static Texture Folder { get { return CSTextureLoader.GetIconTexture("Folder Icon", ImageKind.InternalIcon); } }
		public static Texture GameObject { get { return CSTextureLoader.GetTypeImage(typeof(GameObject)); } }
		public static Texture Help { get { return CSTextureLoader.GetIconTexture("_Help", ImageKind.InternalIcon); } }
		public static Texture HierarchyView { get { return CSTextureLoader.GetIconTexture("UnityEditor.SceneHierarchyWindow", ImageKind.InternalIcon); } }
		public static Texture Info { get { return CSTextureLoader.GetIconTexture("console.infoicon", ImageKind.InternalIcon); } }
		public static Texture InfoSmall { get { return CSTextureLoader.GetIconTexture("console.infoicon.sml", ImageKind.InternalIcon); } }
		public static Texture Inspector { get { return CSTextureLoader.GetIconTexture("UnityEditor.InspectorWindow", ImageKind.InternalIcon); } }
		public static Texture Prefab { get { return UnityEditorInternal.InternalEditorUtility.FindIconForFile("dummy.prefab"); } }
		public static Texture ProjectView { get { return CSTextureLoader.GetIconTexture("Project", ImageKind.InternalIcon); } }
		public static Texture Scene { get { return UnityEditorInternal.InternalEditorUtility.FindIconForFile("dummy.unity"); } }
		public static Texture Script { get { return UnityEditorInternal.InternalEditorUtility.FindIconForFile("dummy.cs"); } }
		public static Texture Search { get { return CSTextureLoader.GetIconTexture("Search Icon", ImageKind.InternalIcon); } }
		public static Texture Settings { get { return CSTextureLoader.GetIconTexture("Settings", ImageKind.InternalIcon); } }
		public static Texture Warn { get { return CSTextureLoader.GetIconTexture("console.warnicon", ImageKind.InternalIcon); } }
		public static Texture WarnSmall { get { return CSTextureLoader.GetIconTexture("console.warnicon.sml", ImageKind.InternalIcon); } }
	}
}