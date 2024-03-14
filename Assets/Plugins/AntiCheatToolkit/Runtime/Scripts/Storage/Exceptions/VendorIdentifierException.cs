#if UNITY_IPHONE

#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using Common;

	internal class VendorIdentifierException : BackgroundThreadAccessException
	{
		public VendorIdentifierException() : base($"{nameof(UnityEngine)}." +
												  $"{nameof(UnityEngine.iOS)}." +
												  $"{nameof(UnityEngine.iOS.Device)}." +
												  $"{nameof(UnityEngine.iOS.Device.vendorIdentifier)}")
		{
		}
	}
}

#endif