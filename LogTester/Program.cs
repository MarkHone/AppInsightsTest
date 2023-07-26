using AppInsightsTest;

namespace LogTester
{
    public class Program
    {
        static void Main(string[] args)
        {
            Startup.StartupApplication();
            Startup.ShutDownApplication();            
        }
    }
}
