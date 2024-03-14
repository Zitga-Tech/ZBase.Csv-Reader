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

	[CustomPropertyDrawer(typeof(ObscuredVector2))]
	internal class ObscuredVector2Drawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			var hiddenValue = prop.FindPropertyRelative("hiddenValue");
			var hiddenValueX = hiddenValue.FindPropertyRelative("x");
			var hiddenValueY = hiddenValue.FindPropertyRelative("y");

			var cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			var inited = prop.FindPropertyRelative("inited");
			var fakeValue = prop.FindPropertyRelative("fakeValue");
			var fakeValueActive = prop.FindPropertyRelative("fakeValueActive");

			var currentCryptoKey = cryptoKey.intValue;
			var val = Vector2.zero;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = cryptoKey.intValue = ObscuredVector2.GenerateKey();
				}
				var ev = ObscuredVector2.Encrypt(Vector2.zero, currentCryptoKey);
				hiddenValueX.intValue = ev.x;
				hiddenValueY.intValue = ev.y;
                inited.boolValue = true;
				fakeValue.vector2Value = Vector2.zero;
			}
			else
			{
				var ev = new ObscuredVector2.RawEncryptedVector2
				{
					x = hiddenValueX.intValue,
					y = hiddenValueY.intValue
				};
				val = ObscuredVector2.Decrypt(ev, currentCryptoKey);
			}

			label = EditorGUI.BeginProperty(position, label, prop);

			EditorGUI.BeginChangeCheck();
			val = EditorGUI.Vector2Field(position, label, val);
			if (EditorGUI.EndChangeCheck())
			{
				var ev = ObscuredVector2.Encrypt(val, currentCryptoKey);
				hiddenValueX.intValue = ev.x;
				hiddenValueY.intValue = ev.y;
				fakeValue.vector2Value = val;
				fakeValueActive.boolValue = true;
			}
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.wideMode ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2f;
		}
	}
}