#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;

	/// <summary> Contains ObscuredFile write operation results. </summary>
	public readonly struct ObscuredFileWriteResult
	{
		/// <summary> Returns true in case operation had no errors. </summary>
		public bool Success => IsValid && Error.ErrorCode == ObscuredFileErrorCode.NoError;
		
		/// <summary> Contains specific error in case #Success is not true but #IsValid is true. </summary>
		public ObscuredFileError Error { get; }
		
		/// <summary>
		/// Returns true if this struct was filled with actual data, otherwise will stay false.
		/// </summary>
		public bool IsValid { get; }
		
		internal ObscuredFileWriteResult(ObscuredFileErrorCode result)
		{
			Error = new ObscuredFileError(result);
			IsValid = true;
		}
		
		private ObscuredFileWriteResult(ObscuredFileError error)
		{
			Error = error;
			IsValid = true;
		}
		
		internal static ObscuredFileWriteResult FromError(Exception exception)
		{
			return new ObscuredFileWriteResult(new ObscuredFileError(exception));
		}

		internal static ObscuredFileWriteResult FromError(ObscuredFileErrorCode errorCode)
		{
			return new ObscuredFileWriteResult(new ObscuredFileError(errorCode));
		}
		
		/// <summary>
		/// Returns contents of this operation result.
		/// </summary>
		/// <returns>Human-readable operation result.</returns>
		public override string ToString()
		{
			return $"{nameof(IsValid)}: {IsValid}\n" +
				   $"{nameof(Error)}: {Error}";
		}
	}
}