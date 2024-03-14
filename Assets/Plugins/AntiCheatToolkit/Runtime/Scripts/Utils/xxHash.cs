/*
xxHashSharp - A pure C# implementation of xxhash
Copyright (C) 2014, Seok-Ju, Yun. (https://github.com/noricube/xxHashSharp)
Specific optimization, Stream version and inlining by Dmitriy Yukhanov (https://codestage.net)
Original C Implementation Copyright (C) 2012-2014, Yann Collet. (https://code.google.com/p/xxhash/)
BSD 2-Clause License (http://www.opensource.org/licenses/bsd-license.php)

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above
      copyright notice, this list of conditions and the following
      disclaimer in the documentation and/or other materials provided
      with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
namespace CodeStage.AntiCheat.Utils
{
	using System.IO;

	/// <summary>
	/// A little bit changed xxHashSharp implementation.
	/// Original Copyright (C) 2014, Seok-Ju, Yun. (https://github.com/noricube/xxHashSharp)
	/// </summary>
	/// Not intended for usage from user code,
	/// touch at your peril since API can change and break backwards compatibility!
	public static class xxHash
	{
		private const uint PRIME32_1 = 2654435761U;
		private const uint PRIME32_2 = 2246822519U;
		private const uint PRIME32_3 = 3266489917U;
		private const uint PRIME32_4 = 668265263U;
		private const uint PRIME32_5 = 374761393U;

		public static uint CalculateHash(byte[] buf, int len, uint seed)
		{
			uint h32;
			int index = 0;

			if (len >= 16)
			{
				int limit = len - 16;
				uint v1 = seed + PRIME32_1 + PRIME32_2;
				uint v2 = seed + PRIME32_2;
				uint v3 = seed;
				uint v4 = seed - PRIME32_1;

				do
				{
					uint read_value = (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24);
					v1 += read_value * PRIME32_2;
					v1 = (v1 << 13) | (v1 >> 19);
					v1 *= PRIME32_1;

					read_value = (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24);
					v2 += read_value * PRIME32_2;
					v2 = (v2 << 13) | (v2 >> 19);
					v2 *= PRIME32_1;

					read_value = (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24);
					v3 += read_value * PRIME32_2;
					v3 = (v3 << 13) | (v3 >> 19);
					v3 *= PRIME32_1;

					read_value = (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24);
					v4 += read_value * PRIME32_2;
					v4 = (v4 << 13) | (v4 >> 19);
					v4 *= PRIME32_1;

				} while (index <= limit);

				h32 = ((v1 << 1) | (v1 >> 31)) + ((v2 << 7) | (v2 >> 25)) + ((v3 << 12) | (v3 >> 20)) + ((v4 << 18) | (v4 >> 14));
			}
			else
			{
				h32 = seed + PRIME32_5;
			}

			h32 += (uint)len;

			while (index <= len - 4)
			{
				h32 += (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24) * PRIME32_3;
				h32 = ((h32 << 17) | (h32 >> 15)) * PRIME32_4;
			}

			while (index < len)
			{
				h32 += buf[index] * PRIME32_5;
				h32 = ((h32 << 11) | (h32 >> 21)) * PRIME32_1;
				index++;
			}

			h32 ^= h32 >> 15;
			h32 *= PRIME32_2;
			h32 ^= h32 >> 13;
			h32 *= PRIME32_3;
			h32 ^= h32 >> 16;

			return h32;
		}
		
		public static uint CalculateHash(Stream buf, int len, uint seed)
		{
			uint h32;
			var index = 0;
			buf.Position = 0;

			if (len >= 16)
			{
				var limit = len - 16;
				var v1 = seed + PRIME32_1 + PRIME32_2;
				var v2 = seed + PRIME32_2;
				var v3 = seed;
				var v4 = seed - PRIME32_1;

				do
				{
					var read_value = (uint)(buf.ReadByte() | buf.ReadByte() << 8 | buf.ReadByte() << 16 | buf.ReadByte() << 24);
					index += 4;
					
					v1 += read_value * PRIME32_2;
					v1 = (v1 << 13) | (v1 >> 19);
					v1 *= PRIME32_1;

					read_value = (uint)(buf.ReadByte() | buf.ReadByte() << 8 | buf.ReadByte() << 16 | buf.ReadByte() << 24);
					index += 4;
					
					v2 += read_value * PRIME32_2;
					v2 = (v2 << 13) | (v2 >> 19);
					v2 *= PRIME32_1;

					read_value = (uint)(buf.ReadByte() | buf.ReadByte() << 8 | buf.ReadByte() << 16 | buf.ReadByte() << 24);
					index += 4;
					v3 += read_value * PRIME32_2;
					v3 = (v3 << 13) | (v3 >> 19);
					v3 *= PRIME32_1;

					read_value = (uint)(buf.ReadByte() | buf.ReadByte() << 8 | buf.ReadByte() << 16 | buf.ReadByte() << 24);
					index += 4;
					v4 += read_value * PRIME32_2;
					v4 = (v4 << 13) | (v4 >> 19);
					v4 *= PRIME32_1;

				} while (index <= limit);

				h32 = ((v1 << 1) | (v1 >> 31)) + ((v2 << 7) | (v2 >> 25)) + ((v3 << 12) | (v3 >> 20)) + ((v4 << 18) | (v4 >> 14));
			}
			else
			{
				h32 = seed + PRIME32_5;
			}

			h32 += (uint)len;

			while (index <= len - 4)
			{
				h32 += (uint)(buf.ReadByte() | buf.ReadByte() << 8 | buf.ReadByte() << 16 | buf.ReadByte() << 24) * PRIME32_3;
				index += 4;
				h32 = ((h32 << 17) | (h32 >> 15)) * PRIME32_4;
			}

			while (index < len)
			{
				h32 += (byte)buf.ReadByte() * PRIME32_5;
				h32 = ((h32 << 11) | (h32 >> 21)) * PRIME32_1;
				index++;
			}

			h32 ^= h32 >> 15;
			h32 *= PRIME32_2;
			h32 ^= h32 >> 13;
			h32 *= PRIME32_3;
			h32 ^= h32 >> 16;
			
			buf.Position = 0;

			return h32;
		}
	}
}