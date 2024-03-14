#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Utils
{
	using System;

	internal static class Base64Utils
	{
		public static string FromBase64ToString(string value)
		{
			return StringUtils.BytesToString(Convert.FromBase64String(value));
		}

		public static char[] FromBase64ToChars(string value)
		{
			return StringUtils.BytesToChars(Convert.FromBase64String(value));
		}

		public static string ToBase64(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException(nameof(value));
			
			return Convert.ToBase64String(StringUtils.StringToBytes(value));
		}

		public static string ToBase64(char[] value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			
			return Convert.ToBase64String(StringUtils.CharsToBytes(value));
		}

	}
}