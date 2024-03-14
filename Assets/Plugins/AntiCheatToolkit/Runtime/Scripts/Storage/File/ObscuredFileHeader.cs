#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System.IO;

	internal struct ObscuredFileHeader
	{
		private const byte HeaderByte1 = 65; // A
		private const byte HeaderByte2 = 67; // C
		private const byte HeaderByte3 = 84; // T
		private const byte HeaderByte4 = 107; // k
		private const byte HeaderVersion = 0;

		private byte Byte1 { get; set; }
		private byte Byte2 { get; set; }
		private byte Byte3 { get; set; }
		private byte Byte4 { get; set; }
		
		public byte Version { get; private set; }
		public ObscurationMode ObscurationMode { get; private set; }

		public bool IsValid()
		{
			return Version == HeaderVersion &&
				   Byte1 == HeaderByte1 &&
				   Byte2 == HeaderByte2 &&
				   Byte3 == HeaderByte3 &&
				   Byte4 == HeaderByte4;
		}
			
		public void ReadFrom(Stream stream)
		{
			Byte1 = (byte)stream.ReadByte();
			Byte2 = (byte)stream.ReadByte();
			Byte3 = (byte)stream.ReadByte();
			Byte4 = (byte)stream.ReadByte();
			Version = (byte)stream.ReadByte();
			ObscurationMode = (ObscurationMode)stream.ReadByte();
		}

		public static void WriteTo(Stream stream, ObscurationMode obscurationMode)
		{
			stream.WriteByte(HeaderByte1);
			stream.WriteByte(HeaderByte2);
			stream.WriteByte(HeaderByte3);
			stream.WriteByte(HeaderByte4);
			stream.WriteByte(HeaderVersion);
			stream.WriteByte((byte)obscurationMode);
		}
	}
}