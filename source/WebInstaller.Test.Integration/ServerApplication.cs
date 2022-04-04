using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace WebInstaller.Test.Integration
{
	public class ServerApplication : WebApplicationFactory<Program>
	{
        protected override IHost CreateHost(IHostBuilder builder)
        {
            _ = builder.UseEnvironment("Test");

            return base.CreateHost(builder);
        }
    }
}
