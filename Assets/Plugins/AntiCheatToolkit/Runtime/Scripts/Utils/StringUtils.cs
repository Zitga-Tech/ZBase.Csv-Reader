#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Utils
{
	using System.Text;

	/// <summary>
	/// Contains few utility methods for string operations used by ACTk. 
	/// </summary>
	/// Not intended for usage from user code,
	/// touch at your peril since API can change and break backwards compatibility!
	public static class StringUtils
	{
		private static readonly char[] HexArray = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};

		public static byte[] CharsToBytes(char[] input)
		{
			return Encoding.UTF8.GetBytes(input);
		}

		public static byte[] StringToBytes(string input)
		{
			return input == null ? null : Encoding.UTF8.GetBytes(input);
		}

		public static char[] BytesToChars(byte[] input)
		{
			return Encoding.UTF8.GetChars(input);
		}

		public static string BytesToString(byte[] input)
		{
			return Encoding.UTF8.GetString(input);
		}

		public static string BytesToString(byte[] input, int index, int count)
		{
			return Encoding.UTF8.GetString(input, index, count);
		}

		public static string HashBytesToHexString(byte[] input)
		{
			var hexChars = new char[input.Length * 2];
			for (var i = 0; i < input.Length; i++)
			{
				var v = (input[i] ^ 144) & 0xFF;
				hexChars[i * 2] = HexArray[(uint)v >> 4];
				hexChars[i * 2 + 1] = HexArray[v & 0x0F];
			}

			return new string(hexChars);
		}
	}
}