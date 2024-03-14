#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using System;
	using UnityEditor;

	internal static class SettingsUtils
	{
		private static SymbolsData cachedSymbolsData;
		
		public static bool IsIL2CPPEnabled()
		{
			// ReSharper disable once ConvertIfStatementToReturnStatement - it's not redundant ;)
			if (PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup) ==
				ScriptingImplementation.IL2CPP)
			{
				return true;
			}

#if ENABLE_IL2CPP
			return true;
#else
			return false;
#endif
		}

		public static bool IsIL2CPPSupported()
		{
			return IsIL2CPPEnabled() || ReflectionTools.IsScriptingImplementationSupported(ScriptingImplementation.IL2CPP, EditorUserBuildSettings.selectedBuildTargetGroup);
		}

		public static bool IsLinkXmlRequired()
		{
			return IsIL2CPPEnabled() && PlayerSettings.stripEngineCode;
		}
		
		public static bool IsLinkXmlEnabled()
		{
			return GetSymbolsData().wallhackLinkXML;
		}
		
		public static SymbolsData GetSymbolsData()
		{
			var groups = (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup));
			foreach (var buildTargetGroup in groups)
			{
				if (buildTargetGroup == BuildTargetGroup.Unknown) continue;
				if (IsBuildTargetGroupNameObsolete(buildTargetGroup.ToString())) continue;

				try
				{
					var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
					
					cachedSymbolsData.wallhackLinkXML |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.WallhackLinkXML);
					cachedSymbolsData.excludeObfuscation |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.ExcludeObfuscation);
					cachedSymbolsData.preventReadPhoneState |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.PreventReadPhoneState);
					cachedSymbolsData.preventInternetPermission |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.PreventInternetPermission);
					cachedSymbolsData.obscuredAutoMigration |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.ObscuredAutoMigration);
					cachedSymbolsData.exposeThirdPartyIntegration |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.ThirdPartyIntegration);
					cachedSymbolsData.usExportCompatible |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.UsExportCompatible);

					cachedSymbolsData.injectionDebug |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.InjectionDebug);
					cachedSymbolsData.injectionDebugVerbose |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.InjectionDebugVerbose);
					cachedSymbolsData.injectionDebugParanoid |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.InjectionDebugParanoid);
					cachedSymbolsData.wallhackDebug |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.WallhackDebug);
					cachedSymbolsData.detectionBacklogs |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.DetectionBacklogs);
					cachedSymbolsData.genericDevLogs |= GetSymbol(symbols, ACTkEditorConstants.Conditionals.GenericDevLogs);
				}
				catch (Exception)
				{
					// ignoring obsolete ones
				}
			}

			return cachedSymbolsData;
		}

		private static bool GetSymbol(string symbols, string symbol)
		{
			var result = false;

			if (symbols == symbol)
			{
				result = true;
			}
			else if (symbols.StartsWith(symbol + ';'))
			{
				result = true;
			}
			else if (symbols.EndsWith(';' + symbol))
			{
				result = true;
			}
			else if (symbols.Contains(';' + symbol + ';'))
			{
				result = true;
			}

			return result;
		}

		public static void SwitchSymbol(string symbol, bool active)
		{
			var groups = (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup));
			
			// this is required to get names for obsolete items alternatives like iOS instead of deprecated iPhone
			var names = Enum.GetNames(typeof(BuildTargetGroup));
			
			for (var i = 0; i < groups.Length; i++)
			{
				var buildTargetGroup = groups[i];
				var buildTargetName = names[i];
				
				if (buildTargetGroup == BuildTargetGroup.Unknown) continue;
				if (IsBuildTargetGroupNameObsolete(buildTargetName)) continue;

				try
				{
					var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

					if (active)
						AddSymbol(ref symbols, symbol);
					else
						RemoveSymbol(ref symbols, symbol);

					PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
				}
				catch (Exception)
				{
					// ignoring obsolete ones
				}
			}
		}

		private static void AddSymbol(ref string symbols, string symbol)
		{
			if (symbols.Length == 0)
			{
				symbols = symbol;
			}
			else
			{
				if (symbols.EndsWith(";"))
				{
					symbols += symbol;
				}
				else
				{
					symbols += ';' + symbol;
				}
			}
		}

		private static void RemoveSymbol(ref string symbols, string symbol)
		{
			if (symbols == symbol)
			{
				symbols = string.Empty;
			}
			else if (symbols.StartsWith(symbol + ';'))
			{
				symbols = symbols.Remove(0, symbol.Length + 1);
			}
			else if (symbols.EndsWith(';' + symbol))
			{
				symbols = symbols.Remove(symbols.LastIndexOf(';' + symbol, StringComparison.Ordinal), symbol.Length + 1);
			}
			else if (symbols.Contains(';' + symbol + ';'))
			{
				symbols = symbols.Replace(';' + symbol + ';', ";");
			}
		}

		private static bool IsBuildTargetGroupNameObsolete(string name)
		{
			var fi = typeof(BuildTargetGroup).GetField(name);
			var attributes = (ObsoleteAttribute[])fi.GetCustomAttributes(typeof(ObsoleteAttribute), false);
			return attributes.Length > 0;
		}
	}
}