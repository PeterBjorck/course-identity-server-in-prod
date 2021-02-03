using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace IdentityService
{
    public class Program
    {
        private static readonly string _applicationName = "IdentityService";

        public static int Main(string[] args)
        {
            Console.Title = _applicationName;

            Settings.StartupTime = DateTime.Now;

            try
            {
                Log.Information("Starting web host for " + _applicationName);
                CreateHostBuilder(args)
                    .ConfigureSerilog(_applicationName, options =>
                    {
                        ConfigureLogLevels(options);
                    })
                    .UseSerilog()
                    .Build()
                    .Run();
                return 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT='{env}'");
            switch (env)
            {
                case "Production":
                    Console.WriteLine("Configuring Kestrel for production");

                    //Use Azure KeyVault for config + Https certificate in key vault
                    return Host.CreateDefaultBuilder(args)
                        .ConfigureAppConfiguration(config =>
                        {
                            config.AddAzureKeyVaultSupport();
                        })
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.AddHttpsSupport(certName: "webapi-certificate");

                            webBuilder.UseStartup<Startup>()
                                .UseUrls("http://*:80;https://*:443");
                        });
                case "Offline":
                    Console.WriteLine("Configuring Kestrel for offline");

                    //Use local configuration and the built in localhost developer certificate
                    return Host.CreateDefaultBuilder(args)
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                        });

                default:
                    //Development
                    Console.WriteLine("Configuring Kestrel for development");

                    //Use Azure Key Vault for config and the built in localhost developer certificate
                    return Host.CreateDefaultBuilder(args)
                        .ConfigureAppConfiguration(config =>
                        {
                            config.AddAzureKeyVaultSupport();
                        })
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                        });
            }
        }
        private static void ConfigureLogLevels(LoggerConfiguration options)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            switch (environmentName)
            {
                case "Production":
                    options.MinimumLevel.Override("IdentityServer4", LogEventLevel.Debug)
                           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                           .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                           .MinimumLevel.Override("System", LogEventLevel.Warning)
                           .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                           .MinimumLevel.Override("Microsoft.AspNetCore.Authorization", LogEventLevel.Information);

                    break;
                case "Offline":
                    options.MinimumLevel.Override("System", LogEventLevel.Warning)
                           .MinimumLevel.Override("IdentityServer4", LogEventLevel.Debug)
                           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                           .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                           .MinimumLevel.Override("Microsoft.AspNetCore.DataProtection", LogEventLevel.Debug)
                           .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                           .MinimumLevel.Override("Microsoft.AspNetCore.Authorization", LogEventLevel.Information);
                    break;
                default:
                    //Development
                    options.MinimumLevel.Override("IdentityServer4", LogEventLevel.Debug)
                           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                           .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                           .MinimumLevel.Override("System", LogEventLevel.Warning)
                           .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                           .MinimumLevel.Override("Microsoft.AspNetCore.Authorization", LogEventLevel.Information)
                           .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Debug);
                           
                    break;
            }
        }

    }
}
