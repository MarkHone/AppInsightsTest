using Startup = AppInsightsTest.Startup;//AppInsightsTest_Static.Startup;

namespace LogTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Startup.StartupApplication();
            Startup.ShutDownApplication();
        }
    }
}
