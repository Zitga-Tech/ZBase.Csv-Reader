#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using Common;
	using UnityEngine;

	internal class PersistentDataPathException : BackgroundThreadAccessException
	{
		public PersistentDataPathException() : base($"{nameof(Application)}." +
													$"{nameof(Application.persistentDataPath)}")
		{
		}
	}
}