using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Serilog;
using Tests.IntegrationTests;

[SetUpFixture]
#pragma warning disable CA1050 // Declare types in namespaces
public class OneTimeTestServerSetup
#pragma warning restore CA1050 // Declare types in namespaces
{
    private static readonly TestServer _testServer = new(TestServerBuilder);
    internal static HttpClient Client = new();
    internal static DatabaseContext Database = new(new DbContextOptionsBuilder<DatabaseContext>().Options);

    [OneTimeSetUp]
    public async Task Before()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        await _testServer.Host.StartAsync();
        Client = _testServer.CreateClient();
        Database = _testServer.Host.Services.GetRequiredService<DatabaseContext>();
    }

    [OneTimeTearDown]
    public async Task After()
    {
        await _testServer.Host.StopAsync();
        _testServer.Dispose();
        Client.Dispose();
        Database.Dispose();
    }

    private static IWebHostBuilder TestServerBuilder => new WebHostBuilder()
        .UseTestServer()
        .UseConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(ConfigurationValues)
            .Build()
        )
        .UseSerilog(new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger()
        )
        .UseStartup<Host.Startup>()
        .ConfigureTestServices(services =>
        {
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseInMemoryDatabase("Database");
            });
            services.AddAuthentication("BasicAuthentication")
                    .AddScheme<AuthenticationSchemeOptions, MockAuthenticatedUser>("BasicAuthentication", null);
        });

    private static Dictionary<string, string> ConfigurationValues => new()
    {
        {"oidc:roles:user", "user"},
        {"oidc:roles:admin", "admin"},
    };
}
