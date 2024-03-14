#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using Common;
	using UnityEditor;

	public static partial class MigrateUtils
	{
		private static bool MigrateObscuredDouble(SerializedProperty sp)
		{
			var hiddenValue = sp.FindPropertyRelative("hiddenValue");
			if (hiddenValue == null)
				return false;

			var fakeValue = sp.FindPropertyRelative("fakeValue");

			var migratedVersion = sp.FindPropertyRelative("migratedVersion");
			if (migratedVersion != null)
			{
				if (migratedVersion.stringValue == MigrationVersion)
				{
					if (!fakeValue.prefabOverride)
						return false;
				}

				migratedVersion.stringValue = MigrationVersion;
			}

			var hiddenValueOldProperty = sp.FindPropertyRelative("hiddenValueOldByte8");
			var hiddenValueOld = default(ACTkByte8);
			var oldValueExists = false;

			if (hiddenValueOldProperty?.FindPropertyRelative("b1") != null)
			{
				hiddenValueOld.b1 = (byte)hiddenValueOldProperty.FindPropertyRelative("b1").intValue;
				hiddenValueOld.b2 = (byte)hiddenValueOldProperty.FindPropertyRelative("b2").intValue;
				hiddenValueOld.b3 = (byte)hiddenValueOldProperty.FindPropertyRelative("b3").intValue;
				hiddenValueOld.b4 = (byte)hiddenValueOldProperty.FindPropertyRelative("b4").intValue;
				hiddenValueOld.b5 = (byte)hiddenValueOldProperty.FindPropertyRelative("b5").intValue;
				hiddenValueOld.b6 = (byte)hiddenValueOldProperty.FindPropertyRelative("b6").intValue;
				hiddenValueOld.b7 = (byte)hiddenValueOldProperty.FindPropertyRelative("b7").intValue;
				hiddenValueOld.b8 = (byte)hiddenValueOldProperty.FindPropertyRelative("b8").intValue;

				if (hiddenValueOld.b1 != 0 ||
					hiddenValueOld.b2 != 0 ||
					hiddenValueOld.b3 != 0 ||
					hiddenValueOld.b4 != 0 ||
					hiddenValueOld.b5 != 0 ||
					hiddenValueOld.b6 != 0 ||
					hiddenValueOld.b7 != 0 ||
					hiddenValueOld.b8 != 0)
				{
					oldValueExists = true;
				}
			}

			if (!oldValueExists)
				return false;

			var union = new LongBytesUnion {b8 = hiddenValueOld};
			union.b8.Shuffle();
			hiddenValue.longValue = union.l;

			hiddenValueOldProperty.FindPropertyRelative("b1").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b2").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b3").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b4").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b5").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b6").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b7").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b8").intValue = 0;

			return true;
		}

		private static bool MigrateObscuredFloat(SerializedProperty sp)
		{
			var hiddenValue = sp.FindPropertyRelative("hiddenValue");
			if (hiddenValue == null)
				return false;

			var fakeValue = sp.FindPropertyRelative("fakeValue");

			var migratedVersion = sp.FindPropertyRelative("migratedVersion");
			if (migratedVersion != null)
			{
				if (migratedVersion.stringValue == MigrationVersion)
				{
					if (!fakeValue.prefabOverride)
						return false;
				}

				migratedVersion.stringValue = MigrationVersion;
			}

			var hiddenValueOldProperty = sp.FindPropertyRelative("hiddenValueOldByte4");
			var hiddenValueOld = default(ACTkByte4);
			var oldValueExists = false;

			if (hiddenValueOldProperty?.FindPropertyRelative("b1") != null)
			{
				hiddenValueOld.b1 = (byte)hiddenValueOldProperty.FindPropertyRelative("b1").intValue;
				hiddenValueOld.b2 = (byte)hiddenValueOldProperty.FindPropertyRelative("b2").intValue;
				hiddenValueOld.b3 = (byte)hiddenValueOldProperty.FindPropertyRelative("b3").intValue;
				hiddenValueOld.b4 = (byte)hiddenValueOldProperty.FindPropertyRelative("b4").intValue;

				if (hiddenValueOld.b1 != 0 ||
					hiddenValueOld.b2 != 0 ||
					hiddenValueOld.b3 != 0 ||
					hiddenValueOld.b4 != 0)
				{
					oldValueExists = true;
				}
			}

			if (!oldValueExists)
				return false;

			var union = new FloatIntBytesUnion {b4 = hiddenValueOld};
			union.b4.Shuffle();
			hiddenValue.longValue = union.i;

			hiddenValueOldProperty.FindPropertyRelative("b1").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b2").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b3").intValue = 0;
			hiddenValueOldProperty.FindPropertyRelative("b4").intValue = 0;

			return true;
		}

		private static bool MigrateObscuredVector2(SerializedProperty sp)
		{
			var hiddenValue = sp.FindPropertyRelative("hiddenValue");
			if (hiddenValue == null)
				return false;

			var fakeValue = sp.FindPropertyRelative("fakeValue");

			var migratedVersion = sp.FindPropertyRelative("migratedVersion");
			if (migratedVersion != null)
			{
				if (migratedVersion.stringValue == MigrationVersion)
				{
					if (!fakeValue.prefabOverride)
						return false;
				}

				migratedVersion.stringValue = MigrationVersion;
			}

			var hiddenValueX = hiddenValue.FindPropertyRelative("x");
			var hiddenValueY = hiddenValue.FindPropertyRelative("y");

			var union = new FloatIntBytesUnion {i = hiddenValueX.intValue};
			union.b4.Shuffle();
			hiddenValueX.intValue = union.i;

			union.i = hiddenValueY.intValue;
			union.b4.Shuffle();
			hiddenValueY.intValue = union.i;

			return true;
		}

		private static bool MigrateObscuredVector3(SerializedProperty sp)
		{
			var hiddenValue = sp.FindPropertyRelative("hiddenValue");
			if (hiddenValue == null)
				return false;

			var fakeValue = sp.FindPropertyRelative("fakeValue");

			var migratedVersion = sp.FindPropertyRelative("migratedVersion");
			if (migratedVersion != null)
			{
				if (migratedVersion.stringValue == MigrationVersion)
				{
					if (!fakeValue.prefabOverride)
						return false;
				}

				migratedVersion.stringValue = MigrationVersion;
			}

			var hiddenValueX = hiddenValue.FindPropertyRelative("x");
			var hiddenValueY = hiddenValue.FindPropertyRelative("y");
			var hiddenValueZ = hiddenValue.FindPropertyRelative("z");

			var union = new FloatIntBytesUnion {i = hiddenValueX.intValue};
			union.b4.Shuffle();
			hiddenValueX.intValue = union.i;

			union.i = hiddenValueY.intValue;
			union.b4.Shuffle();
			hiddenValueY.intValue = union.i;

			union.i = hiddenValueZ.intValue;
			union.b4.Shuffle();
			hiddenValueZ.intValue = union.i;

			return true;
		}
		
		private static bool MigrateObscuredQuaternion(SerializedProperty sp)
		{
			var hiddenValue = sp.FindPropertyRelative("hiddenValue");
			if (hiddenValue == null) 
				return false;

			var fakeValue = sp.FindPropertyRelative("fakeValue");

			var migratedVersion = sp.FindPropertyRelative("migratedVersion");
			if (migratedVersion != null)
			{
				if (migratedVersion.stringValue == MigrationVersion)
				{
					if (!fakeValue.prefabOverride)
						return false;
				}

				migratedVersion.stringValue = MigrationVersion;
			}

			var hiddenValueX = hiddenValue.FindPropertyRelative("x");
			var hiddenValueY = hiddenValue.FindPropertyRelative("y");
			var hiddenValueZ = hiddenValue.FindPropertyRelative("z");
			var hiddenValueW = hiddenValue.FindPropertyRelative("w");

			var union = new FloatIntBytesUnion {i = hiddenValueX.intValue};
			union.b4.Shuffle();
			hiddenValueX.intValue = union.i;

			union.i = hiddenValueY.intValue;
			union.b4.Shuffle();
			hiddenValueY.intValue = union.i;

			union.i = hiddenValueZ.intValue;
			union.b4.Shuffle();
			hiddenValueZ.intValue = union.i;

			union.i = hiddenValueW.intValue;
			union.b4.Shuffle();
			hiddenValueW.intValue = union.i;

			return true;
		}
	}
}