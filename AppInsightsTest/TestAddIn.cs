using ExcelDna.Integration;
using System;
using System.Windows.Forms;

namespace AppInsightsTest
{

    public class TestAddIn : IExcelAddIn
    {
        TestComAddIn _comAddIn;

        public void AutoOpen()
        {

            try
            {
                _comAddIn = new TestComAddIn();
                ExcelComAddInHelper.LoadComAddIn(_comAddIn);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error loading COM AddIn: " + e.ToString());
            }

            Startup.StartupApplication();
        }

        public void AutoClose()
        {
            Startup.ShutDownApplication();
        }

    }

}
