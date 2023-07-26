using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace AppInsightsTest
{

    public static class Startup
    {
        private static readonly IHost _host;
        private static string _appName;
        private static ILogger<AppInsightsTest> _logger;
        private static TelemetryConfiguration _telemetryConfiguration;
        private static TelemetryClient _telemetryClient;

        static Startup() => _host = BuildHost();

        public static FunctionLoggerConfig FunctionLoggerConfig { get; private set; }

        public static void StartupApplication()
        {
            string environment = Environment.GetEnvironmentVariable("Environment") ?? "<UNSET>";
            _logger = _host.Services.GetRequiredService<ILogger<AppInsightsTest>>();
            _logger.LogInformation($"Starting logging for: {_appName}, environment: {environment}.");
            _logger.LogInformation($"Application Insights Instrumentation Key: {_telemetryConfiguration.InstrumentationKey}");
        }

        public static void ShutDownApplication()
        {
            _logger?.LogInformation("Application shutting down.");
            _telemetryClient?.Flush();
            Log.CloseAndFlush();
            Task.Delay(3500).Wait();
        }

        private static IHost BuildHost()
        {
            // When run from Excel-DNA, the security protocol appears to be downgraded to TLS 1.0 so this forces it to use TLS 1.2
            // https://stackoverflow.com/questions/2582036/an-existing-connection-was-forcibly-closed-by-the-remote-host
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                var builder = Host.CreateDefaultBuilder();
                builder.ConfigureServices((context, services) =>
                {
                    FunctionLoggerConfig = GetFunctionLoggerConfig(context.Configuration);
                });
                
                builder.ConfigureLogging((context, loggingBuilder) =>
                {
                    string appInsightsConnectionString = context.Configuration["Serilog:WriteTo:AppInsightsSink:Args:ConnectionString"];
                    loggingBuilder.AddApplicationInsights(
                        configureTelemetryConfiguration: (config) => config.ConnectionString = appInsightsConnectionString,
                        configureApplicationInsightsLoggerOptions: (options) => { }
                    );

                    if (!string.IsNullOrEmpty(appInsightsConnectionString) && _telemetryConfiguration == null)
                    {
                        _telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                        _telemetryConfiguration.ConnectionString = appInsightsConnectionString;
                    }

                    _telemetryClient = _telemetryClient ?? new TelemetryClient(_telemetryConfiguration);
                });

                builder.UseSerilog((context, loggerConfig) => GetSerilogConfig(loggerConfig, context.Configuration));

                return builder.Build();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
            
        }

        private static FunctionLoggerConfig GetFunctionLoggerConfig(IConfiguration appConfig)
        {
            return new FunctionLoggerConfig
            {
                IsActive = bool.Parse(appConfig["FunctionLogger:IsActive"] ?? string.Empty),
                IsLogToFile = bool.Parse(appConfig["FunctionLogger:IsLogToFile"] ?? string.Empty),
                FilenameRoot = appConfig["FunctionLogger:FilenameRoot"] ?? string.Empty
            };
        }
        
        private static void GetSerilogConfig(LoggerConfiguration loggerConfig, IConfiguration appConfig)
        {
            _appName = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Name ?? string.Empty;
            loggerConfig.ReadFrom.Configuration(appConfig);
            loggerConfig.Enrich.WithProperty("MachineName", Environment.MachineName);
            loggerConfig.Enrich.WithProperty("ApplicationName", _appName);
            loggerConfig.Enrich.WithProperty("UserName", Environment.UserName);

            if (FunctionLoggerConfig.IsLogToFile)
            {
                string path = $"{FunctionLoggerConfig.FilenameRoot}-{Environment.UserName}-{Process.GetCurrentProcess().Id}-.log";
                // Have added async writes to file sink to reduce load on main thread.
                // Doesn't appear to be required for Application Insights sink:
                // https://github.com/serilog/serilog-sinks-async
                // https://github.com/serilog-contrib/serilog-sinks-applicationinsights#how-when-and-why-to-flush-messages-manually

                loggerConfig.WriteTo.Async(a =>
                {
                    a.File(path, rollingInterval: RollingInterval.Day
                        , outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {MachineName} {UserName} {ApplicationName} {SourceContext} {Message:lj}{NewLine}{Exception}"
                        , fileSizeLimitBytes: 10485760, rollOnFileSizeLimit: true, retainedFileCountLimit: 7);
                });

            }
            
        }

    }
    
}
