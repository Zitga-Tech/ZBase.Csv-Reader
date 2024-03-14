#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	/// <summary>
	/// Controls device lock tampering sensitivity - from fully functional to full tampering ignorance.
	/// Emits DataFromAnotherDeviceDetected event when detecting data from another device.
	/// </summary>
	/// \sa DeviceLockLevel, ObscuredFile.DataFromAnotherDeviceDetected, ObscuredFilePrefs.DataFromAnotherDeviceDetected, ObscuredPrefs.DataFromAnotherDeviceDetected
	public enum DeviceLockTamperingSensitivity : byte
	{
		/// <summary>
		/// Allows reading data from another devices without detection.
		/// </summary>
		Disabled,
		
		/// <summary>
		/// Allows reading data from another devices and emits DataFromAnotherDeviceDetected event.
		/// </summary>
		Low,
		
		/// <summary>
		/// Prevents reading data from another device and emits DataFromAnotherDeviceDetected event.
		/// </summary>
		Normal
	}
}