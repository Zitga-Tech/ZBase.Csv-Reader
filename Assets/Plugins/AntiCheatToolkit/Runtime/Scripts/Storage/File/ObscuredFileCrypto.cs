#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !UNITY_2019_1_OR_NEWER
#define ACTK_UWP_NO_IL2CPP
#endif

#if !ACTK_UWP_NO_IL2CPP

namespace CodeStage.AntiCheat.Storage
{
	using System;
	using System.IO;
	using System.Security.Cryptography;
	using Utils;

	internal static class ObscuredFileCrypto
    {
		private const int HashSize = sizeof(uint);
		private const int BufferSize = 81920;
		
        private static byte[] copyStreamBuffer;
		
		public static void Encrypt(Stream input, Stream output, EncryptionSettings settings)
        {
			EncryptInternal(input, output, settings);
		}

		public static void Decrypt(Stream input, Stream output, EncryptionSettings settings)
		{
			DecryptInternal(input, output, settings);
		}

		internal static uint CalculateHash(Stream input)
		{
			return CalculateHashInternal(input);
		}
		
		internal static uint ReadHash(Stream input)
		{
			var hashBuffer = new byte[HashSize];
			input.Read(hashBuffer, 0, HashSize);
			return BytesToHash(hashBuffer);
		}
		
		internal static void WriteHash(Stream writer, uint hash)
		{
			writer.Write(HashToBytes(hash), 0, HashSize);
		}
		

		private static uint CalculateHashInternal(Stream input)
		{
			var count = (int)input.Length;
			var hash = xxHash.CalculateHash(input, count, 1613878765);
			return hash;
		}

		private static void EncryptInternal(Stream input, Stream output, EncryptionSettings settings)
		{
			if (settings.ObscurationMode == ObscurationMode.Encrypted)
			{
#if ACTK_US_EXPORT_COMPATIBLE
				CryptoUtils.EncryptRc2(input, output, settings.Password);
#else
				CryptoUtils.EncryptAes(input, output, settings.Password);
#endif
			}
			else 
			{
				input.CopyTo(output, BufferSize);
			}
		}
		
		private static byte[] HashToBytes(uint hash)
		{
			return BitConverter.GetBytes(hash ^ 2122232456);
		}
		
		private static uint BytesToHash(byte[] hash)
		{
			return BitConverter.ToUInt32(hash, 0) ^ 2122232456;
		}

		private static void DecryptInternal(Stream input, Stream output, EncryptionSettings settings)
		{
			var cryptoKey = settings.Password;

			if (settings.ObscurationMode == ObscurationMode.Encrypted)
			{
				var position = input.Position;
				
#if ACTK_US_EXPORT_COMPATIBLE
				try
				{
					CryptoUtils.DecryptRc2(input, output, cryptoKey);
				}
				catch (CryptographicException)
				{
					input.Position = position;
					CryptoUtils.DecryptAes(input, output, cryptoKey);
				}
#else
				try
				{
					CryptoUtils.DecryptAes(input, output, cryptoKey);
				}
				catch (CryptographicException)
				{
					input.Position = position;
					CryptoUtils.DecryptRc2(input, output, cryptoKey);
				}
#endif
			}
			else
			{
				input.CopyTo(output, BufferSize);
			}
		}
	}
}

#endif