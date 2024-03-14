#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;

	/// <summary> Contains ObscuredFile read operation results. </summary>
	public readonly struct ObscuredFileReadResult
	{
		/// <summary>
		/// Returns true in case #Data is not null and #Error's ErrorCode is ObscuredFileErrorCode.NoError, returns false otherwise.
		/// </summary>
		/// <strong>\htmlonly<font color="7030A0">NOTE:</font>\endhtmlonly</strong> it will be
		/// false if data is not genuine or was loaded from another device even if Data itself was read successfully and not null.
		/// Listen to the NotGenuineDataDetected and DataFromAnotherDeviceDetected events
		/// (at ObscuredFile or ObscuredFilePrefs) or check #CheatingDetected, #DataIsNotGenuine and #DataFromAnotherDevice
		/// properties explicitly to react on the possible cheating.
		public bool Success => IsValid && Data != null && !CheatingDetected && Error.ErrorCode == ObscuredFileErrorCode.NoError;
		
		/// <summary>
		/// Contains read bytes. Will be null if data was damaged, file does not exists or device lock feature prevented data read.
		/// </summary>
		public byte[] Data { get; }

		/// <summary>
		/// Indicates either #DataIsNotGenuine or #DataFromAnotherDevice is true.
		/// </summary>
		public bool CheatingDetected => DataIsNotGenuine || DataFromAnotherDevice;
		
		/// <summary>
		/// Returns true if saved data has correct header but signature does not matches file contents. Returns false otherwise.
		/// </summary>
		public bool DataIsNotGenuine { get; }
		
		/// <summary>
		/// Returns true if device lock feature detected data from another device.
		/// </summary>
		public bool DataFromAnotherDevice { get; }
		
		/// <summary>
		/// Contains specific error in case #Success is not true but #IsValid is true.
		/// </summary>
		public ObscuredFileError Error { get; }

		/// <summary>
		/// Returns true if this struct was filled with actual data, otherwise will stay false.
		/// </summary>
		public bool IsValid { get; }
		
		internal ObscuredFileReadResult(byte[] data, bool dataIsNotGenuine, bool dataFromAnotherDevice)
		{
			Data = data;
			DataIsNotGenuine = dataIsNotGenuine;
			DataFromAnotherDevice = dataFromAnotherDevice;
			Error = default;
			IsValid = true;
		}
		
		private ObscuredFileReadResult(ObscuredFileError error)
		{
			Data = default;
			DataIsNotGenuine = default;
			DataFromAnotherDevice = default;
			Error = error;
			IsValid = true;
		}
		
		internal static ObscuredFileReadResult FromError(Exception exception)
		{
			return new ObscuredFileReadResult(new ObscuredFileError(exception));
		}

		internal static ObscuredFileReadResult FromError(ObscuredFileErrorCode errorCode)
		{
			return new ObscuredFileReadResult(new ObscuredFileError(errorCode));
		}

		/// <summary>
		/// Returns contents of this operation result.
		/// </summary>
		/// <returns>Human-readable operation result.</returns>
		public override string ToString()
		{
			return $"{nameof(IsValid)}: {IsValid}\n" +
				   $"Read data length: {Data?.Length ?? 0}\n" +
				   $"{nameof(DataIsNotGenuine)}: {DataIsNotGenuine}\n" +
				   $"{nameof(DataFromAnotherDevice)}: {DataFromAnotherDevice}\n" +
				   $"{nameof(Error)}: {Error}";
		}
	}
}