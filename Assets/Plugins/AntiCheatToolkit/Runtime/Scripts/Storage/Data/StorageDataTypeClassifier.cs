#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;
	using UnityEngine;

	internal static class StorageDataTypeClassifier
	{
		private static readonly Type SByteType = typeof(sbyte);
		private static readonly Type ByteType = typeof(byte);
		private static readonly Type ShortType = typeof(short);
		private static readonly Type UShortType = typeof(ushort);
		private static readonly Type IntType = typeof(int);
		private static readonly Type UIntType = typeof(uint);
		private static readonly Type LongType = typeof(long);
		private static readonly Type ULongType = typeof(ulong);
		private static readonly Type CharType = typeof(char);
		private static readonly Type StringType = typeof(string);
		private static readonly Type FloatType = typeof(float);
		private static readonly Type DoubleType = typeof(double);
		private static readonly Type DecimalType = typeof(decimal);
		private static readonly Type BoolType = typeof(bool);
		private static readonly Type DateTimeType = typeof(DateTime);
		private static readonly Type ByteArray = typeof(byte[]);
		
		private static readonly Type Vector2Type = typeof(Vector2);
		private static readonly Type Vector2IntType = typeof(Vector2Int);
		private static readonly Type Vector3Type = typeof(Vector3);
		private static readonly Type Vector3IntType = typeof(Vector3Int);
		private static readonly Type Vector4Type = typeof(Vector4);
		private static readonly Type QuaternionType = typeof(Quaternion);
		private static readonly Type ColorType = typeof(Color);
		private static readonly Type Color32Type = typeof(Color32);
		private static readonly Type RectType = typeof(Rect);
		private static readonly Type RectIntType = typeof(RectInt);
		private static readonly Type RangeIntType = typeof(RangeInt);
		private static readonly Type Matrix4x4Type = typeof(Matrix4x4);
		private static readonly Type RayType = typeof(Ray);
		private static readonly Type Ray2DType = typeof(Ray2D);

		public static StorageDataType GetStorageDataType<T>()
		{
			var genericType = typeof(T);
			if (genericType == SByteType) return StorageDataType.SByte;
			if (genericType == ByteType) return StorageDataType.Byte;
			if (genericType == ShortType) return StorageDataType.Int16;
			if (genericType == UShortType) return StorageDataType.UInt16;
			if (genericType == IntType) return StorageDataType.Int32;
			if (genericType == UIntType) return StorageDataType.UInt32;
			if (genericType == LongType) return StorageDataType.Int64;
			if (genericType == ULongType) return StorageDataType.UInt64;
			if (genericType == CharType) return StorageDataType.Char;
			if (genericType == FloatType) return StorageDataType.Single;
			if (genericType == DoubleType) return StorageDataType.Double;
			if (genericType == DecimalType) return StorageDataType.Decimal;
			if (genericType == BoolType) return StorageDataType.Boolean;
			if (genericType == StringType) return StorageDataType.String;
			if (genericType == DateTimeType) return StorageDataType.DateTime;
			if (genericType == ByteArray) return StorageDataType.ByteArray;
			
			if (genericType == Vector2Type) return StorageDataType.Vector2;
			if (genericType == Vector2IntType) return StorageDataType.Vector2Int;
			if (genericType == Vector3Type) return StorageDataType.Vector3;
			if (genericType == Vector3IntType) return StorageDataType.Vector3Int;
			if (genericType == Vector4Type) return StorageDataType.Vector4;
			if (genericType == QuaternionType) return StorageDataType.Quaternion;
			if (genericType == ColorType) return StorageDataType.Color;
			if (genericType == Color32Type) return StorageDataType.Color32;
			if (genericType == RectType) return StorageDataType.Rect;
			if (genericType == RectIntType) return StorageDataType.RectInt;
			if (genericType == RangeIntType) return StorageDataType.RangeInt;
			if (genericType == Matrix4x4Type) return StorageDataType.Matrix4x4;
			if (genericType == RayType) return StorageDataType.Ray;
			if (genericType == Ray2DType) return StorageDataType.Ray2D;

			return StorageDataType.Unknown;
		}
	}
}