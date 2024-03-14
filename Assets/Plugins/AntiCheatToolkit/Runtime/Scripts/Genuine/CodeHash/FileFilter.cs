#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Genuine.CodeHash
{
	using System;
	using System.IO;

	internal class FileFilter
	{
#pragma warning disable 0649
		public bool caseSensitive;
		public bool folderRecursive;
		public bool exactFileNameMatch;
		public bool exactFolderMatch;

		public string filterPath;
		public string filterExtension;
		public string filterFileName;
#pragma warning restore 0649

		public bool MatchesPath(string filePath, string root = null)
		{
			if (filterExtension != null)
			{
				var extension = Path.GetExtension(filePath);
				if (string.IsNullOrEmpty(extension) || extension == ".")
				{
					return false;
				}

				extension = extension.Remove(0, 1);
				if (!filterExtension.Equals(extension,
					caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			if (filterFileName != null)
			{
				var fileName = Path.GetFileNameWithoutExtension(filePath);
				if (string.IsNullOrEmpty(fileName))
				{
					return false;
				}

				if (exactFileNameMatch)
				{
					if (!filterFileName.Equals(fileName, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
				}
				else
				{
					if (fileName.IndexOf(filterFileName, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) == -1)
					{
						return false;
					}
				}
			}

			if (filterPath != null)
			{
				if (filePath.IndexOf(filterPath, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) == -1)
				{
					return false;
				}
			}

			return true;
		}

		public override string ToString()
		{
			return caseSensitive + "|" +
			       folderRecursive + "|" +
			       exactFileNameMatch + "|" +
			       exactFolderMatch + "|" +
			       filterPath + "|" +
			       filterExtension + "|" +
			       filterFileName;
		}

		/*public bool MatchesZipFile(string entryFileName)
		{
			Debug.LogError(entryFileName);
			Debug.LogError(Path.GetFileNameWithoutExtension(entryFileName));
			Debug.LogError(Path.GetExtension(entryFileName));
			return false;
			//return entryFileName.EndsWith(filterFileName + filterExtension, StringComparison.OrdinalIgnoreCase);
		}*/
	}
}