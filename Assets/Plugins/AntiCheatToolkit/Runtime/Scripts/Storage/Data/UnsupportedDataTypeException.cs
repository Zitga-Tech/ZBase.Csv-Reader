#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;

	internal class UnsupportedDataTypeException : Exception
	{
		public UnsupportedDataTypeException(Type type):base($"Unsupported data type: {type}!")
		{ }
	}
}