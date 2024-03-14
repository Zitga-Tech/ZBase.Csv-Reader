#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !UNITY_2019_1_OR_NEWER
#define ACTK_UWP_NO_IL2CPP
#endif

namespace CodeStage.AntiCheat.Genuine.CodeHash
{
	using System.IO;

#if !ACTK_UWP_NO_IL2CPP
	using System.Security.Cryptography;
#endif
	/// <summary>
	/// Just an Utility class to make it easier to work with SHA1.
	/// </summary>
	/// Not intended for usage from user code,
	/// touch at your peril since API can change and break backwards compatibility!
	public class SHA1Wrapper
	{
#if !ACTK_UWP_NO_IL2CPP
		private readonly SHA1Managed sha1;
#endif

		public SHA1Wrapper()
		{
#if !ACTK_UWP_NO_IL2CPP
			sha1 = new SHA1Managed();
#else
			UnityEngine.Debug.LogError("UWP .NET Scripting Backend is not supported. Please use IL2CPP instead.");
#endif
		}


		public byte[] ComputeHash(Stream stream)
		{
#if !ACTK_UWP_NO_IL2CPP
			return sha1.ComputeHash(stream);
#else
			return null;
#endif
		}

		public byte[] ComputeHash(byte[] bytes)
		{
#if !ACTK_UWP_NO_IL2CPP
			return sha1.ComputeHash(bytes);
#else
			return null;
#endif
		}

		public void Clear()
		{
#if !ACTK_UWP_NO_IL2CPP
			sha1.Clear();
#endif
		}
	}
}