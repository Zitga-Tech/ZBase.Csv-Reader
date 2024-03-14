#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !UNITY_2019_1_OR_NEWER
#define ACTK_UWP_NO_IL2CPP
#endif

#if !ACTK_UWP_NO_IL2CPP

namespace CodeStage.AntiCheat.Utils
{
	using System;
	using System.IO;
	using System.Security.Cryptography;
	
	internal static class CryptoUtils
	{
		private const int BufferSize = 81920;
		private const byte Iterations = 10;

		private const byte AesSaltLengthBytes = 16;
		private const byte AesKeyLengthBytes = 16;
		
		private const byte Rc2SaltLengthBytes = 8;
		private const byte Rc2KeyLengthBytes = 7;
		
		public static void EncryptAes(Stream input, Stream output, byte[] password)
		{
			EncryptInternal<AesCryptoServiceProvider>(input, output, password, AesKeyLengthBytes);
		}

		public static void DecryptAes(Stream input, Stream output, byte[] password)
		{
			DecryptInternal<AesCryptoServiceProvider>(input, output, password, AesKeyLengthBytes, AesSaltLengthBytes);
		}
		
		public static void EncryptRc2(Stream input, Stream output, byte[] password)
		{
			EncryptInternal<RC2CryptoServiceProvider>(input, output, password, Rc2KeyLengthBytes);
		}
		
		public static void DecryptRc2(Stream input, Stream output, byte[] password)
		{
			DecryptInternal<RC2CryptoServiceProvider>(input, output, password, Rc2KeyLengthBytes, Rc2SaltLengthBytes);
		}
		
		private static void EncryptInternal<T>(Stream input, Stream output, byte[] password, byte keyLength) where T : SymmetricAlgorithm, new()
		{
			using (var crypto = new T())
			{
				crypto.GenerateIV();
				var salt = crypto.IV;
				
				using (var key = new Rfc2898DeriveBytes(password, salt, Iterations))
				{
					crypto.Key = key.GetBytes(keyLength);
				}
				
				output.Write(salt, 0, salt.Length);
				
				using (var encryptor = crypto.CreateEncryptor())
				{
					using (var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
					{
						input.CopyTo(cryptoStream, BufferSize);
					}
				}
			}
		}
		
		private static void DecryptInternal<T>(Stream input, Stream output, byte[] password, byte keyLength, byte saltLength) where T : SymmetricAlgorithm, new()
		{
			try
			{
				using (var crypto = new T())
				{
					var salt = new byte[saltLength];
					input.Read(salt, 0, saltLength);
					crypto.IV = salt;

					using (var key = new Rfc2898DeriveBytes(password, salt, Iterations))
					{
						crypto.Key = key.GetBytes(keyLength);
					}

					using(var decryptor = crypto.CreateDecryptor())
					{
						using (var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
						{
							cryptoStream.CopyTo(output, BufferSize);
						}
					}
				}
			}
			catch (Exception e)
			{
				throw new CryptographicException("Something went wrong while trying to decrypt data!", e);
			}
		}
	}
}
#endif