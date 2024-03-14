#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using System.IO;

	internal static class InjectionConstants
	{
		public const string LegacyWhitelistRelativePath = "InjectionDetectorData/UserWhitelist.bytes";

		public const string PrefsKey = "ACTDIDEnabledGlobal";
		public const string DataSeparator = ":";

		public static readonly string ResourcesFolder = Path.Combine(ACTkEditorConstants.AssetsFolder, "Resources");
		public static readonly string DataFilePath = Path.Combine(ResourcesFolder, DataFileName);
		
		private const string DataFileName = "fndid.bytes";
	}
}