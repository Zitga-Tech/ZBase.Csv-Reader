#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using System;

	/// <summary>
	/// Describes assembly which is added to the InjectionDetector "white list".
	/// </summary>
	[Serializable]
	public class AllowedAssembly
	{
		[Obsolete("Please use Name property instead.", false)]
		public string name => Name;
		
		[Obsolete("Please use Hashes property instead.", false)]
		public int[] hashes => Hashes;
		
		/// <summary>
		/// Assembly name, i.e.: ACTk.Runtime.
		/// </summary>
		public string Name { get; }
		
		/// <summary>
		/// Array of whitelisted hashes for the assembly with given Name.
		/// </summary>
		public int[] Hashes { get; private set; }

		/// <summary>
		/// Constructs new instance.
		/// </summary>
		/// <param name="name">Sets Name property.</param>
		/// <param name="hashes">Sets Hashes property.</param>
		public AllowedAssembly(string name, int[] hashes)
		{
			Name = name;
			Hashes = hashes;
		}

		/// <summary>
		/// Allows adding new hash to the Hashes collection.
		/// </summary>
		/// <param name="hash">New whitelisted hash for the assembly with specified Name.</param>
		/// <returns>True if hash was added and false otherwise (i.e. when hash already existed in the collection).</returns>
		public bool AddHash(int hash)
		{
			if (Array.IndexOf(Hashes, hash) != -1) return false;

			var oldLen = Hashes.Length;
			var newLen = oldLen + 1;

			var newHashesArray = new int[newLen];
			Array.Copy(Hashes, newHashesArray, oldLen);

			Hashes = newHashesArray;
			Hashes[oldLen] = hash;

			return true;
		}

		public override string ToString()
		{
			return Name + " (hashes: " + Hashes.Length + ")";
		}
	}
}