#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;

	/// <summary> Possible error codes for the file read operation. </summary>
	public enum ObscuredFileErrorCode : byte
	{
		/// <summary> Indicates no errors happened. </summary>
		NoError = 0,
		/// <summary> Indicates file to read from wasn't found. </summary>
		FileNotFound = 5,
		/// <summary> Indicates file header was damaged. </summary>
		FileDamaged = 10,
		/// <summary>
		/// Indicates read data was not locked but you are using
		/// DeviceLockLevel.Strict which prevents reading such files.
		/// </summary>
		DataIsNotLocked = 15,
		/// <summary> Indicates used tool wasn't properly initialized. See error logs for more information. </summary>
		NotInitialized = 20,
		/// <summary> Indicates some other exception occured, see ObscuredFileError.Exception for details. </summary>
		OtherException = 250,
	}
	
	/// <summary> ObscuredFile-related errors container. </summary>
	public readonly struct ObscuredFileError
	{
		/// <summary>
		/// Represents error code. If there was no error it will equal to ObscuredFileErrorCode.NoError.
		/// </summary>
		public ObscuredFileErrorCode ErrorCode { get; }
		
		/// <summary>
		/// Contains exception details if ErrorCode equals to the ObscuredFileErrorCode.OtherException.
		/// </summary>
		/// Can be null.
		public Exception Exception { get; }

		internal ObscuredFileError(ObscuredFileErrorCode code)
		{
			ErrorCode = code;
			Exception = null;
		}
		
		internal ObscuredFileError(Exception exception)
		{
			ErrorCode = ObscuredFileErrorCode.OtherException;
			Exception = exception;
		}
		
		/// <summary> Returns contents of this error. </summary>
		/// <returns>Human-readable error information.</returns>
		public override string ToString()
		{
			switch (ErrorCode)
			{
				case ObscuredFileErrorCode.NoError:
					return "No error";
				case ObscuredFileErrorCode.OtherException:
					return Exception.ToString();
				default:
					return $"ErrorCode: {ErrorCode}";
			}
		}
	}
}