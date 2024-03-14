#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.EditorCommon.Tools
{
	using System;
	using UnityEditor;
	using UnityEngine;
	
	internal static class CSColorTools
	{
		internal enum ColorKind
		{
			Green,
			Red,
			Purple
		}
		
		public const string GreenHex = "02C85F";
		public const string GreenDarkHex = "02981F";
		public const string PurpleHex = "A76ED1";
		public const string PurpleDarkHex = "7030A0";
		public const string RedHex = "FF4040";
		public const string RedAltHex = "FF6060";
		public const string RedDarkHex = "FF1010";
		public const string BrightGreyHex = "E5E5E5";

		public readonly static Color32 Green = new Color32(2, 200, 95, 255);
		public readonly static Color32 GreenDark = new Color32(2, 152, 31, 255);
		public readonly static Color32 Purple = new Color32(167, 110, 209, 255);
		public readonly static Color32 PurpleDark = new Color32(112, 48, 160, 255);
		public readonly static Color32 RedAlt = new Color32(255, 96, 96, 255);
		public readonly static Color32 RedDark = new Color32(255, 16, 16, 255);
		public readonly static Color32 BrightGrey = new Color32(229, 229, 229, 255);
		
		public static string EditorGreenHex
		{
			get
			{
				return EditorGUIUtility.isProSkin ? GreenHex : GreenDarkHex;
			}
		}
		
		public static string EditorPurpleHex
		{
			get
			{
				return EditorGUIUtility.isProSkin ? PurpleHex : PurpleDarkHex;
			}
		}
		
		public static string EditorRedHex
		{
			get
			{
				return EditorGUIUtility.isProSkin ? RedAltHex : RedDarkHex;
			}
		}
		
		public static Color EditorGreen
		{
			get
			{
				return EditorGUIUtility.isProSkin ? Green : GreenDark;
			}
		}

		public static Color EditorPurple
		{
			get
			{
				return EditorGUIUtility.isProSkin ? Purple : PurpleDark;
			}
		}

		public static Color EditorRed
		{
			get
			{
				return EditorGUIUtility.isProSkin ? RedAlt : RedDark;
			}
		}

		public static Color DimmedColor
		{
			get
			{
				return ChangeAlpha(GUI.skin.label.normal.textColor, 150);
			}
		}
		
		public static Color BrightGreyDimmed
		{
			get
			{
				return ChangeAlpha(BrightGrey, 150);
			}
		}

		public static Color GreenColor
		{
			get
			{
				return LerpToGreen(GUI.skin.label.normal.textColor, 0.3f);
			}
		}

		public static Color RedColor
		{
			get
			{
				return LerpToRed(GUI.skin.label.normal.textColor, 0.3f);
			}
		}

		public static Color BackgroundGreenTint
		{
			get
			{
				return EditorGUIUtility.isProSkin ? new Color32(0, 255, 0, 150) : new Color32(0, 255, 0, 30);
			}
		}

		public static Color BackgroundRedTint
		{
			get
			{
				return EditorGUIUtility.isProSkin ? new Color32(255, 0, 0, 150) : new Color32(255, 0, 0, 30);
			}
		}
		
		public static string WrapBool(bool value)
		{
			return WrapString(value.ToString(), value ? ColorKind.Green : ColorKind.Red);
		}
		
		public static string WrapString(string inputGood, string inputBad, bool good)
		{
			return WrapString(good ? inputGood : inputBad, good ? ColorKind.Green : ColorKind.Red);
		}
		
		public static string WrapString(string input, bool good)
		{
			return WrapString(input, good ? ColorKind.Green : ColorKind.Red);
		}
		
		public static string WrapString(string input, ColorKind colorKind)
		{
			switch (colorKind)
			{
				case ColorKind.Green:
					return WrapString(input, EditorGreenHex);
				case ColorKind.Red:
					return WrapString(input, EditorRedHex);
				case ColorKind.Purple:
					return WrapString(input, EditorPurpleHex);
				default:
					throw new ArgumentOutOfRangeException("colorKind", colorKind, null);
			}
		}
		
		public static string WrapString(string input, Color color)
		{
			var colorString = ColorUtility.ToHtmlStringRGBA(color);
			return WrapString(input, colorString);
		}

		// color argument should be in rrggbbaa format or match standard html color name, without '#'
		public static string WrapString(string input, string color)
		{
			return "<color=#" + color + ">" + input + "</color>";
		}

		public static Color32 LerpToRed(Color32 inValue, float greenAmountPercent)
		{
			return Color.Lerp(inValue, Color.red, greenAmountPercent);
		}

		public static Color32 LerpToGreen(Color32 inValue, float greenAmountPercent)
		{
			return Color.Lerp(inValue, Color.green, greenAmountPercent);
		}
		
		public static Color32 LerpToYellow(Color32 inValue, float greenAmountPercent)
		{
			return Color.Lerp(inValue, Color.yellow, greenAmountPercent);
		}

		private static Color32 ChangeAlpha(Color32 inValue, byte alphaValue)
		{
			inValue.a = alphaValue;
			return inValue;
		}
	}
}