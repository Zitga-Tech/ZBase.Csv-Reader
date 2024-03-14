#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Genuine.CodeHash
{
	using System;

	/// <summary>
	/// Result produced by CodeHashGenerator. Contains resulting code hash or errors information.
	/// </summary>
	public class HashGeneratorResult
	{
		[Obsolete("Please use SummaryHash property instead.")]
		public string CodeHash
		{
			get { return SummaryHash; }
		}

		/// <summary>
		/// Summary hash for all files in currently running build.
		/// May be null in case #Success is not true.
		/// </summary>
		/// Use with caution: summary hash for runtime build may differ from the summary hash
		/// you got in Editor, for example, for Android App Bundles.
		/// Use #FileHashes for more accurate hashes comparison control.
		public string SummaryHash
		{
			get
			{
				return buildHashes.SummaryHash;
			}
		}

		/// <summary>
		/// Hashes for all files in currently running build.
		/// Feel free to compare it against hashes array you got in Editor to find if
		/// runtime version has new unknown hashes (this is an indication build was altered).
		/// </summary>
		public FileHash[] FileHashes
		{
			get
			{
				return buildHashes.FileHashes;
			}
		}

		/// <summary>
		/// Error message you could find useful in case #Success is not true.
		/// </summary>
		public string ErrorMessage { get; private set; }

		/// <summary>
		/// True if generation was successful and resulting hashes are stored in #FileHashes,
		/// otherwise check #ErrorMessage to find out error cause.
		/// </summary>
		public bool Success
		{
			get { return ErrorMessage == null; }
		}

		private string summaryCodeHash;
		private BuildHashes buildHashes;

		internal static HashGeneratorResult FromError(string errorMessage)
		{
			return new HashGeneratorResult
			{
				ErrorMessage = errorMessage
			};
		}

		internal static HashGeneratorResult FromBuildHashes(BuildHashes buildHashes)
		{
			return new HashGeneratorResult
			{
				buildHashes = buildHashes
			};
		}

		/// <summary>
		/// Checks is passes hash exists in file hashes of this instance.
		/// </summary>
		/// <param name="hash">Target file hash.</param>
		/// <returns>True if such hash presents at #FileHashes and false otherwise.</returns>
		public bool HasFileHash(string hash)
		{
			return buildHashes.HasFileHash(hash);
		}
	}
}