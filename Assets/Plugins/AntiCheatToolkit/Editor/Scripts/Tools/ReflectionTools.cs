#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using System;
	using System.Reflection;
	using Common;
	using UnityEditor;
	using UnityEngine;
#if UNITY_2021_2_OR_NEWER
	using UnityEditor.Build;
#endif

	internal static class ReflectionTools
	{
		private static readonly Type ScriptingImplementationType = typeof(ScriptingImplementation);
#if UNITY_2021_2_OR_NEWER
		private delegate object GetScriptingImplementations(NamedBuildTarget target);
		private static readonly Type NamedBuildTargetType = typeof(NamedBuildTarget);
#else
		private delegate object GetScriptingImplementations(BuildTargetGroup target);
		private static readonly Type BuildTargetGroupType = typeof(BuildTargetGroup);
#endif
		private static readonly Type InspectorWindowType = ScriptingImplementationType.Assembly.GetType("UnityEditor.Modules.ModuleManager", false);
		private static readonly Type ScriptingImplementationsType = ScriptingImplementationType.Assembly.GetType("UnityEditor.Modules.IScriptingImplementations", false);

		

		private static GetScriptingImplementations getScriptingImplementationsDelegate;
		private static MethodInfo scriptingImplementationsTypeEnabledMethodInfo;

		public static bool IsScriptingImplementationSupported(ScriptingImplementation implementation, BuildTargetGroup target)
		{
			if (InspectorWindowType == null)
			{
				Debug.LogError(ACTk.ConstructErrorForSupport("Couldn't find UnityEditor.Modules.ModuleManager type!"));
				return false;
			}

			if (ScriptingImplementationsType == null)
			{
				Debug.LogError(ACTk.ConstructErrorForSupport("Couldn't find UnityEditor.Modules.IScriptingImplementations type!"));
				return false;
			}

			if (getScriptingImplementationsDelegate == null)
			{
#if UNITY_2021_2_OR_NEWER
				var mi = InspectorWindowType.GetMethod("GetScriptingImplementations", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new []{NamedBuildTargetType}, null);
#else
				var mi = InspectorWindowType.GetMethod("GetScriptingImplementations", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new []{BuildTargetGroupType}, null);
#endif
				if (mi == null)
				{
					Debug.LogError(ACTk.ConstructErrorForSupport("Couldn't find GetScriptingImplementations method!"));
					return false;
				}
				getScriptingImplementationsDelegate = (GetScriptingImplementations)Delegate.CreateDelegate(typeof(GetScriptingImplementations), mi);
			}

#if UNITY_2021_2_OR_NEWER
			var result = getScriptingImplementationsDelegate.Invoke(NamedBuildTarget.FromBuildTargetGroup(target));
#else
			var result = getScriptingImplementationsDelegate.Invoke(target);
#endif
			if (result == null) // happens for default platform support module
			{
				return PlayerSettings.GetDefaultScriptingBackend(target) == implementation;
			}

			if (scriptingImplementationsTypeEnabledMethodInfo == null)
			{
				scriptingImplementationsTypeEnabledMethodInfo = ScriptingImplementationsType.GetMethod("Enabled", BindingFlags.Public | BindingFlags.Instance);
				if (scriptingImplementationsTypeEnabledMethodInfo == null)
				{
					Debug.LogError(ACTk.ConstructErrorForSupport("Couldn't find IScriptingImplementations.Enabled() method!"));
					return false;
				}
			}

			var enabledImplementations = (ScriptingImplementation[])scriptingImplementationsTypeEnabledMethodInfo.Invoke(result, null);
			return Array.IndexOf(enabledImplementations, implementation) != -1;
		}
	}
}