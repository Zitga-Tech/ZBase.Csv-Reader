#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	/// <summary> Used to specify level of the device lock feature strictness. </summary>
	/// Use it to prevent cheating via 100% game saves sharing or sharing purchased in-app items for example.<br/>
	///
	/// Relies on <a href="https://docs.unity3d.com/ScriptReference/SystemInfo-deviceUniqueIdentifier.html">SystemInfo.deviceUniqueIdentifier</a> when not using custom DeviceIdHolder.DeviceId.<br/>
	/// Please note, deviceUniqueIdentifier may change in some rare cases, so one day all locked data may became inaccessible on same device.<br/>
	///
	/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly On iOS use at your peril with default DeviceId! There is no reliable way to get persistent device ID on iOS. So avoid using it or use in conjunction with DeviceIdHolder.DeviceId (see below).<br/></strong>
	/// <strong>\htmlonly<font color="7030A0">NOTE #1:</font>\endhtmlonly On iOS it tries to receive vendorIdentifier in first place, to avoid device id change while updating from iOS6 to iOS7. It leads to device ID change while updating from iOS5, but such case appears much rarer.<br/></strong>
	/// <strong>\htmlonly<font color="7030A0">NOTE #2:</font>\endhtmlonly You may use own device id via DeviceIdHolder.DeviceId property to make it more reliable and predictable. Use it to lock saves to the specified email for example.<br/></strong>
	/// <strong>\htmlonly<font color="7030A0">NOTE #3:</font>\endhtmlonly Main thread may lock up for a noticeable time while obtaining device ID first time on some devices. Consider using DeviceIdHolder.ForceLockToDeviceInit() at loading screen or other desirable stall moment of your app to prevent undesirable behavior in such cases.</strong>
	/// \sa DeviceIdHolder.ForceLockToDeviceInit(), DeviceIdHolder.DeviceId, ObscuredPrefs.DeviceLockSettings, ObscuredFilePrefs.DeviceLockSettings, DeviceLockSettings
	public enum DeviceLockLevel : byte
	{
		/// <summary>
		/// Both locked and not locked to any device data can be read and does not locks saves to the current device.
		/// </summary>
		None,

		/// <summary>
		/// Does locks to the current device and still allows reading not locked data
		/// (useful when you decided to lock your saves in one of app updates and wish to keep user data).
		/// </summary>
		Soft,

		/// <summary>
		/// Does locks to the current device and reads only locked to the current device data.
		/// This is a preferred mode, but it should be enabled right from the first app release.
		/// If you released an app without data lock consider using Soft lock or any previously saved data will not be accessible.
		/// </summary>
		Strict
	}
}