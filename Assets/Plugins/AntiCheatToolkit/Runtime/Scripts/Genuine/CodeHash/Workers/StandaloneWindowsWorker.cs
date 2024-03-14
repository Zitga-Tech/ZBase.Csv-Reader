#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !UNITY_2019_1_OR_NEWER
#define ACTK_UWP_NO_IL2CPP
#endif

#if !ACTK_UWP_NO_IL2CPP

namespace CodeStage.AntiCheat.Genuine.CodeHash
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;
	using Common;
	using UnityEngine;
	using Utils;

	internal class StandaloneWindowsWorker : BaseWorker
	{
		public static BuildHashes GetBuildHashes(string buildPath, FileFilter[] fileFilters, SHA1Wrapper sha1)
		{
			var files = Directory.GetFiles(buildPath, "*", SearchOption.AllDirectories);
			var count = files.Length;
			if (count == 0)
			{
				return null;
			}

			var fileHashes = new List<FileHash>();
			for (var i = 0; i < count; i++)
			{
				var filePath = files[i];

				// skip folders since we can't hash them
				if (Directory.Exists(filePath))
				{
					continue;
				}

				foreach (var fileFilter in fileFilters)
				{
					if (fileFilter.MatchesPath(filePath, buildPath))
					{
#if UNITY_EDITOR
						UnityEditor.EditorUtility.DisplayProgressBar("ACTk: Generating code hash", "Hashing files...",
							(i + 1f) / count);
#endif
						using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
						using (var bs = new BufferedStream(fs))
						{
							var hash = sha1.ComputeHash(bs);
							var hashString = StringUtils.HashBytesToHexString(hash);
							//Debug.Log("Path: " + filePath + "\nHash: " + hashString);
							fileHashes.Add(new FileHash(filePath, hashString));
						}
					}
				}
			}

			if (fileHashes.Count == 0)
			{
				return null;
			}

			return new BuildHashes(buildPath, fileHashes, sha1);
		}

		public override void Execute()
		{
			base.Execute();

			try
			{
				var buildFolder = Path.GetFullPath(Application.dataPath + @"\..\");
				var t = new Thread(GenerateHashThread);
				t.Start(buildFolder);
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Something went wrong while calculating hash!", e);
				Complete(HashGeneratorResult.FromError(e.ToString()));
			}
		}

		private void GenerateHashThread(object folder)
		{
			var buildFolder = (string)folder;

			try
			{
				var sha1 = new SHA1Wrapper();
#if ENABLE_IL2CPP
				var il2cpp = true;
#else
				var il2cpp = false;
#endif
				var buildHashes = GetBuildHashes(buildFolder, CodeHashGenerator.GetFileFiltersStandaloneWindows(il2cpp),
					sha1);
				sha1.Clear();
				Complete(HashGeneratorResult.FromBuildHashes(buildHashes));
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Something went wrong in hashing thread!", e);
				Complete(HashGeneratorResult.FromError(e.ToString()));
			}
		}
	}
}

#endif