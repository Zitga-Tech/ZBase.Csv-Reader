#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	internal struct SymbolsData
	{
		public bool wallhackLinkXML;
		public bool excludeObfuscation;
		public bool preventReadPhoneState;
		public bool preventInternetPermission;
		public bool obscuredAutoMigration;
		public bool exposeThirdPartyIntegration;
		public bool usExportCompatible;
		
		public bool injectionDebug;
		public bool injectionDebugVerbose;
		public bool injectionDebugParanoid;
		public bool wallhackDebug;
		public bool detectionBacklogs;
		public bool genericDevLogs;
	}
}