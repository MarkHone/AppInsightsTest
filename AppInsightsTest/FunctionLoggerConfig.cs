namespace AppInsightsTest
{
    public class FunctionLoggerConfig
    {
        public bool IsActive { get; set; }
        public bool IsLogToFile { get; set; }
        public string FilenameRoot { get; set; } = string.Empty;
    }
}
