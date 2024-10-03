using System;
using System.IO;

namespace QPMLinksoftwareNew
{
    public class LogFile
    {
        public struct LogLevelConstant
        {
            public const int Error = 0;
            public const int Warning = 1;
            public const int Info = 2;
            public const int Debug = 3;
        }
        public string NL = Environment.NewLine;
        private string[] LogLevelName = new string[4] { "(error)  ", "(warning)", "(info)   ", "(debug)  " }; // Equal lengths.

        private struct LogSettingDef
        {
            public string LogFilePath;               // Full path without ending backslash. 
            public string LogFileNameStart;          // Normally the name of the running application.
            public int LogFileLifeTimeDays;          // Old log files are deleted after this number of days.
            public int LogLevel;                     // See LogLevelConstant struct
            public DateTime LatestDateTimeFileCheck; // Check for old log files once a day (and at object creation time).
        }
        private LogSettingDef LogSetting = new LogSettingDef();

        public LogFile(string LogFilePath, string LogFileNameStart, int LogFileLifeTimeDays, int LogLevel)
        {
            LogSetting.LogFilePath = LogFilePath;
            LogSetting.LogFileNameStart = LogFileNameStart;
            LogSetting.LogFileLifeTimeDays = LogFileLifeTimeDays;
            LogSetting.LogLevel = LogLevel;
            LogSetting.LogFileNameStart = LogFileNameStart;
            LogSetting.LatestDateTimeFileCheck = DateTime.Now.AddDays(-2); // For check at object creation time
            OldLogFileCheck();
            WriteToLog(LogFile.LogLevelConstant.Info,
                            "Software version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        public void SetLoglevel(int LogLevel)
        {
            LogSetting.LogLevel = LogLevel;
        }

        public void WriteToLog(int LogLevel, string Message)
        {
            if (LogLevel <= LogSetting.LogLevel)
            {
                try
                {
                    OldLogFileCheck();
                    StreamWriter Log = File.AppendText(LogSetting.LogFilePath + @"\" +
                        LogSetting.LogFileNameStart + DateTime.Now.ToString("yyMMdd") + ".log");
                    if (Message == "")
                    {
                        Log.WriteLine(""); // Empty message means empty line (no date/time).
                    }
                    else
                    {
                        Log.WriteLine(System.DateTime.Now.ToString("yy-MM-dd HH:mm:ss") +
                            " " + LogLevelName[LogLevel] + " " + Message);
                    }
                    Log.Flush();
                    Log.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Log file error (1):\n" + ex.ToString());
                    Console.ReadLine();
                }
            }
        }

        private void OldLogFileCheck()
        {
            try
            {
                if (LogSetting.LatestDateTimeFileCheck <= DateTime.Now.AddDays(-1))
                {
                    string OldestDateInFileName = DateTime.Now.AddDays(-LogSetting.LogFileLifeTimeDays).ToString("yyMMdd");
                    string[] PresentLogFileNames;
                    PresentLogFileNames = Directory.GetFiles(LogSetting.LogFilePath, LogSetting.LogFileNameStart + "*.log");
                    foreach (string FullFileName in PresentLogFileNames)
                    {
                        string LogFileName = FullFileName.Substring(LogSetting.LogFilePath.Length + 1);
                        // Skip other files with suffix .log
                        if (LogFileName.Substring(0, LogSetting.LogFileNameStart.Length).CompareTo(LogSetting.LogFileNameStart) == 0)
                        {
                            string DateInLogFileName = LogFileName.Substring(LogSetting.LogFileNameStart.Length, 6);
                            if (DateInLogFileName.CompareTo(OldestDateInFileName) < 0)
                            {
                                File.Delete(FullFileName);
                            }
                        }
                    }
                    LogSetting.LatestDateTimeFileCheck = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Log file error (2):\n" + ex.ToString());
                Console.ReadLine();
            }
        }

    }
}
