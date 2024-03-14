#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if UNITY_ANDROID && !UNITY_EDITOR
#define ACTK_ANDROID_DEVICE
#endif

#if ACTK_ANDROID_DEVICE

namespace CodeStage.AntiCheat.Genuine.CodeHash
{
	using Common;
	using System;
	using UnityEngine;

	internal class AndroidWorker : BaseWorker
	{
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers=false)]
		private class CodeHashGeneratorCallback : AndroidJavaProxy
		{
			private readonly AndroidWorker parent;

			public CodeHashGeneratorCallback(AndroidWorker parent) : base("net.codestage.actk.androidnative.CodeHashCallback")
			{
				this.parent = parent;
			}

			[System.Reflection.Obfuscation(Exclude = true)]
			// called from native Android plugin, from separate thread
			public void OnSuccess(string buildPath, string[] paths, string[] hashes, string summaryHash)
			{
				var fileHashes = new FileHash[hashes.Length];
				for (var i = 0; i < hashes.Length; i++)
				{
					var hash = hashes[i];
					var path = paths[i];

					fileHashes[i] = new FileHash(path, hash);
				}

				var buildHashes = new BuildHashes(buildPath, fileHashes, summaryHash);
				parent.Complete(HashGeneratorResult.FromBuildHashes(buildHashes));
			}

			[System.Reflection.Obfuscation(Exclude = true)]
			// called from native Android plugin, from separate thread
			public void OnError(string errorMessage)
			{
				parent.Complete(HashGeneratorResult.FromError(errorMessage));
			}
		}

		public override void Execute()
		{
			base.Execute();

		    const string classPath = "net.codestage.actk.androidnative.CodeHashGenerator";

		    try
		    {
			    using (var nativeClass = new AndroidJavaClass(classPath))
			    {
#if ENABLE_IL2CPP
					var il2cpp = true;
#else
				    var il2cpp = false;
#endif

				    var filters = CodeHashGenerator.GetFileFiltersAndroid(il2cpp);
					nativeClass.CallStatic("GetCodeHash", GenerateStringArrayFromFilters(filters), new CodeHashGeneratorCallback(this));
			    }
		    }
		    catch (Exception e)
		    {
				ACTk.PrintExceptionForSupport("Can't initialize NativeRoutines!", e);
		    }
		}

		private string[] GenerateStringArrayFromFilters(FileFilter[] filters)
		{
			var itemsCount = filters.Length;
			var result = new string[itemsCount];
			for (var i = 0; i < itemsCount; i++)
			{
				result[i] = filters[i].ToString();
			}

			return result;
		}
	}
}

#endif