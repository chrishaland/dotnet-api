using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Repository;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
#if !DEBUG
using Serilog.Formatting.Compact;
using System.IO;
#endif

namespace Host
{
    public class Program
    {
        private static readonly Serilog.ILogger _logger = Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
#if DEBUG
                .WriteTo.Console()
#else
                .WriteTo.Console(new CompactJsonFormatter())
#endif
                .CreateLogger();

        public static async Task<int> Main(string[] args)
        {
            IHost host;

            try
            {
                _logger.Information("Service is starting up.");
                _logger.Information("Initializing the host.");

                host = CreateHostBuilder(args).Build();

                _logger.Information("Host initialized.");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Host terminated unexpectedly");
                Log.CloseAndFlush();
                throw;
            }

            try
            {
                await MigrateDatabase(host);

                _logger.Information("Starting the host.");
                await host.RunAsync();
                _logger.Information("Host is shutting down.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder(args)
            .UseSerilog(ConfigureSerilog)
            .ConfigureWebHostDefaults(webBuilder => webBuilder
                .ConfigureKestrel(ConfigureKestrel)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .UseStartup<Startup>()
            );

        private static async Task MigrateDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            if (string.IsNullOrEmpty(configuration.GetConnectionString("Database"))) return;

            _logger.Information("Migrating database");
            var database = scope.ServiceProvider.GetRequiredService<Database>();
            await database.Database.MigrateAsync();
            _logger.Information("Migrated database");
        }

        private static void ConfigureKestrel(KestrelServerOptions options)
        {
            options.AddServerHeader = false;
        }

        private static void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder config)
        {
            void SetConfigurationBasePath()
            {
                var module = Process.GetCurrentProcess().MainModule;
                if (module?.ModuleName != "dotnet" && module?.ModuleName != "dotnet.exe" && module?.ModuleName != "w3wp" && module?.ModuleName != "w3wp.exe")
                {
#if DEBUG
                    config.SetBasePath(context.HostingEnvironment.ContentRootPath);
#else
					config.SetBasePath(Path.GetDirectoryName(module?.FileName));
#endif
                }
            }
            SetConfigurationBasePath();
            config.AddJsonFile($"configmap/appsettings.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"secret/appsettings.json", optional: true, reloadOnChange: true);
        }

        private static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration config)
        {
            config
                .MinimumLevel.Debug()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
#if DEBUG
                .WriteTo.Console()
#else
                .WriteTo.Console(new CompactJsonFormatter())
#endif
            ;
        }
    }
}
