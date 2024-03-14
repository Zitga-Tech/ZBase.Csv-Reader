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
	using Common;

	using System;
	using UnityEngine;
	using System.Runtime.InteropServices;
	using UnityEngine.Serialization;
	using Utils;

	/// <summary>
	/// Use it instead of regular <c>double</c> for any cheating-sensitive variables.
	/// </summary>
	/// <strong><em>Regular type is faster and memory wiser comparing to the obscured one!</em></strong><br/>
	/// Feel free to use regular types for all short-term operations and calculations while keeping obscured type only at the long-term declaration (i.e. class field).
	[Serializable]
	public struct ObscuredDouble : IObscuredType, IFormattable, IEquatable<ObscuredDouble>, IComparable<ObscuredDouble>, IComparable<double>, IComparable
	{
#if UNITY_EDITOR
		public string migratedVersion;
#endif

		[SerializeField]
		private long currentCryptoKey;

		[SerializeField]
		private long hiddenValue;

		[SerializeField]
		[FormerlySerializedAs("hiddenValue")]
#pragma warning disable 414
		private ACTkByte8 hiddenValueOldByte8;
#pragma warning restore 414

		[SerializeField]
		private bool inited;

		[SerializeField]
		private double fakeValue;

		[SerializeField]
		private bool fakeValueActive;

		private ObscuredDouble(double value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);
			hiddenValueOldByte8 = default(ACTkByte8);

#if UNITY_EDITOR
			fakeValue = value;
			fakeValueActive = true;
			migratedVersion = null;
#else
			var detectorRunning = Detectors.ObscuredCheatingDetector.ExistsAndIsRunning;
			fakeValue = detectorRunning ? value : 0L;
			fakeValueActive = detectorRunning;
#endif
			inited = true;
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// Key can be generated automatically using GenerateKey().
		/// \sa Decrypt(), GenerateKey()
		public static long Encrypt(double value, long key)
		{
			return DoubleLongBytesUnion.XorDoubleToLong(value, key);
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// \sa Encrypt()
		public static double Decrypt(long value, long key)
		{
			return DoubleLongBytesUnion.XorLongToDouble(value, key);
		}

		/// <summary>
		/// Allows to update the encrypted value to the newer encryption format.
		/// </summary>
		/// Use when you have some encrypted values saved somewhere with previous ACTk version
		/// and you wish to set them using SetEncrypted() to the newer ACTk version obscured type.
		/// Current migration variants:
		/// from 0 or 1 to 2 - migrate obscured type from ACTk 1.5.2.0-1.5.8.0 to the 1.5.9.0+ format
		/// <param name="encrypted">Encrypted value you got from previous ACTk version obscured type with GetEncrypted().</param>
		/// <param name="fromVersion">Source format version.</param>
		/// <param name="toVersion">Target format version.</param>
		/// <returns>Migrated raw encrypted value which you may use for SetEncrypted() later.</returns>
		public static long MigrateEncrypted(long encrypted, byte fromVersion = 0, byte toVersion = 2)
		{
			return DoubleLongBytesUnion.Migrate(encrypted, fromVersion, toVersion);
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
		public static ObscuredDouble FromEncrypted(long encrypted, long key)
		{
			var instance = new ObscuredDouble();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Generates random key. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <returns>Key suitable for manual Encrypt() calls.</returns>
		public static long GenerateKey()
		{
			return RandomUtils.GenerateLongKey();
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		/// <param name="key">Encryption key needed to decrypt returned value.</param>
		/// <returns>Encrypted value as is.</returns>
		/// Use it in conjunction with SetEncrypted().<br/>
		/// Useful for saving data in obscured state.
		/// \sa FromEncrypted(), SetEncrypted()
		public long GetEncrypted(out long key)
		{
			if (!inited)
				Init();
			
			key = currentCryptoKey;
			return hiddenValue;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value. Crypto key should be same as when encrypted value was got with GetEncrypted().
		/// </summary>
		/// Use it in conjunction with GetEncrypted().<br/>
		/// Useful for loading data stored in obscured state.
		/// \sa FromEncrypted()
		public void SetEncrypted(long encrypted, long key)
		{
			inited = true;
			hiddenValue = encrypted;
			currentCryptoKey = key;

			if (Detectors.ObscuredCheatingDetector.ExistsAndIsRunning)
			{
				fakeValueActive = false;
				fakeValue = InternalDecrypt();
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
		public double GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(decrypted, currentCryptoKey);
		}

		private double InternalDecrypt()
		{
			if (!inited)
			{
				Init();
				return 0;
			}

#if ACTK_OBSCURED_AUTO_MIGRATION
			if (hiddenValueOldByte8.b1 != 0 ||
			    hiddenValueOldByte8.b2 != 0 ||
			    hiddenValueOldByte8.b3 != 0 ||
			    hiddenValueOldByte8.b4 != 0 ||
			    hiddenValueOldByte8.b5 != 0 ||
			    hiddenValueOldByte8.b6 != 0 ||
			    hiddenValueOldByte8.b7 != 0 ||
			    hiddenValueOldByte8.b8 != 0)
			{
				var union = new DoubleLongBytesUnion { b8 = hiddenValueOldByte8 };
				union.b8.Shuffle();
				hiddenValue = union.l;

				hiddenValueOldByte8.b1 = 0;
				hiddenValueOldByte8.b2 = 0;
				hiddenValueOldByte8.b3 = 0;
				hiddenValueOldByte8.b4 = 0;
				hiddenValueOldByte8.b5 = 0;
				hiddenValueOldByte8.b6 = 0;
				hiddenValueOldByte8.b7 = 0;
				hiddenValueOldByte8.b8 = 0;
			}
#endif

			var decrypted = Decrypt(hiddenValue, currentCryptoKey);

			if (Detectors.ObscuredCheatingDetector.ExistsAndIsRunning && fakeValueActive && Math.Abs(decrypted - fakeValue) > Detectors.ObscuredCheatingDetector.Instance.doubleEpsilon)
			{
				Detectors.ObscuredCheatingDetector.Instance.OnCheatingDetected();
			}

			return decrypted;
		}
		
		private void Init()
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(0, currentCryptoKey);
			fakeValue = 0;
			fakeValueActive = false;
			inited = true;
		}

		#region operators, overrides, interface implementations

		//! @cond
		public static implicit operator ObscuredDouble(double value)
		{
			return new ObscuredDouble(value);
		}

		public static implicit operator double(ObscuredDouble value)
		{
			return value.InternalDecrypt();
		}

		public static explicit operator ObscuredDouble(ObscuredFloat f)
		{
			return (float)f;
		}

		public static ObscuredDouble operator ++(ObscuredDouble input)
		{
			return Increment(input, 1);
		}

		public static ObscuredDouble operator --(ObscuredDouble input)
		{
			return Increment(input, -1);
		}

		private static ObscuredDouble Increment(ObscuredDouble input, double increment)
		{
			var decrypted = input.InternalDecrypt() + increment;
			input.hiddenValue = Encrypt(decrypted, input.currentCryptoKey);

			if (Detectors.ObscuredCheatingDetector.ExistsAndIsRunning)
			{
				input.fakeValue = decrypted;
				input.fakeValueActive = true;
			}
			else
			{
				input.fakeValueActive = false;
			}

			return input;
		}

		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecrypt().ToString();
		}

		public string ToString(string format)
		{
			return InternalDecrypt().ToString(format);
		}

		public string ToString(IFormatProvider provider)
		{
			return InternalDecrypt().ToString(provider);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecrypt().ToString(format, provider);
		}

		public override bool Equals(object obj)
		{
			return obj is ObscuredDouble && Equals((ObscuredDouble)obj);
		}

		public bool Equals(ObscuredDouble obj)
		{
			return obj.InternalDecrypt().Equals(InternalDecrypt());
		}

		public int CompareTo(ObscuredDouble other)
		{
			return InternalDecrypt().CompareTo(other.InternalDecrypt());
		}

		public int CompareTo(double other)
		{
			return InternalDecrypt().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
#if !ACTK_UWP_NO_IL2CPP
			return InternalDecrypt().CompareTo(obj);
#else
			if (obj == null) return 1;
			if (!(obj is double)) throw new ArgumentException("Argument must be double");
			return CompareTo((double)obj);
#endif
		}

		#endregion

		#region obsolete

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.")]
		public static void SetNewCryptoKey(long newKey) {}

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.")]
		public void ApplyNewCryptoKey() {}

		[Obsolete("Please use new Encrypt(value, key) API instead.", true)]
		public static long Encrypt(double value) { throw new Exception(); }

		[Obsolete("Please use new Decrypt(value, key) API instead.", true)]
		public static double Decrypt(long value) { throw new Exception(); }

		[Obsolete("Please use new FromEncrypted(encrypted, key) API instead.", true)]
		public static ObscuredDouble FromEncrypted(long encrypted) { throw new Exception(); }

		[Obsolete("Please use new GetEncrypted(out key) API instead.", true)]
		public long GetEncrypted() { throw new Exception(); }

		[Obsolete("Please use new SetEncrypted(encrypted, key) API instead.", true)]
		public void SetEncrypted(long encrypted) {}

		#endregion

		//! @endcond

		[StructLayout(LayoutKind.Explicit)]
		private struct DoubleLongBytesUnion
		{
			[FieldOffset(0)]
			internal double d;

			[FieldOffset(0)]
			internal long l;

			[FieldOffset(0)]
			internal ACTkByte8 b8;

			internal static long Migrate(long value, byte fromVersion, byte toVersion)
			{
				var u = FromLong(value);

				if (fromVersion < 2 && toVersion == 2)
				{
					u.b8.Shuffle();
				}

				return u.l;
			}

			internal static long XorDoubleToLong(double value, long key)
			{
				return FromDouble(value).Shuffle(key).l;
			}

			internal static double XorLongToDouble(long value, long key)
			{
				return FromLong(value).UnShuffle(key).d;
			}

			private static DoubleLongBytesUnion FromDouble(double value)
			{
				return new DoubleLongBytesUnion { d = value};
			}

			private static DoubleLongBytesUnion FromLong(long value)
			{
				return new DoubleLongBytesUnion { l = value};
			}

			private DoubleLongBytesUnion Shuffle(long key)
			{
				l ^= key;
				b8.Shuffle();

				return this;
			}

			private DoubleLongBytesUnion UnShuffle(long key)
			{
				b8.UnShuffle();
				l ^= key;

				return this;
			}
		}
	}
}