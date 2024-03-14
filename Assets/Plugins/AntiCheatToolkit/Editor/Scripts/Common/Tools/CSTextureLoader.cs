#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.EditorCommon.Tools
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEngine;

	internal enum ImageKind
	{
		External,
		InternalTexture,
		InternalIcon
	}

	internal static class CSTextureLoader
	{
		public static string ExternalTexturesFolder { get; set; }
		public static string LogPrefix { get; set; }
		
		private static readonly Dictionary<string, Texture> CachedTextures = new Dictionary<string, Texture>();

		public static Texture GetTexture(string fileName)
		{
			return GetTexture(fileName, false);
		}

		public static Texture GetIconTexture(string fileName, ImageKind kind = ImageKind.External)
		{
			return GetTexture(fileName, true, kind);
		}

		private static Texture GetTexture(string fileName, bool icon, ImageKind kind = ImageKind.External)
		{
			Texture result;
			var isDark = EditorGUIUtility.isProSkin;

			var textureName = fileName;
			if (isDark)
				textureName = "d_" + textureName;

			if (CachedTextures.ContainsKey(textureName))
			{
				result = CachedTextures[textureName];
			}
			else
			{
				var path = fileName;

				if (kind == ImageKind.External)
				{
					fileName = textureName;
					path = Path.Combine(ExternalTexturesFolder, "Textures");
				
					if (icon)
						path = Path.Combine(path, "Icons");
				
					path = Path.Combine(path, fileName);
					if (!File.Exists(Path.GetFullPath(path)) && !Path.HasExtension(path))
					{
						path = Path.ChangeExtension(path, "png");
					}
				
					if (!File.Exists(Path.GetFullPath(path)))
					{
						Debug.LogWarning("Couldn't find icon " + fileName + " at path " + path);
						return null;
					}
				}
				
				switch (kind)
				{
					case ImageKind.External:
						result = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
						break;
					case ImageKind.InternalTexture:
						result = EditorGUIUtility.FindTexture(path);
						break;
					case ImageKind.InternalIcon:
						result = EditorGUIUtility.IconContent(path).image;
						break;
					default:
						throw new ArgumentOutOfRangeException("kind", kind, null);
				}

				if (result == null)
					Debug.LogError(LogPrefix + "Some error occurred while looking for image\n" + path);
				else
					CachedTextures[textureName] = result;
			}
			return result;
		}

		public static Texture GetTypeImage(Type type)
		{
			var key = type.ToString();
			if (CachedTextures.ContainsKey(key))
			{
				return CachedTextures[key];
			}

			var texture = EditorGUIUtility.ObjectContent(null, type).image;
			CachedTextures.Add(key, texture);

			return texture;
		}
	}
}