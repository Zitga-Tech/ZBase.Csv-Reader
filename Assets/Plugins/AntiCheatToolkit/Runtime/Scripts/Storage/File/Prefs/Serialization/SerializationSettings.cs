#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	internal enum ACTkSerializationKind
	{
		Binary
	}
	
	internal class SerializationSettings
	{
		public ACTkSerializationKind SerializationKind { get; }
		
		public SerializationSettings(ACTkSerializationKind serializationKind = ACTkSerializationKind.Binary)
		{
			SerializationKind = serializationKind;
		}
	}
}