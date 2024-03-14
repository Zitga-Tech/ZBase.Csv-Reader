#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Utils
{
	using System;

	/// <summary>
	/// Random utility which can be used from background threads.
	/// </summary>
	public static class ThreadSafeRandom
	{
		private static readonly Random Global = new Random();

		[ThreadStatic]
		private static Random local;
		
		/// <summary>
		/// Generates random <c>int</c> number within specified range.
		/// </summary>
		/// <param name="minInclusive">Minimal value, inclusive.</param>
		/// <param name="maxExclusive">Maximum value, exclusive.</param>
		/// <returns>Random value in specified range.</returns>
		public static int Next(int minInclusive, int maxExclusive)
		{
			var inst = local;

			if (inst != null)
			{
				return inst.Next(minInclusive, maxExclusive);
			}

			int seed;

			lock (Global)
			{
				seed = Global.Next();
			}

			local = inst = new Random(seed);
			return inst.Next(minInclusive, maxExclusive);
		}

		/// <summary>
		/// Generates random <c>long</c> number within specified range.
		/// </summary>
		/// <param name="minInclusive">Minimal value, inclusive.</param>
		/// <param name="maxExclusive">Maximum value, exclusive.</param>
		/// <returns>Random value in specified range.</returns>
		public static long NextLong(long minInclusive, long maxExclusive)
		{
			var inst = local;

			if (inst != null)
			{
				return NextLong(inst, minInclusive, maxExclusive);
			}

			int seed;

			lock (Global)
			{
				seed = Global.Next();
			}

			local = inst = new Random(seed);
			return NextLong(inst, minInclusive, maxExclusive);
		}

		/// <summary>
		/// Fills passed buffer with random bytes.
		/// </summary>
		/// <param name="buffer">Buffer filled with random bytes.</param>
		public static void NextBytes(byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			
			var inst = local;

			if (inst != null)
			{
				inst.NextBytes(buffer);
				return;
			}

			int seed;

			lock (Global)
			{
				seed = Global.Next();
			}

			local = inst = new Random(seed);
			inst.NextBytes(buffer);
		}

		/// <summary>
		/// Fills passed buffer with random <c>char</c> values.
		/// </summary>
		/// <param name="buffer">Buffer filled with random <c>char</c> values.</param>
		public static void NextChars(char[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			
			var inst = local;

			if (inst != null)
			{
				NextChars(inst, buffer);
				return;
			}

			int seed;

			lock (Global)
			{
				seed = Global.Next();
			}

			local = inst = new Random(seed);
			NextChars(inst, buffer);
		}

		private static long NextLong(Random random, long minInclusive, long maxExclusive)
		{
			var result = (long)random.Next((int)(minInclusive >> 32), (int)(maxExclusive >> 32));
			result <<= 32;
			result |= (uint)random.Next((int)minInclusive, (int)maxExclusive);
			return result;
		}

		private static void NextChars(Random random, char[] buffer)
		{
			for (var i = 0; i < buffer.Length; ++i)
			{
				// capping to byte value here to not exceed
				// 56 bit crypto keys length requirement by
				// Apple to avoid cryptography declaration
				buffer[i] = (char) (random.Next() % 256);
			}
		}
	}
}