#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using System;
	using Common;
	using Detectors;
	using EditorCommon.Tools;
	using Genuine.CodeHash;
	using UnityEditor;
	using UnityEditorInternal;
	using UnityEngine;

	internal static class SettingsGUI
	{
		private const string Homepage = "https://codestage.net/uas/actk";
		private const string APILink = "https://codestage.net/uas_files/actk/api/";
		private const string DiscordLink = "https://discord.gg/Ppsb89naWf";
		private const string ForumLink = "https://forum.unity.com/threads/anti-cheat-toolkit-stop-cheaters-easily.196578/";
		private const string ReviewURL = "https://assetstore.unity.com/packages/slug/202695?aid=1011lGBp&pubref=actk#reviews";
		
		private static SerializedObject graphicsSettingsAsset;
		private static SerializedProperty includedShaders;

		public static void OnGUI()
		{
			GUITools.Separator();
			DrawSettingsHeader();

			GUILayout.Space(5f);
			DrawIL2CPPSection();

			EditorGUILayout.Space();
			DrawInjectionSection();

			EditorGUILayout.Space();
			DrawHashSection();

			EditorGUILayout.Space();
			DrawWallHackSection();

			EditorGUILayout.Space();
			DrawConditionalSection();
			
			GUILayout.Space(5f);
		}

		private static void DrawSettingsHeader()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(5f);
				using (new GUILayout.VerticalScope())
				{
					GUILayout.Label("Version: " + ACTk.Version);

					using (new GUILayout.HorizontalScope())
					{
						if (GUITools.ImageButton("", "Homepage", Icons.Home))
							Application.OpenURL(Homepage);

						if (GUITools.ImageButton("", "Live Support at Discord", Icons.Discord))
							Application.OpenURL(DiscordLink);
						
						if (GUITools.ImageButton("", "Forums Support thread", Icons.Forum))
							Application.OpenURL(ForumLink);

						if (GUITools.ImageButton("", "Other Support contacts", Icons.Support))
							Application.OpenURL(ACTk.SupportContact);
						
						if (GUITools.ImageButton("", "Write a Review at the Asset Store, it will help a lot!", CSEditorIcons.FavoriteIcon))
						{
							if (!Event.current.control)
								Application.OpenURL(ReviewURL);
							else
								AssetStore.Open(ReviewURL);
						}

						GUILayout.Space(10f);

						if (GUITools.ImageButton("", "Anti-Cheat Toolkit Manual (Readme.pdf)", Icons.Manual))
							EditorTools.OpenReadme();

						if (GUITools.ImageButton("", "API reference", Icons.API))
							Application.OpenURL(APILink);

						GUILayout.Space(10f);

						if (GUITools.ImageButton("", "About", CSEditorIcons.Help))
						{
							EditorUtility.DisplayDialog("About Anti-Cheat Toolkit v" + ACTk.Version,
								"Developer: Dmitriy Yukhanov\n" +
								"Logo: Daniele Giardini \\m/\n" +
								"Icons: Google, Discord\n" +
								"Support: my lovely wife, daughters and you! <3\n\n" +
								@"¯\_(ツ)_/¯", "Fine!");
						}
					}
					GUILayout.Space(1f);
				}

				GUILayout.FlexibleSpace();

				var logo = Images.Logo;
				if (logo != null)
				{
					logo.wrapMode = TextureWrapMode.Clamp;
					var logoRect = EditorGUILayout.GetControlRect(GUILayout.Width(logo.width), GUILayout.Height(logo.height));
					logoRect.y += 13;
					GUI.DrawTexture(logoRect, logo);
				}
			}
		}

		private static void DrawIL2CPPSection()
		{
			DrawSettingSection("IL2CPP is your friend",
				ACTkEditorPrefsSettings.IL2CPPFoldout,
				foldout => ACTkEditorPrefsSettings.IL2CPPFoldout = foldout, DrawContent);

			void DrawContent()
			{
				GUILayout.Label("IL2CPP prevents Mono injections and easy code decompilation. " +
								"Also consider obfuscating your metadata to make cheaters cry, see <b>readme</b> for details.",
					GUITools.RichLabel);

				GUILayout.Label("<b>Note: IL2CPP is AOT and does not support JIT!</b>", GUITools.RichMiniLabel);

				GUILayout.Space(5f);

				var supported = SettingsUtils.IsIL2CPPSupported();
				GUILayout.Label($"IL2CPP Supported: {CSColorTools.WrapBool(supported)}",
					GUITools.RichLabel);

				var enabled = SettingsUtils.IsIL2CPPEnabled();
				GUILayout.Label($"IL2CPP Enabled: {CSColorTools.WrapBool(enabled)}",
					GUITools.RichLabel);

				if (SettingsUtils.IsIL2CPPEnabled() || !SettingsUtils.IsIL2CPPSupported())
					return;

				GUILayout.Space(5f);
				EditorGUILayout.HelpBox("Use IL2CPP to stop injections & easy code decompilation",
					MessageType.Warning, true);
				GUILayout.Space(5f);
				if (GUILayout.Button(new GUIContent("Switch to IL2CPP")))
				{
					PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup,
						ScriptingImplementation.IL2CPP);
				}
			}
		}
		
		private static void DrawInjectionSection()
		{
			DrawSettingSection("Injection Detector",
				ACTkEditorPrefsSettings.InjectionFoldout,
				foldout => ACTkEditorPrefsSettings.InjectionFoldout = foldout, DrawContent);

			void DrawContent()
			{
				var enableInjectionDetector = ACTkSettings.Instance.InjectionDetectorEnabled;

				if (SettingsUtils.IsIL2CPPEnabled())
				{
					EditorGUILayout.HelpBox("Injection is not possible in IL2CPP,\n" +
											"this detector is not needed in IL2CPP builds", MessageType.Info, true);
					GUILayout.Space(5f);
				}
				else if (!InjectionRoutines.IsTargetPlatformCompatible())
				{
					EditorGUILayout.HelpBox(
						"Injection Detection is only supported in non-IL2CPP Standalone and Android builds",
						MessageType.Warning, true);
					GUILayout.Space(5f);
				}

				using (new GUILayout.HorizontalScope())
				{
					EditorGUI.BeginChangeCheck();
					enableInjectionDetector = EditorGUILayout.ToggleLeft(new GUIContent(
							"Add mono injection detection support to build",
							"Injection Detector checks assemblies against whitelist. " +
							"Please enable this option if you're using Injection Detector " +
							"and default whitelist will be generated while Unity builds resulting build.\n" +
							"Has no effect for IL2CPP or unsupported platforms."), enableInjectionDetector
					);
					if (EditorGUI.EndChangeCheck())
					{
						ACTkSettings.Instance.InjectionDetectorEnabled = enableInjectionDetector;
					}
				}

				GUILayout.Space(3);

				if (GUILayout.Button(new GUIContent(
						"Edit Custom Whitelist (" + ACTkSettings.Instance.InjectionDetectorWhiteList.Count + ")",
						"Fill any external assemblies which are not included into the project to the user-defined whitelist to make Injection Detector aware of them."))
				   )
				{
					UserWhitelistEditor.ShowWindow();
				}
			}
		}

		private static void DrawHashSection()
		{
			DrawSettingSection("Code Hash Generator",
				ACTkEditorPrefsSettings.HashFoldout,
				foldout => ACTkEditorPrefsSettings.HashFoldout = foldout, DrawContent);

			void DrawContent()
			{
				var option = ACTkSettings.Instance.PreGenerateBuildHash;

				EditorGUI.BeginChangeCheck();
				option = EditorGUILayout.ToggleLeft(
					new GUIContent("Generate code hash on build completion",
						"Generates hash after build is finished, prints it to the console & sends it via CodeHashGeneratorPostprocessor."),
					option);
				if (EditorGUI.EndChangeCheck())
				{
					ACTkSettings.Instance.PreGenerateBuildHash = option;
				}

				EditorGUILayout.Space();

				GUILayout.Label(
					"Can differ from runtime hash if you post-process code in resulting build (e.g. obfuscate, compress, etc.).",
					GUITools.RichLabel);

				GUILayout.Space(5f);
				EditorGUILayout.HelpBox(
					"Always make sure post-build hash equals runtime one if you're using it for later comparison",
					MessageType.Info, true);

				if (!CodeHashGenerator.IsTargetPlatformCompatible())
				{
					EditorGUILayout.HelpBox("Current platform is not supported: Windows or Android required",
						MessageType.Warning, true);
				}
			}
		}

		private static void DrawWallHackSection()
		{
			DrawSettingSection("WallHack Detector",
				ACTkEditorPrefsSettings.WallHackFoldout,
				foldout => ACTkEditorPrefsSettings.WallHackFoldout = foldout, DrawContent, false);
			
			void DrawContent()
			{
				DrawShaderPanel();
				GUILayout.Space(5f);
				DrawLinkXmlPanel();
			}

			void DrawShaderPanel()
			{
				using (GUITools.Vertical(GUITools.PanelWithBackground))
				{
					GUILayout.Label("Wireframe module shader", EditorStyles.boldLabel);
					GUILayout.Space(5f);
					GUILayout.Label(
						"Wireframe module uses own shader under the hood and it should be included into the build.",
						EditorStyles.wordWrappedLabel);

					ReadGraphicsAsset();

					if (graphicsSettingsAsset != null && includedShaders != null)
					{
						// outputs whole included shaders list, use for debug
						//EditorGUILayout.PropertyField(includedShaders, true);

						var shaderIndex = GetWallhackDetectorShaderIndex();

						EditorGUI.BeginChangeCheck();

						GUILayout.Label(
							$"Shader status: {CSColorTools.WrapString("included", "not included", shaderIndex != -1)}",
							GUITools.RichLabel);
						GUILayout.Space(5f);
						EditorGUILayout.HelpBox("You don't need to include it if you're not going to use Wireframe module",
							MessageType.Info, true);
						GUILayout.Space(5f);

						if (shaderIndex != -1)
						{
							if (GUILayout.Button("Remove shader"))
							{
								includedShaders.DeleteArrayElementAtIndex(shaderIndex);
								includedShaders.DeleteArrayElementAtIndex(shaderIndex);
							}

							GUILayout.Space(3);
						}
						else
						{
							using (GUITools.Horizontal())
							{
								if (GUILayout.Button("Auto Include"))
								{
									var shader = Shader.Find(WallHackDetector.WireframeShaderName);
									if (shader != null)
									{
										includedShaders.InsertArrayElementAtIndex(includedShaders.arraySize);
										var newItem = includedShaders.GetArrayElementAtIndex(includedShaders.arraySize - 1);
										newItem.objectReferenceValue = shader;
									}
									else
									{
										Debug.LogError(ACTk.ConstructErrorForSupport($"Can't find {WallHackDetector.WireframeShaderName} shader!"));
									}
								}

								if (GUILayout.Button("Include manually (see readme.pdf)"))
								{
									SettingsService.OpenProjectSettings("Project/Graphics");
								}
							}

							GUILayout.Space(3);
						}

						if (EditorGUI.EndChangeCheck())
						{
							graphicsSettingsAsset.ApplyModifiedProperties();
						}
					}
					else
					{
						GUILayout.Label("Can't automatically control " + WallHackDetector.WireframeShaderName +
										" shader existence at the Always Included Shaders list. Please, manage this manually in Graphics Settings.");
						if (GUILayout.Button("Open Graphics Settings"))
						{
							EditorApplication.ExecuteMenuItem("Edit/Project Settings/Graphics");
						}
					}
				}
			}
			
			void DrawLinkXmlPanel()
			{
				using (GUITools.Vertical(GUITools.PanelWithBackground))
				{
					GUILayout.Label("IL2CPP Strip Engine Code caution", EditorStyles.boldLabel);
					GUILayout.Space(5f);
					var linkXmlEnabled = SettingsUtils.IsLinkXmlEnabled();
					if (!linkXmlEnabled)
					{
						var linkXmlRequired = SettingsUtils.IsLinkXmlRequired();
						if (linkXmlRequired)
						{
							EditorGUILayout.HelpBox(
								"CAUTION: False positives are possible due to IL2CPP Strip Engine Code feature, enable automatic link.xml generation below to prevent it.",
								MessageType.Error);
							if (GUILayout.Button("Enable automatic link.xml generation"))
							{
								SettingsUtils.SwitchSymbol(ACTkEditorConstants.Conditionals.WallhackLinkXML, true);
							}
						}
						else
						{
							EditorGUILayout.HelpBox(
								"False positives are possible due to IL2CPP Strip Engine Code feature but you're safe since either IL2CPP or String Engine Code feature is not active now.",
								MessageType.Info);
							if (GUILayout.Button("Enable automatic link.xml generation anyway"))
							{
								SettingsUtils.SwitchSymbol(ACTkEditorConstants.Conditionals.WallhackLinkXML, true);
							}
						}
					}
					else
					{
						EditorGUILayout.HelpBox(
							"Automatic link.xml generation enabled, you are safe from possible false positives caused by IL2CPP Strip Engine Code feature.",
							MessageType.Info);
						if (GUILayout.Button("Disable automatic link.xml generation"))
						{
							SettingsUtils.SwitchSymbol(ACTkEditorConstants.Conditionals.WallhackLinkXML, false);
						}
					}

					GUILayout.Label(
						$"This setting is duplicated by {ACTkEditorConstants.Conditionals.WallhackLinkXML} conditional symbol in conditional symbols section below",
						EditorStyles.wordWrappedMiniLabel);

				}
			}
		}

		private static void DrawConditionalSection()
		{
			var header = "Conditional Compilation Symbols";
			if (EditorApplication.isCompiling)
			{
				header += $" [{CSColorTools.WrapString("compiling...", CSColorTools.ColorKind.Purple)}]";
			}
			
			DrawSettingSection(header,
				ACTkEditorPrefsSettings.ConditionalFoldout,
				foldout => ACTkEditorPrefsSettings.ConditionalFoldout = foldout, DrawContent);

			void DrawContent()
			{
				if (EditorApplication.isCompiling)
					GUI.enabled = false;

				GUILayout.Label("Here you may switch conditional compilation symbols used in ACTk.\n" +
								"Check Readme for more details on each symbol.", EditorStyles.wordWrappedLabel);
				EditorGUILayout.Space();

				var symbolsData = SettingsUtils.GetSymbolsData();

				GUILayout.Label("Debug Symbols", GUITools.LargeBoldLabel);
				GUITools.Separator();

				DrawSymbol(ref symbolsData.injectionDebug,
					ACTkEditorConstants.Conditionals.InjectionDebug,
					"Switches the Injection Detector debug.");
				DrawSymbol(ref symbolsData.injectionDebugVerbose,
					ACTkEditorConstants.Conditionals.InjectionDebugVerbose,
					"Switches the Injection Detector verbose debug level.");
				DrawSymbol(ref symbolsData.injectionDebugParanoid,
					ACTkEditorConstants.Conditionals.InjectionDebugParanoid,
					"Switches the Injection Detector paranoid debug level.");
				DrawSymbol(ref symbolsData.wallhackDebug,
					ACTkEditorConstants.Conditionals.WallhackDebug,
					"Switches the WallHack Detector debug - you'll see the WallHack objects in scene and get extra information in console.");
				DrawSymbol(ref symbolsData.detectionBacklogs,
					ACTkEditorConstants.Conditionals.DetectionBacklogs,
					"Enables additional logs in some detectors to make it easier to debug false positives.");
				DrawSymbol(ref symbolsData.genericDevLogs,
					ACTkEditorConstants.Conditionals.GenericDevLogs,
					"Enables additional generic development logs all across the toolkit (used mainly for development purposes).");

				EditorGUILayout.Space();

				GUILayout.Label("Compatibility Symbols", GUITools.LargeBoldLabel);
				GUITools.Separator();

				DrawSymbol(ref symbolsData.exposeThirdPartyIntegration,
					ACTkEditorConstants.Conditionals.ThirdPartyIntegration,
					"Enable to let other third-party code in project know you have ACTk added.");
				DrawSymbol(ref symbolsData.wallhackLinkXML,
					ACTkEditorConstants.Conditionals.WallhackLinkXML,
					"Enables automatic link.xml generation to prevent stripping of components required by WallHack Detector.\nSee details at WallHack Detector settings section above.");
				DrawSymbol(ref symbolsData.excludeObfuscation,
					ACTkEditorConstants.Conditionals.ExcludeObfuscation,
					"Enable if you use Unity-unaware obfuscators which support ObfuscationAttribute to help avoid names corruption.");
				DrawSymbol(ref symbolsData.preventReadPhoneState,
					ACTkEditorConstants.Conditionals.PreventReadPhoneState,
					"Disables ObscuredPrefs Lock To Device functionality.");
				DrawSymbol(ref symbolsData.preventInternetPermission,
					ACTkEditorConstants.Conditionals.PreventInternetPermission,
					"Disables TimeCheatingDetector functionality.");
				DrawSymbol(ref symbolsData.obscuredAutoMigration,
					ACTkEditorConstants.Conditionals.ObscuredAutoMigration,
					"Enables automatic migration of ObscuredFloat and ObscuredDouble instances from the ACTk 1.5.2.0-1.5.8.0 to the 1.5.9.0+ format. Reduces these types performance a bit.");
				DrawSymbol(ref symbolsData.usExportCompatible,
					ACTkEditorConstants.Conditionals.UsExportCompatible,
					"Enables US Encryption Export Regulations compatibility mode so ACTk do not force you to declare you're using encryption when publishing your application to the Apple App Store.");

				GUI.enabled = true;
			}
		}
		
		private static void DrawSettingSection(string caption, bool foldout, Action<bool> foldoutCallback, Action drawContentCallback, bool drawBackground = true)
		{
			using (var changed = new EditorGUI.ChangeCheckScope())
			{
				var fold = GUITools.DrawFoldHeader(caption, foldout);
				if (changed.changed)
				{
					foldout = fold;
					foldoutCallback(foldout);
				}
			}

			if (!foldout) return;

			GUILayout.Space(3f);
			using (GUITools.Horizontal())
			{
				GUILayout.Space(20f);
				
				using (GUITools.Vertical(drawBackground ? GUITools.PanelWithBackground : GUIStyle.none))
				{
					drawContentCallback();
					GUILayout.Space(3);
				}
				GUILayout.Space(5f);
			}
		}
		
		public static bool IsWallhackDetectorShaderIncluded()
		{
			var result = false;

			ReadGraphicsAsset();
			if (GetWallhackDetectorShaderIndex() != -1)
				result = true;

			return result;
		}

		private static void ReadGraphicsAsset()
		{
			if (graphicsSettingsAsset != null) return;

			var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
			if (assets.Length > 0)
				graphicsSettingsAsset = new SerializedObject(assets[0]);

			if (graphicsSettingsAsset != null)
			{
				includedShaders = graphicsSettingsAsset.FindProperty("m_AlwaysIncludedShaders");
				if (includedShaders == null)
					Debug.LogError(ACTk.ConstructErrorForSupport("Couldn't find m_AlwaysIncludedShaders property"));
			}
			else
			{
				Debug.LogError(ACTk.ConstructErrorForSupport("Couldn't find GraphicsSettings asset"));
			}
		}

		private static int GetWallhackDetectorShaderIndex()
		{
			if (graphicsSettingsAsset == null || includedShaders == null) return -1;

			var result = -1;
			graphicsSettingsAsset.Update();

			var itemsCount = includedShaders.arraySize;
			for (var i = 0; i < itemsCount; i++)
			{
				var arrayItem = includedShaders.GetArrayElementAtIndex(i);
				if (arrayItem.objectReferenceValue != null)
				{
					var shader = (Shader)(arrayItem.objectReferenceValue);

					if (shader.name == WallHackDetector.WireframeShaderName)
					{
						result = i;
						break;
					}
				}
			}

			return result;
		}

		private static void DrawSymbol(ref bool field, string symbol, string hint)
		{
			EditorGUI.BeginChangeCheck();
			field = EditorGUILayout.ToggleLeft(new GUIContent(symbol, hint), field);
			if (EditorGUI.EndChangeCheck())
			{
				SettingsUtils.SwitchSymbol(symbol, field);
			}
		}
	}
}