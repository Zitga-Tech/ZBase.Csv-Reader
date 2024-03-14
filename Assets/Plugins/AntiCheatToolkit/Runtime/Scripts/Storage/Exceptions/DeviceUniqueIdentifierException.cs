#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using Common;
	using UnityEngine;

	internal class DeviceUniqueIdentifierException : BackgroundThreadAccessException
	{
		public DeviceUniqueIdentifierException() : base($"{nameof(SystemInfo)}." +
														$"{nameof(SystemInfo.deviceUniqueIdentifier)}")
		{
		}
	}
}