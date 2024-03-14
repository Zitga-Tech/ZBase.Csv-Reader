#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	using ObscuredTypes;

	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(ObscuredDouble))]
	internal class ObscuredDoubleDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			var hiddenValue = prop.FindPropertyRelative("hiddenValue");

			var cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			var inited = prop.FindPropertyRelative("inited");
			var fakeValue = prop.FindPropertyRelative("fakeValue");
			var fakeValueActive = prop.FindPropertyRelative("fakeValueActive");

			var currentCryptoKey = cryptoKey.longValue;

			double val = 0;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = cryptoKey.longValue = ObscuredDouble.GenerateKey();
				}

				inited.boolValue = true;

				hiddenValue.longValue = ObscuredDouble.Encrypt(0, currentCryptoKey);
			}
			else
			{
				val = ObscuredDouble.Decrypt(hiddenValue.longValue, currentCryptoKey);
			}

			label = EditorGUI.BeginProperty(position, label, prop);

			EditorGUI.BeginChangeCheck();
			val = EditorGUI.DoubleField(position, label, val);
			if (EditorGUI.EndChangeCheck())
			{
				hiddenValue.longValue = ObscuredDouble.Encrypt(val, currentCryptoKey);
				fakeValue.doubleValue = val;
				fakeValueActive.boolValue = true;
			}
			EditorGUI.EndProperty();
		}
	}
}