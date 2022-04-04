using System;
namespace WebInstaller.Model
{
	public class FileChange : File
	{
		public ChangeType ChangeType { get; private set; }

		public FileChange(string fullName, long size, DateTime dateOfModification, ChangeType changeType)
			: base (fullName, size, dateOfModification)
        {
			ChangeType = changeType;
        }
	}

	public enum ChangeType
    {
		A, // Added
		D, // Deleted
		M  // Modified
    }
}

