using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace WebInstaller.Test.Integration
{
	internal class ServerApplication : WebApplicationFactory<Program>
	{
        protected override IHost CreateHost(IHostBuilder builder)
        {
            _ = builder.UseEnvironment("Test");

            return base.CreateHost(builder);
        }
    }
}
