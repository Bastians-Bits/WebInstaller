namespace WebInstaller.Model
{
	public class File
	{
		public string FullName { get; private set; }
		public long Size { get; private set; }
		public DateTime DateOfModification { get; private set; }

		public File(string fullName, long size, DateTime dateOfModification)
        {
			FullName = fullName;
			Size = size;
			DateOfModification = dateOfModification;
        }
    }
}

