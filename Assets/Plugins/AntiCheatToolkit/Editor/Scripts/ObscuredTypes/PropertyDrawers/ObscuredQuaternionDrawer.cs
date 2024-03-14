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

	[CustomPropertyDrawer(typeof(ObscuredQuaternion))]
	internal class ObscuredQuaternionDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			var hiddenValue = prop.FindPropertyRelative("hiddenValue");
			var hiddenValueX = hiddenValue.FindPropertyRelative("x");
			var hiddenValueY = hiddenValue.FindPropertyRelative("y");
			var hiddenValueZ = hiddenValue.FindPropertyRelative("z");
			var hiddenValueW = hiddenValue.FindPropertyRelative("w");

			var cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			var inited = prop.FindPropertyRelative("inited");
			var fakeValue = prop.FindPropertyRelative("fakeValue");
			var fakeValueActive = prop.FindPropertyRelative("fakeValueActive");

			var currentCryptoKey = cryptoKey.intValue;
			var val = Quaternion.identity;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = cryptoKey.intValue = ObscuredQuaternion.GenerateKey();
				}
				var ev = ObscuredQuaternion.Encrypt(Quaternion.identity, currentCryptoKey);
				hiddenValueX.intValue = ev.x;
				hiddenValueY.intValue = ev.y;
				hiddenValueZ.intValue = ev.z;
				hiddenValueW.intValue = ev.w;
				inited.boolValue = true;

				fakeValue.quaternionValue = Quaternion.identity;
			}
			else
			{
				var ev = new ObscuredQuaternion.RawEncryptedQuaternion
				{
					x = hiddenValueX.intValue,
					y = hiddenValueY.intValue,
					z = hiddenValueZ.intValue,
					w = hiddenValueW.intValue
				};
				val = ObscuredQuaternion.Decrypt(ev, currentCryptoKey);
			}

			label = EditorGUI.BeginProperty(position, label, prop);
			EditorGUI.BeginChangeCheck();
			val = Vector4ToQuaternion(EditorGUI.Vector4Field(position, label, QuaternionToVector4(val)));
			if (EditorGUI.EndChangeCheck())
			{
				var ev = ObscuredQuaternion.Encrypt(val, currentCryptoKey);
				hiddenValueX.intValue = ev.x;
				hiddenValueY.intValue = ev.y;
				hiddenValueZ.intValue = ev.z;
				hiddenValueW.intValue = ev.w;

				fakeValue.quaternionValue = val;
				fakeValueActive.boolValue = true;
			}
			EditorGUI.EndProperty();
        }

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.wideMode ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2f;
		}

		private Vector4 QuaternionToVector4(Quaternion value)
		{
			return new Vector4(value.x, value.y, value.z, value.w);
		}
		
		private Quaternion Vector4ToQuaternion(Vector4 value)
		{
			return new Quaternion(value.x, value.y, value.z, value.w);
		}
	}
}