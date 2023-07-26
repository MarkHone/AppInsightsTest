using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace AppInsightsTest_Static
{

    public static class Startup
    {
        private static TelemetryConfiguration _telemetryConfiguration;
        private static TelemetryClient _telemetryClient;
        private static ILogger _logger;

        static Startup() => BuildLogger("AppInsightsTest");

        private static string GetTelemetryConnectionString() => "[INSERT_CONNECTION_STRING_HERE]";

        public static void StartupApplication_Static()
        {
            string environment = Environment.GetEnvironmentVariable("Environment") ?? "<UNSET>";
            LogInformation($"Starting logging for : {Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty}, environment: {environment}.");
            LogInformation($"Application Insights Instrumentation Key: {_telemetryConfiguration.InstrumentationKey}");
        }

        public static void ShutDownApplication_Static()
        {
            LogInformation("Application shutting down.");
            // Explicitly call Flush() followed by sleep is required in console apps.
            // This is to ensure that even if application terminates, telemetry is sent to the back-end.
            _telemetryClient.Flush();
            Log.CloseAndFlush();
            Task.Delay(3500).Wait();
        }

        private static TelemetryConfiguration GetTelemetryConfiguration()
        {
            _telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            _telemetryConfiguration.ConnectionString = GetTelemetryConnectionString();
            return _telemetryConfiguration;
        }

        public static void BuildLogger(string appName)
        {
            _telemetryConfiguration = GetTelemetryConfiguration();
            _telemetryClient = new TelemetryClient(_telemetryConfiguration);
            string path = $"C:\\temp\\fortisaddin-{Environment.UserName}-{Process.GetCurrentProcess().Id}-.log";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", appName)
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("UserName", Environment.UserName)
                .WriteTo.Async(a =>
                {
                    a.ApplicationInsights(_telemetryConfiguration, TelemetryConverter.Traces);
                })
                .WriteTo.Async(a =>
                {
                    a.File(path, rollingInterval: RollingInterval.Day);
                })
                .WriteTo.Console()
                .CreateLogger();

            _logger = Log.Logger;
        }
        
        public static void LogInformation(string message)
        {
            _logger.Information(message);
        }
        
    }
    
}
