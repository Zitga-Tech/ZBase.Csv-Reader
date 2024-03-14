#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Utils
{
	internal static class RandomUtils
	{
		public static string GenerateRandomString(int length)
		{
			var stringChars = new char[length];
			GenerateCharArrayKey(ref stringChars);

			return new string(stringChars);
		}

		internal static byte GenerateByteKey()
		{
			return (byte)ThreadSafeRandom.Next(100, 255);
		}

		internal static sbyte GenerateSByteKey()
		{
			return (sbyte)ThreadSafeRandom.Next(100, 127);
		}

		internal static char GenerateCharKey()
		{
			return (char)ThreadSafeRandom.Next(10000, 60000);
		}

		internal static short GenerateShortKey()
		{
			return (short)ThreadSafeRandom.Next(10000, short.MaxValue);
		}

		internal static ushort GenerateUShortKey()
		{
			return (ushort)ThreadSafeRandom.Next(10000, ushort.MaxValue);
		}

		internal static int GenerateIntKey()
		{
			return ThreadSafeRandom.Next(1000000000, int.MaxValue);
		}

		internal static uint GenerateUIntKey()
		{
			return (uint)GenerateIntKey();
		}

		internal static long GenerateLongKey()
		{
#if !ACTK_US_EXPORT_COMPATIBLE
			return ThreadSafeRandom.NextLong(1000000000000000000, long.MaxValue);
#else
			return GenerateIntKey();
#endif
		}

		internal static ulong GenerateULongKey()
		{
			return (ulong)GenerateLongKey();
		}

		internal static void GenerateCharArrayKey(ref char[] arrayToFill)
		{
			if (arrayToFill == null)
			{
				arrayToFill = new char[7];
			}
			else if (arrayToFill.Length < 7)
			{
				arrayToFill = new char[7];
			}

			ThreadSafeRandom.NextChars(arrayToFill);
		}
	}
}