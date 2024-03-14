#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;
	using Common;
	using UnityEngine;
	using Utils;
	
	internal static class StorageDataConverter
	{
		public static Color32 BytesToColor32Legacy(byte[] data)
		{
			var encodedColor = BitConverter.ToUInt32(data, 0);;
			var a = (byte)(encodedColor >> 24);
			var r = (byte)(encodedColor >> 16);
			var g = (byte)(encodedColor >> 8);
			var b = (byte)(encodedColor >> 0);
			return new Color32(r, g, b, a);
		}
		
		public static ObscuredPrefsData GetPrefsDataFromValue<T>(T value)
		{
			var storageType = StorageDataTypeClassifier.GetStorageDataType<T>();
			var data = GetBytesFromValue(storageType, value);
			return new ObscuredPrefsData(storageType, data);
		}

		public static T GetValueFromPrefsData<T>(ObscuredPrefsData prefsData)
		{
			return GetValueFromData<T>(prefsData.type, prefsData.data);
		}

		public static T GetValueFromData<T>(byte[] data)
		{
			var type = StorageDataTypeClassifier.GetStorageDataType<T>();
			return GetValueFromData<T>(type, data);
		}

		public static T GetValueFromData<T>(StorageDataType type, byte[] data)
		{
			if (data == null || (data.Length == 0 && type != StorageDataType.String))
			{
				ACTk.ConstructErrorForSupport($"Source data for value of type {typeof(T)} is empty!" +
											  "Can't convert it to the desired type, data will be lost.");
				return default;
			}

			try
			{
				return GetValueFromBytes<T>(type, data);
			}
			catch (ArgumentOutOfRangeException e)
			{
				if (e.Message.StartsWith("Unknown"))
				{
					ACTk.PrintExceptionForSupport(
						$"Something went wrong while converting bytes to specific type {typeof(T)}", e);
				}
				else
				{
					Debug.LogError(
						$"{ACTk.LogPrefix}Something went wrong while converting bytes to specific type {typeof(T)}!\n" +
						"Please make sure you are getting value with same type you set it previously, " +
						$"e.g. load data using {nameof(ObscuredPrefs)}.{nameof(ObscuredPrefs.Get)}<long>() if " +
						$"you used {nameof(ObscuredPrefs)}.{nameof(ObscuredPrefs.Set)}<int>() to save it!");
					Debug.LogException(e);
				}
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport(
					$"Something went wrong while converting bytes to specific type {typeof(T)}", e);
			}
			
			return default;
		}
		
		private static byte[] GetBytesFromValue<T>(StorageDataType type, T value)
		{
			switch (type)
			{
				case StorageDataType.Unknown:
					throw new UnsupportedDataTypeException(typeof(T));
				case StorageDataType.SByte:
					return ConvertToBytes((sbyte)(object)value);
				case StorageDataType.Byte:
					return ConvertToBytes((byte)(object)value);
				case StorageDataType.Int16:
					return BitConverter.GetBytes((short)(object)value);
				case StorageDataType.UInt16:
					return BitConverter.GetBytes((ushort)(object)value);
				case StorageDataType.Int32:
					return BitConverter.GetBytes((int)(object)value);
				case StorageDataType.UInt32:
					return BitConverter.GetBytes((uint)(object)value);
				case StorageDataType.Int64:
					return BitConverter.GetBytes((long)(object)value);
				case StorageDataType.UInt64:
					return BitConverter.GetBytes((ulong)(object)value);
				case StorageDataType.Char:
					return BitConverter.GetBytes((char)(object)value);
				case StorageDataType.Single:
					return BitConverter.GetBytes((float)(object)value);
				case StorageDataType.Double:
					return BitConverter.GetBytes((double)(object)value);
				case StorageDataType.Decimal:
					return ConvertToBytes((decimal)(object)value);
				case StorageDataType.Boolean:
					return BitConverter.GetBytes((bool)(object)value);
				case StorageDataType.String:
					return StringUtils.StringToBytes(value as string);
				case StorageDataType.DateTime:
					return BitConverter.GetBytes(((DateTime)(object)value).Ticks);
				case StorageDataType.ByteArray:
					return value as byte[];
				case StorageDataType.Vector2:
					return ConvertToBytes((Vector2)(object)value);
				case StorageDataType.Vector2Int:
					return ConvertToBytes((Vector2Int)(object)value);
				case StorageDataType.Vector3:
					return ConvertToBytes((Vector3)(object)value);
				case StorageDataType.Vector3Int:
					return ConvertToBytes((Vector3Int)(object)value);
				case StorageDataType.Vector4:
					return ConvertToBytes((Vector4)(object)value);
				case StorageDataType.Quaternion:
					return ConvertToBytes((Quaternion)(object)value);
				case StorageDataType.Color:
					return ConvertToBytes((Color)(object)value);
				case StorageDataType.Color32:
					return ConvertToBytes((Color32)(object)value);
				case StorageDataType.Rect:
					return ConvertToBytes((Rect)(object)value);
				case StorageDataType.RectInt:
					return ConvertToBytes((RectInt)(object)value);
				case StorageDataType.RangeInt:
					return ConvertToBytes((RangeInt)(object)value);
				case StorageDataType.Matrix4x4:
					return ConvertToBytes((Matrix4x4)(object)value);
				case StorageDataType.Ray:
					return ConvertToBytes((Ray)(object)value);
				case StorageDataType.Ray2D:
					return ConvertToBytes((Ray2D)(object)value);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
		
		private static T GetValueFromBytes<T>(StorageDataType type, byte[] data)
		{
			switch (type)
			{
				case StorageDataType.Unknown:
					throw new UnsupportedDataTypeException(typeof(T));
				case StorageDataType.SByte:
					return (T)(object)ConvertToValue<sbyte>(data);
				case StorageDataType.Byte:
					return (T)(object)ConvertToValue<byte>(data);
				case StorageDataType.Int16:
					return (T)(object)BitConverter.ToInt16(data, 0);
				case StorageDataType.UInt16:
					return (T)(object)BitConverter.ToUInt16(data, 0);
				case StorageDataType.Int32:
					return (T)(object)BitConverter.ToInt32(data, 0);
				case StorageDataType.UInt32:
					return (T)(object)BitConverter.ToUInt32(data, 0);
				case StorageDataType.Single:
					return (T)(object)BitConverter.ToSingle(data, 0);
				case StorageDataType.Double:
					return (T)(object)BitConverter.ToDouble(data, 0);
				case StorageDataType.Decimal:
					return (T)(object)ConvertToValue<decimal>(data);
				case StorageDataType.Int64:
					return (T)(object)BitConverter.ToInt64(data, 0);
				case StorageDataType.UInt64:
					return (T)(object)BitConverter.ToUInt64(data, 0);
				case StorageDataType.Char:
					return (T)(object)BitConverter.ToChar(data, 0);
				case StorageDataType.Boolean:
					return (T)(object)BitConverter.ToBoolean(data, 0);
				case StorageDataType.String:
					return (T)(object)StringUtils.BytesToString(data, 0, data.Length);
				case StorageDataType.DateTime:
					return (T)(object)new DateTime(BitConverter.ToInt64(data, 0));
				case StorageDataType.ByteArray:
					return (T)(object)data;
				case StorageDataType.Vector2:
					return (T)(object)ConvertToValue<Vector2>(data);
				case StorageDataType.Vector2Int:
					return (T)(object)ConvertToValue<Vector2Int>(data);
				case StorageDataType.Vector3:
					return (T)(object)ConvertToValue<Vector3>(data);
				case StorageDataType.Vector3Int:
					return (T)(object)ConvertToValue<Vector3Int>(data);
				case StorageDataType.Vector4:
					return (T)(object)ConvertToValue<Vector4>(data);
				case StorageDataType.Quaternion:
					return (T)(object)ConvertToValue<Quaternion>(data);
				case StorageDataType.Color:
					return (T)(object)ConvertToValue<Color>(data);
				case StorageDataType.Color32:
					return (T)(object)ConvertToValue<Color32>(data);
				case StorageDataType.Rect:
					return (T)(object)ConvertToValue<Rect>(data);
				case StorageDataType.RectInt:
					return (T)(object)ConvertToValue<RectInt>(data);
				case StorageDataType.RangeInt:
					return (T)(object)ConvertToValue<RangeInt>(data);
				case StorageDataType.Matrix4x4:
					return (T)(object)ConvertToValue<Matrix4x4>(data);
				case StorageDataType.Ray:
					return (T)(object)ConvertToValue<Ray>(data);
				case StorageDataType.Ray2D:
					return (T)(object)ConvertToValue<Ray2D>(data);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown {nameof(StorageDataType)} {type}");
			}
		}
		
		private static unsafe byte[] ConvertToBytes<T>(T value) where T : unmanaged 
		{
			var pointer = (byte*)&value;
    
			var result = new byte[sizeof(T)];
			for (var i = 0; i < sizeof(T); i++) 
				result[i] = pointer[i];
    
			return result;
		}

		private static unsafe T ConvertToValue<T>(byte[] data) where T : unmanaged
		{
			var result = default(T);
			var pointer = (byte*)&result;
    
			for (var i = 0; i < sizeof(T); i++) 
				pointer[i] = data[i];
    
			return result;
		}
	}
}