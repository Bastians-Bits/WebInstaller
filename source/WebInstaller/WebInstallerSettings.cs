using System.Text;

namespace WebInstaller
{
	public class WebInstallerSettings
	{
		#pragma warning disable CS8618
        public Dictionary<string, string> MimeTypes { get; set; }
        public string Files { get; set; }
		public string Installer { get; set; }
        #pragma warning restore CS8618
    }
}
