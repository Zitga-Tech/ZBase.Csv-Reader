#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using System.IO;
	using EditorCommon.Tools;
	using UnityEngine;
	
	internal static class Icons
	{
		public static Texture API => CSTextureLoader.GetIconTexture("API.png");
		public static Texture Forum => CSTextureLoader.GetIconTexture("Forum.png");
		public static Texture Discord => CSTextureLoader.GetIconTexture("Discord.png");
		public static Texture Manual => CSTextureLoader.GetIconTexture("Manual.png");
		public static Texture Home => CSTextureLoader.GetIconTexture("Home.png");
		public static Texture Support => CSTextureLoader.GetIconTexture("Support.png");
		
		static Icons()
		{
			SetupPath();
		}

		public static void SetupPath()
		{
			CSTextureLoader.ExternalTexturesFolder = Path.Combine(EditorTools.GetACTkDirectory(), "Editor");
		}
	}

	internal static class Images
	{
		public static Texture Logo => CSTextureLoader.GetTexture("Logo.png");
		
		static Images()
		{
			Icons.SetupPath();
		}
	}
}