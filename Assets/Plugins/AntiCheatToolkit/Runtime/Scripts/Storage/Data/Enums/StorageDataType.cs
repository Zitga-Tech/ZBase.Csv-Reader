#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	/// <summary>
	/// Data types supported by ObscuredPrefs and ObscuredFilePrefs.
	/// </summary>
	public enum StorageDataType : byte
	{
		/// <summary>Reserved for unsupported types.</summary>
		Unknown = 0,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.sbyte">System.SByte</a></summary>
		SByte = 1,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.byte">System.Byte</a></summary>
		Byte = 2,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.int16">System.Int16</a></summary>
		Int16 = 3,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.uint16">System.UInt16</a></summary>
		UInt16 = 4,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.int32">System.Int32</a></summary>
		Int32 = 5,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.uint32">System.UInt32</a></summary>
		UInt32 = 10,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.int64">System.Int64</a></summary>
		Int64 = 30,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.uint64">System.UInt64</a></summary>
		UInt64 = 32,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.char">System.Char</a></summary>
		Char = 33,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.single">System.Single</a></summary>
		Single = 20,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.double">System.Double</a></summary>
		Double = 25,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.decimal">System.Decimal</a></summary>
		Decimal = 27,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.boolean">System.Boolean</a></summary>
		Boolean = 35,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.string">System.String</a></summary>
		String = 15,
		
		/// <summary><a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.datetime">System.DateTime</a></summary>
		DateTime = 37,
		
		/// <summary>Array of <a target="_blank" href="https://docs.microsoft.com/en-us/dotnet/api/system.byte">System.Byte</a></summary>
		ByteArray = 40,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Vector2.html">UnityEngine.Vector2</a></summary>
		Vector2 = 45,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Vector2Int.html">UnityEngine.Vector2Int</a></summary>
		Vector2Int = 47,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Vector3.html">UnityEngine.Vector3</a></summary>
		Vector3 = 50,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Vector3Int.html">UnityEngine.Vector3Int</a></summary>
		Vector3Int = 51,		
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Vector4.html">UnityEngine.Vector4</a></summary>
		Vector4 = 53,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Quaternion.html">UnityEngine.Quaternion</a></summary>
		Quaternion = 55,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Color.html">UnityEngine.Color</a></summary>
		Color = 60,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Color32.html">UnityEngine.Color32</a></summary>
		Color32 = 62,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Rect.html">UnityEngine.Rect</a></summary>
		Rect = 65,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/RectInt.html">UnityEngine.RectInt</a></summary>
		RectInt = 67,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/RangeInt.html">UnityEngine.RangeInt</a></summary>
		RangeInt = 70,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Matrix4x4.html">UnityEngine.Matrix4x4</a></summary>
		Matrix4x4 = 78,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Ray.html">UnityEngine.Ray</a></summary>
		Ray = 80,
		
		/// <summary><a target="_blank" href="https://docs.unity3d.com/ScriptReference/Ray2D.html">UnityEngine.Ray2D</a></summary>
		Ray2D = 83,
	}
}