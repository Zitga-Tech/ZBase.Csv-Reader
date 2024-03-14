#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Genuine.CodeHash
{
	/// <summary>
	/// Holds hash for the specific file.
	/// </summary>
	public class FileHash
	{
		/// <summary>
		/// Path to the file which was hashed.
		/// </summary>
		public string Path { get; }

		/// <summary>
		/// Hash of the file. Calculated using semi-custom hashing algorithm based on SHA1.
		/// </summary>
		public string Hash { get; }

		internal FileHash(string path, string hash)
		{
			Path = path;
			Hash = hash;
		}
	}
}