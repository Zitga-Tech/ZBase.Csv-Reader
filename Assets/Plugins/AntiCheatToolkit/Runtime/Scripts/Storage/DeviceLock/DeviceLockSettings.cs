#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	/// <summary>
	/// Controls Device Lock feature settings.
	/// </summary>
	public class DeviceLockSettings
	{
		/// <summary>
		/// Allows locking saved data to the current device.
		/// </summary>
		/// Read more in #DeviceLockLevel description.
		/// \sa Sensitivity
		public DeviceLockLevel Level { get; set; }

		/// <summary>
		/// Controls device lock tampering detection sensitivity.
		/// </summary>
		/// Read more in #DeviceLockTamperingSensitivity description.
		/// \sa Level
		public DeviceLockTamperingSensitivity Sensitivity { get; set; }
		
		/// <summary>
		/// Creates instance with custom settings.
		/// </summary>
		public DeviceLockSettings(DeviceLockLevel level = DeviceLockLevel.None, DeviceLockTamperingSensitivity sensitivity = DeviceLockTamperingSensitivity.Normal)
		{
			Level = level;
			Sensitivity = sensitivity;
		}
	}
}