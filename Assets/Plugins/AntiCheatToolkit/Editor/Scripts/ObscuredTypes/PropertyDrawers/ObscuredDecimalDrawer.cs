#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	using Common;
	using ObscuredTypes;

	using System.Globalization;
	using System.Runtime.InteropServices;
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(ObscuredDecimal))]
	internal class ObscuredDecimalDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			var hiddenValue = prop.FindPropertyRelative("hiddenValue");
			var hiddenValue1 = hiddenValue.FindPropertyRelative("b1");
			var hiddenValue2 = hiddenValue.FindPropertyRelative("b2");
			var hiddenValue3 = hiddenValue.FindPropertyRelative("b3");
			var hiddenValue4 = hiddenValue.FindPropertyRelative("b4");
			var hiddenValue5 = hiddenValue.FindPropertyRelative("b5");
			var hiddenValue6 = hiddenValue.FindPropertyRelative("b6");
			var hiddenValue7 = hiddenValue.FindPropertyRelative("b7");
			var hiddenValue8 = hiddenValue.FindPropertyRelative("b8");
			var hiddenValue9 = hiddenValue.FindPropertyRelative("b9");
			var hiddenValue10 = hiddenValue.FindPropertyRelative("b10");
			var hiddenValue11 = hiddenValue.FindPropertyRelative("b11");
			var hiddenValue12 = hiddenValue.FindPropertyRelative("b12");
			var hiddenValue13 = hiddenValue.FindPropertyRelative("b13");
			var hiddenValue14 = hiddenValue.FindPropertyRelative("b14");
			var hiddenValue15 = hiddenValue.FindPropertyRelative("b15");
			var hiddenValue16 = hiddenValue.FindPropertyRelative("b16");

			var cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			var inited = prop.FindPropertyRelative("inited");
			//SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
			var fakeValueActive = prop.FindPropertyRelative("fakeValueActive");

			var currentCryptoKey = cryptoKey.longValue;

			var union = new DecimalBytesUnion();
			decimal val = 0;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = cryptoKey.longValue = ObscuredDecimal.GenerateKey();
				}
				inited.boolValue = true;

				union.d = ObscuredDecimal.Encrypt(0, currentCryptoKey);

				hiddenValue1.intValue = union.b16.b1;
				hiddenValue2.intValue = union.b16.b2;
				hiddenValue3.intValue = union.b16.b3;
				hiddenValue4.intValue = union.b16.b4;
				hiddenValue5.intValue = union.b16.b5;
				hiddenValue6.intValue = union.b16.b6;
				hiddenValue7.intValue = union.b16.b7;
				hiddenValue8.intValue = union.b16.b8;
				hiddenValue9.intValue = union.b16.b9;
				hiddenValue10.intValue = union.b16.b10;
				hiddenValue11.intValue = union.b16.b11;
				hiddenValue12.intValue = union.b16.b12;
				hiddenValue13.intValue = union.b16.b13;
				hiddenValue14.intValue = union.b16.b14;
				hiddenValue15.intValue = union.b16.b15;
				hiddenValue16.intValue = union.b16.b16;
			}
			else
			{
				union.b16.b1 = (byte)hiddenValue1.intValue;
				union.b16.b2 = (byte)hiddenValue2.intValue;
				union.b16.b3 = (byte)hiddenValue3.intValue;
				union.b16.b4 = (byte)hiddenValue4.intValue;
				union.b16.b5 = (byte)hiddenValue5.intValue;
				union.b16.b6 = (byte)hiddenValue6.intValue;
				union.b16.b7 = (byte)hiddenValue7.intValue;
				union.b16.b8 = (byte)hiddenValue8.intValue;
				union.b16.b9 = (byte)hiddenValue9.intValue;
				union.b16.b10 = (byte)hiddenValue10.intValue;
				union.b16.b11 = (byte)hiddenValue11.intValue;
				union.b16.b12 = (byte)hiddenValue12.intValue;
				union.b16.b13 = (byte)hiddenValue13.intValue;
				union.b16.b14 = (byte)hiddenValue14.intValue;
				union.b16.b15 = (byte)hiddenValue15.intValue;
				union.b16.b16 = (byte)hiddenValue16.intValue;

				val = ObscuredDecimal.Decrypt(union.d, currentCryptoKey);
			}

			label = EditorGUI.BeginProperty(position, label, prop);

			EditorGUI.BeginChangeCheck();
			decimal.TryParse(EditorGUI.TextField(position, label, val.ToString(CultureInfo.InvariantCulture)), out val);
			if (EditorGUI.EndChangeCheck())
			{
				union.d = ObscuredDecimal.Encrypt(val, currentCryptoKey);

				hiddenValue1.intValue = union.b16.b1;
				hiddenValue2.intValue = union.b16.b2;
				hiddenValue3.intValue = union.b16.b3;
				hiddenValue4.intValue = union.b16.b4;
				hiddenValue5.intValue = union.b16.b5;
				hiddenValue6.intValue = union.b16.b6;
				hiddenValue7.intValue = union.b16.b7;
				hiddenValue8.intValue = union.b16.b8;
				hiddenValue9.intValue = union.b16.b9;
				hiddenValue10.intValue = union.b16.b10;
				hiddenValue11.intValue = union.b16.b11;
				hiddenValue12.intValue = union.b16.b12;
				hiddenValue13.intValue = union.b16.b13;
				hiddenValue14.intValue = union.b16.b14;
				hiddenValue15.intValue = union.b16.b15;
				hiddenValue16.intValue = union.b16.b16;

				//fakeValue.doubleValue = 0;
				fakeValueActive.boolValue = false;
			}

			EditorGUI.EndProperty();
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct DecimalBytesUnion
		{
			[FieldOffset(0)]
			public decimal d;

			[FieldOffset(0)]
			public ACTkByte16 b16;
		}
	}
}