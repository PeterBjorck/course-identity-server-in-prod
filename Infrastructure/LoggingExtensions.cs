using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Infrastructure
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configure Serilog and its sinks
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="applicationName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureSerilog(this IHostBuilder builder, 
                                                    string applicationName,
                                                    Action<LoggerConfiguration> options)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var seqServerUrl = configuration["SeqServerUrl"] ?? "";

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithProperty("Environment", env);

            //Set the logging levels
            options?.Invoke(logger);

            ConfigureSerilogSinks(logger, seqServerUrl, env);

            return builder;
        }

        
        /// <summary>
        /// Configure the sinks depending on the current environment (development, offline or production)
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="seqServerUrl"></param>
        /// <param name="env"></param>
        private static void ConfigureSerilogSinks(LoggerConfiguration logger, 
                                                  string seqServerUrl, 
                                                  string env)
        {
            switch (env.ToLower())
            {
                case "offline":
                    Console.WriteLine("Logging to Console");

                    Log.Logger = logger
                        .WriteTo.Console(
                            restrictedToMinimumLevel: LogEventLevel.Verbose,
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                            theme: AnsiConsoleTheme.Code)

                        .CreateLogger();
                    break;
                case "production":

                    Console.WriteLine("Logging to Console and Seq at " + seqServerUrl);

                    if(string.IsNullOrWhiteSpace(seqServerUrl))
                        throw new ArgumentNullException(nameof(seqServerUrl));

                    //In production we write:
                    //- Fatal entries to the console
                    //- Debug level and up to Seq
                    Log.Logger = logger
                        .WriteTo.Console(
                            restrictedToMinimumLevel: LogEventLevel.Fatal,
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                            theme: AnsiConsoleTheme.Code)

                        .WriteTo.Seq(serverUrl: seqServerUrl,
                            restrictedToMinimumLevel: LogEventLevel.Debug,
                            batchPostingLimit: 1000,
                            period: TimeSpan.FromSeconds(5),
                            apiKey: "",
                            bufferBaseFilename: "seq-buffer.txt",
                            bufferSizeLimitBytes: 1000000,
                            eventBodyLimitBytes: 262144,
                            controlLevelSwitch: null,
                            messageHandler: null,
                            retainedInvalidPayloadsLimitBytes: null,
                            compact: false,
                            queueSizeLimit: 100000)
                        .CreateLogger();

                    break;
                default:
                    //In development we send debug statements to both seq and the console
                    Console.WriteLine("Logging to Console and Seq at " + seqServerUrl);

                    if (string.IsNullOrWhiteSpace(seqServerUrl))
                        throw new ArgumentNullException(nameof(seqServerUrl));

                    Log.Logger = logger
                        .WriteTo.Console(
                            restrictedToMinimumLevel: LogEventLevel.Debug,
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                            theme: AnsiConsoleTheme.Code)

                        //Comment out writeTo.Seq if you don't want to send log traffic to Seq during development
                        ///*
                        .WriteTo.Seq(serverUrl: seqServerUrl,
                            restrictedToMinimumLevel: LogEventLevel.Debug,
                            batchPostingLimit: 1000,
                            period: TimeSpan.FromSeconds(5),
                            apiKey: "",
                            bufferBaseFilename: "seq-buffer.txt",
                            bufferSizeLimitBytes: 1000000,
                            eventBodyLimitBytes: 262144,
                            controlLevelSwitch: null,
                            messageHandler: null,
                            retainedInvalidPayloadsLimitBytes: null,
                            compact: false,
                            queueSizeLimit: 100000)
                        //*/
                        .CreateLogger();
                        
                    break;
            }
        }
    }
}
