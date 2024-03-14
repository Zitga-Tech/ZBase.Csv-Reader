#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using Utils;

	/// <summary>
	/// Represents how data will be saved to file.
	/// </summary>
	public enum ObscurationMode : byte
	{
		/// <summary>
		/// No encryption, just plain binary data. Serialized data will be readable.
		/// </summary>
		Plain = 0,
		
		/// <summary>
		/// All data will be encrypted and not readable in the file.
		/// </summary>
		Encrypted = 1
	}
	
	/// <summary>
	/// Different ObscuredFile and ObscuredFilePrefs encryption-related settings.
	/// </summary>
	public class EncryptionSettings
	{
		/// <summary>
		/// Represents the way data is stored in the file: eiter plain (as is) or encrypted with user-set password,
		/// depending on #Password value.
		/// </summary>
		public ObscurationMode ObscurationMode { get; }
		
		/// <summary>
		/// Password used to encrypt and decrypt data. Switches #ObscurationMode to ObscurationMode.Encrypted when set
		/// or to ObscurationMode.Plain when not set.
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly Any existing data encrypted with
		/// one password will not be readable with another password.</strong>
		public byte[] Password { get; }

		/// <summary>
		/// Creates new specified encryption settings instance.
		/// </summary>
		/// <param name="password">Password string used to encrypt and decrypt data.
		/// Changes the way data is stored in the file: plain (as is) if <c>password</c>
		/// is not set or empty, otherwise data will be encrypted on write and decrypted on read using this
		/// <c>password</c>. Will be converted to byte[] internally.</param>
		/// <returns>Configured instance.</returns>
		public EncryptionSettings(string password) : this(StringUtils.StringToBytes(password))
		{
		}

		/// <summary>
		/// Creates new specified encryption settings instance.
		/// </summary>
		/// <param name="password">Password bytes used to encrypt and decrypt data.
		/// Changes the way data is stored in the file: plain (as is) if <c>password</c>
		/// is not set or empty, otherwise data will be encrypted on write and decrypted on read using this
		/// <c>password</c>.</param>
		/// <returns>Configured instance.</returns>
		public EncryptionSettings(byte[] password = null)
		{
			if (password == null || password.Length == 0)
				ObscurationMode = ObscurationMode.Plain;
			else
				ObscurationMode = ObscurationMode.Encrypted;
			
			Password = password;
		}
	}
}