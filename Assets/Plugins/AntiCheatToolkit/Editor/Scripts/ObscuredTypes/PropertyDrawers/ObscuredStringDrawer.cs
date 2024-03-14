#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	using ObscuredTypes;

	using System;
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(ObscuredString))]
	internal class ObscuredStringDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			MigrateUtils.MigrateObscuredStringIfNecessary(prop);

			var hiddenChars = prop.FindPropertyRelative("hiddenChars");

			var cryptoKey = prop.FindPropertyRelative("cryptoKey");
			var inited = prop.FindPropertyRelative("inited");
			var fakeValue = prop.FindPropertyRelative("fakeValue");
			var fakeValueActive = prop.FindPropertyRelative("fakeValueActive");

			var currentCryptoKey = GetChars(cryptoKey);

			var val = string.Empty;

			if (!inited.boolValue)
			{
				if (currentCryptoKey.Length == 0)
				{
					currentCryptoKey = ObscuredString.GenerateKey();
					SetChars(cryptoKey, currentCryptoKey);
				}
				inited.boolValue = true;
				EncryptAndSetChars(val.ToCharArray(), hiddenChars, currentCryptoKey);
				fakeValue.stringValue = val;
			}
			else
			{
				var size = hiddenChars.FindPropertyRelative("Array.size");
				var showMixed = size.hasMultipleDifferentValues;

				if (!showMixed)
				{
					for (var i = 0; i < hiddenChars.arraySize; i++)
					{
						showMixed |= hiddenChars.GetArrayElementAtIndex(i).hasMultipleDifferentValues;
						if (showMixed) break;
					}
				}

				if (!showMixed)
				{
					var chars = new char[hiddenChars.arraySize];
					for (var i = 0; i < hiddenChars.arraySize; i++)
					{
						chars[i] = (char)hiddenChars.GetArrayElementAtIndex(i).intValue;
					}

					val = ObscuredString.Decrypt(chars, currentCryptoKey);
				}
				else
				{
					EditorGUI.showMixedValue = true;
				}
			}
			
			if (label.text.IndexOf('[') != -1)
			{
				var dataIndex = prop.propertyPath.IndexOf("Array.data[", StringComparison.Ordinal);

				if (dataIndex >= 0)
				{
					dataIndex += 11;

					var index = "Element " + prop.propertyPath.Substring(dataIndex, prop.propertyPath.IndexOf("]", dataIndex, StringComparison.Ordinal) - dataIndex);
					label.text = index;
				}
			}

			label = EditorGUI.BeginProperty(position, label, prop);

			EditorGUI.BeginChangeCheck();
			val = EditorGUI.TextField(position, label, val);
			if (EditorGUI.EndChangeCheck())
			{
				EncryptAndSetChars(val.ToCharArray(), hiddenChars, currentCryptoKey);
				fakeValue.stringValue = val;
				fakeValueActive.boolValue = true;
			}

			EditorGUI.showMixedValue = false;
			EditorGUI.EndProperty();
		}

		private static void EncryptAndSetChars(char[] val, SerializedProperty prop, char[] key)
		{
			var encrypted = ObscuredString.InternalEncryptDecrypt(val, key);
			SetChars(prop, encrypted);
		}

		public static byte[] GetBytesObsolete(SerializedProperty property)
		{
			var length = property.arraySize;
			var result = new byte[length];

			for (var i = 0; i < length; i++)
			{
				result[i] = (byte)property.GetArrayElementAtIndex(i).intValue;
			}

			return result;
		}
		
		public static void SetChars(SerializedProperty property, char[] array)
		{
			property.ClearArray();
			property.arraySize = array.Length;

			for (var i = 0; i < array.Length; i++)
			{
				property.GetArrayElementAtIndex(i).intValue = array[i];
			}
		}
		
		private static char[] GetChars(SerializedProperty property)
		{
			var length = property.arraySize;
			var result = new char[length];

			for (var i = 0; i < length; i++)
			{
				result[i] = (char)property.GetArrayElementAtIndex(i).intValue;
			}

			return result;
		}
	}
}