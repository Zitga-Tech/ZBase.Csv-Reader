#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Common
{
	using System;
	
	internal class BackgroundThreadAccessException : Exception
	{
		public string AccessedApi { get; }
		
		public BackgroundThreadAccessException(string apiName):base($"Attempt {apiName} access from non-main thread! " +
										  "This API can't be accessed from child threads.")
		{
			AccessedApi = apiName;
		}
	}
}