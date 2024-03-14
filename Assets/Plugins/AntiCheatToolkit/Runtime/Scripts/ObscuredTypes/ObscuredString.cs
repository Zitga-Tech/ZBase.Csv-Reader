#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if (UNITY_WINRT || UNITY_WINRT_10_0 || UNITY_WSA || UNITY_WSA_10_0) && !UNITY_2019_1_OR_NEWER
#define ACTK_UWP_NO_IL2CPP
#endif

namespace CodeStage.AntiCheat.ObscuredTypes
{
	using System;
	using Common;
	using UnityEngine;
	using Utils;

	/// <summary>
	/// Use it instead of regular <c>string</c> for any cheating-sensitive variables.
	/// </summary>
	/// <strong><em>Regular type is faster and memory wiser comparing to the obscured one!</em></strong><br/>
	/// Feel free to use regular types for all short-term operations and calculations while keeping obscured type only at the long-term declaration (i.e. class field).
	[Serializable]
	public sealed class ObscuredString : IObscuredType, IComparable<ObscuredString>, IComparable<string>, IComparable
	{
		[SerializeField]
		private string currentCryptoKey; // deprecated

#pragma warning disable 0649
		[SerializeField]
		private byte[] hiddenValue; // deprecated
#pragma warning restore 0649

		[SerializeField]
		private char[] cryptoKey;

		[SerializeField]
		private char[] hiddenChars;

		[SerializeField]
		private bool inited;

		[SerializeField]
		private string fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		// for serialization purposes
		private ObscuredString(){}

		private ObscuredString(string value)
		{
			cryptoKey = new char[7];
			GenerateKey(ref cryptoKey);
			hiddenChars = InternalEncryptDecrypt(value.ToCharArray(), cryptoKey);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
#else
			var detectorRunning = Detectors.ObscuredCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : null;
			fakeValueActive = detectorRunning;
#endif
			inited = true;
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static char[] Encrypt(string value, string key)
		{
			return Encrypt(value, key.ToCharArray());
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static char[] Encrypt(string value, char[] key)
		{
			return Encrypt(value.ToCharArray(), key);
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static char[] Encrypt(char[] value, char[] key)
		{
			return InternalEncryptDecrypt(value, key);
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// \sa Encrypt()
		public static string Decrypt(char[] value, string key)
		{
			return Decrypt(value, key.ToCharArray());
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// \sa Encrypt()
		public static string Decrypt(char[] value, char[] key)
		{
			return new string(InternalEncryptDecrypt(value, key));
		}

		/// <summary>
		/// Creates and fills obscured variable with raw encrypted value previously got from GetEncrypted().
		/// </summary>
		/// Literally does same job as SetEncrypted() but makes new instance instead of filling existing one,
		/// making it easier to initialize new variables from saved encrypted values.
		///
		/// <param name="encrypted">Raw encrypted value you got from GetEncrypted().</param>
		/// <param name="key">Encryption key you've got from GetEncrypted().</param>
		/// <returns>New obscured variable initialized from specified encrypted value.</returns>
		/// \sa GetEncrypted(), SetEncrypted()
		public static ObscuredString FromEncrypted(char[] encrypted, char[] key)
		{
			var instance = new ObscuredString();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Use this only to decrypt data encrypted with previous ACTk versions.
		/// </summary>
		/// Please use \ref FromEncrypted() "FromEncrypted(char[], char[])" in other cases.
		[Obsolete("Use this only to decrypt data encrypted with previous ACTk versions. " +
		          "Please use FromEncrypted(char[], char[]) in other cases.")]
		public static ObscuredString FromEncrypted(string encrypted, string key = "4441")
		{
			var instance = new ObscuredString();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Generates random key in new allocated array. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <returns>Key suitable for manual Encrypt() calls.</returns>
		public static char[] GenerateKey()
		{
			var arrayToFill = new char[7];
			GenerateKey(ref arrayToFill);
			return arrayToFill;
		}

		/// <summary>
		/// Generates random key. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <param name="arrayToFill">Preallocated char array. Only first 7 bytes are filled.</param>
		public static void GenerateKey(ref char[] arrayToFill)
		{
			RandomUtils.GenerateCharArrayKey(ref arrayToFill);
		}

		[Obsolete("Please use version with ref argument or without arguments instead.")]
		public static char[] GenerateKey(char[] arrayToFill)
		{
			RandomUtils.GenerateCharArrayKey(ref arrayToFill);
			return arrayToFill;
		}

		internal static char[] InternalEncryptDecrypt(char[] value, char[] key)
		{
			if (value == null || value.Length == 0)
			{
				return value;
			}

			if (key.Length == 0)
			{
				Debug.LogError(ACTk.LogPrefix + "Empty key can't be used for string encryption or decryption!");
				return value;
			}

			var keyLength = key.Length;
			var valueLength = value.Length;

			var result = new char[valueLength];

			for (var i = 0; i < valueLength; i++)
			{
				result[i] = (char)(value[i] ^ key[i % keyLength]);
			}

			return result;
		}

		internal static string EncryptDecryptObsolete(string value, string key)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			if (string.IsNullOrEmpty(key))
			{
				Debug.LogError(ACTk.LogPrefix + "Empty key can't be used for string encryption or decryption!");
				return string.Empty;
			}

			var keyLength = key.Length;
			var valueLength = value.Length;

			var result = new char[valueLength];

			for (var i = 0; i < valueLength; i++)
			{
				result[i] = (char)(value[i] ^ key[i % keyLength]);
			}

			return new string(result);
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		/// <param name="key">Encryption key needed to decrypt returned value.</param>
		/// <returns>Encrypted value as is.</returns>
		/// Use it in conjunction with SetEncrypted().<br/>
		/// Useful for saving data in obscured state.
		/// \sa FromEncrypted(), SetEncrypted()
		public char[] GetEncrypted(out char[] key)
		{
			if (!inited)
				Init();
			
			key = cryptoKey;
			return hiddenChars;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value. Crypto key should be same as when encrypted value was got with GetEncrypted().
		/// </summary>
		/// Use it in conjunction with GetEncrypted().<br/>
		/// Useful for loading data stored in obscured state.
		/// \sa FromEncrypted()
		public void SetEncrypted(char[] encrypted, char[] key)
		{
			inited = true;
			hiddenChars = encrypted;
			cryptoKey = key;

			if (Detectors.ObscuredCheatingDetector.ExistsAndIsRunning)
			{
				fakeValueActive = false;
				fakeValue = InternalDecryptToString();
				fakeValueActive = true;
			}
			else
			{
				fakeValueActive = false;
			}
		}

		/// <summary>
		/// Use this only to decrypt data encrypted with previous ACTk versions.
		/// </summary>
		/// Please use \ref SetEncrypted() "SetEncrypted(char[], char[])" in other cases.
		[Obsolete("Use this only to decrypt data encrypted with previous ACTk versions. " +
		          "Please use SetEncrypted(char[], char[]) in other cases.")]
		public void SetEncrypted(string encrypted, string key)
		{
			inited = true;
			var decrypted = EncryptDecryptObsolete(encrypted, key);
			cryptoKey = GenerateKey();
			hiddenChars = Encrypt(decrypted, cryptoKey);

			if (Detectors.ObscuredCheatingDetector.ExistsAndIsRunning)
			{
				fakeValueActive = false;
				fakeValue = InternalDecryptToString();
				fakeValueActive = true;
			}
			else
			{
				fakeValueActive = false;
			}
		}

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public string GetDecrypted()
		{
			return InternalDecryptToString();
		}

		/// <summary>
		/// GC-friendly alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value as a raw chars array in case you don't wish to allocate new string.</returns>
		public char[] GetDecryptedToChars()
		{
			return InternalDecrypt();
		}

		/// <summary>
		/// Allows to change current crypto key to the new random value and re-encrypt variable using it.
		/// Use it for extra protection against 'unknown value' search.
		/// Just call it sometimes when your variable doesn't change to fool the cheater.
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly produces some GC allocations, be careful when using it!</strong>
		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			GenerateKey(ref cryptoKey);
			hiddenChars = InternalEncryptDecrypt(decrypted, cryptoKey); // encrypting
		}

		private string InternalDecryptToString()
		{
			return new string(InternalDecrypt());
		}

		private char[] InternalDecrypt()
		{
			if (!inited)
			{
				Init();
				return new char[0];
			}

			if (!string.IsNullOrEmpty(currentCryptoKey))
			{
				MigrateFromACTkV1();
			}

			var decrypted = InternalEncryptDecrypt(hiddenChars, cryptoKey);

			if (Detectors.ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && !CompareCharsToString(decrypted, fakeValue))
			{
				Detectors.ObscuredCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}
		
		private void Init()
		{
			cryptoKey = new char[7];
			GenerateKey(ref cryptoKey);
			hiddenChars = InternalEncryptDecrypt(new char[0], cryptoKey); // encrypting
			fakeValue = string.Empty;
			fakeValueActive = false;
			inited = true;
		}

		private bool CompareCharsToString(char[] chars, string s)
		{
			if (chars.Length != s.Length) return false;

			for (var i = 0; i < chars.Length; i++)
			{
				if (chars[i] != s[i])
				{
					return false;
				}
			}

			return true;
		}

		internal void MigrateFromACTkV1()
		{
			var decryptedOld = EncryptDecryptObsolete(GetStringObsolete(hiddenValue), currentCryptoKey);
			GenerateKey(ref cryptoKey);
			hiddenChars = InternalEncryptDecrypt(decryptedOld.ToCharArray(), cryptoKey);
			currentCryptoKey = null;
		}

		#region operators, overrides, interface implementations

		//! @cond
		public int Length
		{
			get { return hiddenChars.Length; }
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public char this[int index]
		{
			get
			{
				if (index < 0 || index >= Length)
				{
					throw new IndexOutOfRangeException();
				}

				return InternalDecrypt()[index];
			}
		}

		public static implicit operator ObscuredString(string value)
		{
			return value == null ? null : new ObscuredString(value);
		}

		public static implicit operator string(ObscuredString value)
		{
			return value == null ? null : value.InternalDecryptToString();
		}

		/// <summary>
		/// Determines whether two specified ObscuredStrings have the same value.
		/// </summary>
		///
		/// <returns>
		/// true if the value of <paramref name="a"/> is the same as the value of <paramref name="b"/>; otherwise, false.
		/// </returns>
		/// <param name="a">An ObscuredString or null. </param><param name="b">An ObscuredString or null. </param><filterpriority>3</filterpriority>
		public static bool operator ==(ObscuredString a, ObscuredString b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if ((object)a == null || (object)b == null)
			{
				return false;
			}

			if (a.cryptoKey == b.cryptoKey)
			{
				return ArraysEquals(a.hiddenChars, b.hiddenChars);
			}

			return ArraysEquals(a.InternalDecrypt(), b.InternalDecrypt());
		}

		/// <summary>
		/// Determines whether two specified ObscuredStrings have different values.
		/// </summary>
		///
		/// <returns>
		/// true if the value of <paramref name="a"/> is different from the value of <paramref name="b"/>; otherwise, false.
		/// </returns>
		/// <param name="a">An ObscuredString or null. </param><param name="b">An ObscuredString or null. </param><filterpriority>3</filterpriority>
		public static bool operator !=(ObscuredString a, ObscuredString b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public string Substring(int startIndex)
		{
			return Substring(startIndex, Length - startIndex);
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public string Substring(int startIndex, int length)
		{
			return InternalDecryptToString().Substring(startIndex, length);
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public bool StartsWith(string value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			return InternalDecryptToString().StartsWith(value, comparisonType);
		}

		/// <summary>
		/// Proxy to the String API.
		/// Please consider avoiding using this in a hot path since it invokes decryption on every access call.
		/// </summary>
		public bool EndsWith(string value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			return InternalDecryptToString().EndsWith(value, comparisonType);
		}

		public override int GetHashCode()
		{
			return InternalDecryptToString().GetHashCode();
		}

		public override string ToString()
		{
			return new string(InternalDecrypt());
		}

		public override bool Equals(object obj)
		{
			var obscuredString = obj as ObscuredString;
			return obscuredString != null && Equals(obscuredString);
		}

		public bool Equals(ObscuredString value)
		{
			if (value == null) return false;

			if (cryptoKey == value.cryptoKey)
			{
				return ArraysEquals(hiddenChars, value.hiddenChars);
			}

			return ArraysEquals(InternalDecrypt(), value.InternalDecrypt());
		}

		public bool Equals(ObscuredString value, StringComparison comparisonType)
		{
			return value != null && string.Equals(InternalDecryptToString(), value.InternalDecryptToString(), comparisonType);
		}

		public int CompareTo(ObscuredString other)
		{
			return InternalDecryptToString().CompareTo(other.InternalDecryptToString());
		}

		public int CompareTo(string other)
		{
			return InternalDecryptToString().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
#if !ACTK_UWP_NO_IL2CPP
			return InternalDecryptToString().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is string)) throw new ArgumentException("Argument must be string");
			return CompareTo((string)obj);
#endif
		}

		#endregion

		#region obsolete

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.")]
		public static void SetNewCryptoKey(string newKey) {}

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.")]
		public void ApplyNewCryptoKey() {}

		[Obsolete("Please use new Encrypt(value, key) or Decrypt(value, key) API instead.", true)]
		public static string EncryptDecrypt(string value) { throw new Exception(); }


		[Obsolete("Please use new Encrypt(value, key) or Decrypt(value, key) APIs instead. " +
		          "This API will be removed in future updates.")]
		public static string EncryptDecrypt(string value, string key)
		{
			return EncryptDecryptObsolete(value, key);
		}

/*		[Obsolete("Please use new FromEncrypted(encrypted, key) API instead.", true)]
		public static ObscuredString FromEncrypted(string encrypted) { throw new Exception(); }*/

		[Obsolete("Please use new GetEncrypted(out key) API instead.", true)]
		public string GetEncrypted() { throw new Exception(); }

		[Obsolete("Please use new SetEncrypted(encrypted, key) API instead.", true)]
		public void SetEncrypted(string encrypted) {}

		#endregion

		//! @endcond

		internal static string GetStringObsolete(byte[] bytes)
		{
			var chars = new char[bytes.Length / sizeof(char)];
			Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		internal static byte[] GetBytesObsolete(string str)
		{
			var bytes = new byte[str.Length * sizeof(char)];
			Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		private static bool ArraysEquals(char[] a1, char[] a2)
		{
			if (a1 == a2) return true;
			if (a1 == null || a2 == null) return false;
			if (a1.Length != a2.Length) return false;

			for (var i = 0; i < a1.Length; i++)
			{
				if (a1[i] != a2[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}