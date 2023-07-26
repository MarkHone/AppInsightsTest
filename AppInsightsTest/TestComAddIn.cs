using ExcelDna.Integration;
using ExcelDna.Integration.Extensibility;
using System;
using System.Runtime.InteropServices;

namespace AppInsightsTest
{
    [ComVisible(true)]
    public class TestComAddIn : ExcelComAddIn
    {
        public TestComAddIn()
        {
        }

        public override void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
        {
        }

        public override void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
        }

        public override void OnAddInsUpdate(ref Array custom)
        {
        }

        public override void OnStartupComplete(ref Array custom)
        {
        }

        public override void OnBeginShutdown(ref Array custom)
        {
            Startup.ShutDownApplication();
        }

    }

}
