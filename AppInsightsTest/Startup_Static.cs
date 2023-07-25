using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using System;
using System.Threading.Tasks;

namespace AppInsightsTest
{

    public static class Startup_Static
    {
        private static string _environment = null;
        private static TelemetryConfiguration _telemetryConfiguration;
        private static TelemetryClient _telemetryClient;

        static Startup_Static() => StartupLogging("AppInsightsTest");

        private static String GetTelemetryConnectionString() => "[INSERT_CONNECTION_STRING_HERE]";
        private static String GetEnvironmentString() => "Dev";
        private static String GetIsLoggingActive() => "TRUE";

        private static TelemetryConfiguration GetTelemetryConfiguration()
        {
            _telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            _telemetryConfiguration.ConnectionString = GetTelemetryConnectionString();
            return _telemetryConfiguration;
        }

        public static void StartupLogging(string appName)
        {
            _environment = GetEnvironmentString();
            _telemetryConfiguration = GetTelemetryConfiguration();
            _telemetryClient = new TelemetryClient(_telemetryConfiguration);

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
                    a.File("C:\\temp\\AppInsightsTest.log", rollingInterval: RollingInterval.Day);
                })
                .WriteTo.Console()
                .CreateLogger();

            LogInformation("----------------------------------------------------------------------");
            LogInformation($"StartUpLogging for : {appName}");
            LogInformation($"Environment: {_environment}");
            LogInformation($"Application Insights Instrumentation Key: {GetTelemetryConnectionString()}");

        }

        public static void ShutDownLogging()
        {
            LogInformation("ShutDownLogging");

            // Explicitly call Flush() followed by sleep is required in console apps.
            // This is to ensure that even if application terminates, telemetry is sent to the back-end.
            _telemetryClient.Flush();
            Log.CloseAndFlush();
            Task.Delay(3500).Wait();
        }
        
        public static void LogInformation(string message)
        {
            if (GetIsLoggingActive() == "TRUE")
            {
                Log.Logger.Information(message);
            }

        }
        
        public static void StartupApplication()
        {
            LogInformation("Logger is working...");
            ShutDownLogging();
        }

    }
    
}
