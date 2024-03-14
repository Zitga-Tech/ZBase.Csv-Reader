#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Common
{
	using System;

	[Serializable]
	internal struct ACTkByte8
	{
		public byte b1;
		public byte b2;
		public byte b3;
		public byte b4;
		public byte b5;
		public byte b6;
		public byte b7;
		public byte b8;

		public void Shuffle()
		{
			var buffer = b1;
			b1 = b2;
			b2 = buffer;
			buffer = b5;
			b5 = b6;
			var buffer2 = b8;
			b8 = buffer;
			b6 = buffer2;
		}

		public void UnShuffle()
		{
			var buffer = b1;
			b1 = b2;
			b2 = buffer;
			buffer = b5;
			b5 = b8;
			var buffer2 = b6;
			b6 = buffer;
			b8 = buffer2;
		}
	}
}