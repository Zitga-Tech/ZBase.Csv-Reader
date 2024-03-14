#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;
	using UnityEngine;

	public static partial class ObscuredPrefs
	{
		#region int
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetInt(string key, int value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static int GetInt(string key, int defaultValue = 0)
		{
			return Get(key, defaultValue);
		}
		#endregion
		
		#region uint
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetUInt(string key, uint value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static uint GetUInt(string key, uint defaultValue = 0)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region string
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetString(string key, string value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static string GetString(string key, string defaultValue = "")
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region float
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetFloat(string key, float value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static float GetFloat(string key, float defaultValue = 0f)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region double
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetDouble(string key, double value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static double GetDouble(string key, double defaultValue = 0)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region decimal
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetDecimal(string key, decimal value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static decimal GetDecimal(string key, decimal defaultValue = 0)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region long
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetLong(string key, long value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static long GetLong(string key, long defaultValue = 0)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region ulong
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetULong(string key, ulong value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static ulong GetULong(string key, ulong defaultValue = 0)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region bool
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetBool(string key, bool value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static bool GetBool(string key, bool defaultValue = false)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region byte[]
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetByteArray(string key, byte[] value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static byte[] GetByteArray(string key, byte defaultValue = 0, int defaultLength = 0)
		{
			var encryptedKey = EncryptKey(key);
			var encrypted = GetEncryptedPrefsString(key, encryptedKey);

			if (encrypted == RawNotFound)
			{
				return ConstructByteArray(defaultValue, defaultLength);
			}

			return DecryptByteArrayValue(key, encrypted, defaultValue, defaultLength);
		}

		private static byte[] ConstructByteArray(byte value, int length)
		{
			var bytes = new byte[length];
			for (var i = 0; i < length; i++)
			{
				bytes[i] = value;
			}
			return bytes;
		}
		#endregion

		#region Vector2
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetVector2(string key, Vector2 value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static Vector2 GetVector2(string key)
		{
			return GetVector2(key, Vector2.zero);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static Vector2 GetVector2(string key, Vector2 defaultValue)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region Vector3
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetVector3(string key, Vector3 value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static Vector3 GetVector3(string key)
		{
			return GetVector3(key, Vector3.zero);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static Vector3 GetVector3(string key, Vector3 defaultValue)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region Quaternion
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetQuaternion(string key, Quaternion value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static Quaternion GetQuaternion(string key)
		{
			return GetQuaternion(key, Quaternion.identity);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static Quaternion GetQuaternion(string key, Quaternion defaultValue)
		{
			return Get(key, defaultValue);
		}
		#endregion

		#region Color32
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates. Be careful with Color and Color32 difference!", false)]
		public static void SetColor(string key, Color32 value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates. Be careful with Color and Color32 difference!", false)]
		public static Color32 GetColor(string key)
		{
			return GetColor(key, new Color32(0,0,0,1));
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates. Be careful with Color and Color32 difference!", false)]
		public static Color32 GetColor(string key, Color32 defaultValue)
		{
			return Get(key, defaultValue);
		}

		#endregion

		#region Rect
		[Obsolete("Please use generic " + nameof(Set) + " instead. This API will be removed in next updates.", false)]
		public static void SetRect(string key, Rect value)
		{
			Set(key, value);
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static Rect GetRect(string key)
		{
			return GetRect(key, new Rect(0,0,0,0));
		}

		[Obsolete("Please use generic " + nameof(Get) + " instead. This API will be removed in next updates.", false)]
		public static Rect GetRect(string key, Rect defaultValue)
		{
			return Get(key, defaultValue);
		}

		#endregion
	}
}