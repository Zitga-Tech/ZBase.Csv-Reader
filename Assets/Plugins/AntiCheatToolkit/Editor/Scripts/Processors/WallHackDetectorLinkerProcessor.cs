#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if UNITY_2019_3_OR_NEWER && ACTK_WALLHACK_LINK_XML

namespace CodeStage.AntiCheat.EditorCode.Processors
{
	using System;
	using System.IO;
	using Common;
	using Detectors;
	using UnityEditor.Build;
	using UnityEditor.Build.Reporting;
	using UnityEditor.UnityLinker;
	using UnityEngine;

	internal class WallHackDetectorLinkerProcessor : IUnityLinkerProcessor
	{
		private static string linkData;
		
		public int callbackOrder { get; }

		private readonly string path;

		public WallHackDetectorLinkerProcessor()
		{
			path = ACTkEditorConstants.LinkXmlPath;
		}
		
		public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
		{
			try
			{
				if (linkData == null)
					linkData = ConstructLinkData();
				
				File.WriteAllText(path, linkData);
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Couldn't write link.xml!", e);
			}
			
			Debug.Log($"{ACTk.LogPrefix}Additional link.xml generated for {WallHackDetector.ComponentName}:\n{path}");
			return path;
		}

#if !UNITY_2021_2_OR_NEWER
		public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
		{
			// ignoring since it was deprecated in Unity 2021.2
		}

		public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
		{
			// ignoring since it was deprecated in Unity 2021.2
		}
#endif

		private string ConstructLinkData()
		{
			return "<linker>\n" +
				   $"\t<assembly fullname=\"{nameof(UnityEngine)}\">\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(BoxCollider)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(MeshCollider)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(CapsuleCollider)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(Camera)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(Rigidbody)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(MeshRenderer)}\" preserve=\"all\"/>\n" +
				   $"\t\t<type fullname=\"{nameof(UnityEngine)}.{nameof(CharacterController)}\" preserve=\"all\"/>\n" +
				   "\t</assembly>\n" +
				   "</linker>";
		}
	}
}

#endif