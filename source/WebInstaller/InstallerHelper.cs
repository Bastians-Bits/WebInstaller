
namespace WebInstaller
{
	public class InstallerHelper
	{
		protected string OS { get; private set; }
		public string DownloadName { get; protected set; } = "MISSING";

		public InstallerHelper(string os)
		{
			OS = os;
		}

		public async Task<byte[]> Installer(string installer)
        {
			string path = Path.GetFullPath(installer);

			if (!Directory.Exists(path))
				throw new Exception("Installer Directory does not exists");

			if (!Directory.Exists($"{path}{Path.DirectorySeparatorChar}{OS}"))
				throw new Exception("Installer Directory for the requested OS does not exist");

			DirectoryInfo directoryInfo = new($"{path}{Path.DirectorySeparatorChar}{OS}");
			FileInfo? fileInfo = directoryInfo.GetFiles().FirstOrDefault();

			if (fileInfo == null)
				throw new Exception("Installer OS Directory does not contain any files");

			DownloadName = fileInfo.Name;

			return await File.ReadAllBytesAsync(fileInfo.FullName);
        }
	}
}
