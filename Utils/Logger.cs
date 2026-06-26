using System;
using System.IO;

namespace AgentManagement.Avalonia.Utils
{
    /// <summary>
    /// 日志工具类 - 将日志输出到控制台和文件
    /// </summary>
    public static class Logger
    {
        private static readonly string LogFilePath;
        private static readonly object LockObject = new object();

        static Logger()
        {
            // 日志文件路径：应用程序目录下的 logs 文件夹
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            // 日志文件名格式：yyyy-MM-dd.log
            string logFileName = $"api_{DateTime.Now:yyyy-MM-dd}.log";
            LogFilePath = Path.Combine(logDir, logFileName);

            // 写入日志文件头
            WriteToLogFile($"\n\n========== 应用程序启动: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========\n");
        }

        /// <summary>
        /// 写入信息日志
        /// </summary>
        public static void Info(string message)
        {
            WriteLog("INFO", message, ConsoleColor.White);
        }

        /// <summary>
        /// 写入成功日志
        /// </summary>
        public static void Success(string message)
        {
            WriteLog("SUCCESS", message, ConsoleColor.Green);
        }

        /// <summary>
        /// 写入警告日志
        /// </summary>
        public static void Warning(string message)
        {
            WriteLog("WARNING", message, ConsoleColor.Yellow);
        }

        /// <summary>
        /// 写入错误日志
        /// </summary>
        public static void Error(string message, Exception? ex = null)
        {
            WriteLog("ERROR", message, ConsoleColor.Red);
            
            if (ex != null)
            {
                WriteLog("ERROR", $"异常类型: {ex.GetType().Name}", ConsoleColor.Red);
                WriteLog("ERROR", $"堆栈跟踪:\n{ex.StackTrace}", ConsoleColor.Red);
                
                if (ex.InnerException != null)
                {
                    WriteLog("ERROR", $"内部异常: {ex.InnerException.Message}", ConsoleColor.Red);
                }
            }
        }

        /// <summary>
        /// 写入调试日志
        /// </summary>
        public static void Debug(string message)
        {
            WriteLog("DEBUG", message, ConsoleColor.Gray);
        }

        /// <summary>
        /// 写入分隔线
        /// </summary>
        public static void Separator(string title = "")
        {
            string line = $"\n========== {title} ==========";
            Console.WriteLine(line);
            WriteToLogFile(line);
        }

        /// <summary>
        /// 核心日志写入方法
        /// </summary>
        private static void WriteLog(string level, string message, ConsoleColor color)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logMessage = $"[{timestamp}] [{level}] {message}";

            // 输出到控制台
            lock (LockObject)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(logMessage);
                Console.ForegroundColor = originalColor;
            }

            // 输出到文件
            WriteToLogFile(logMessage);
        }

        /// <summary>
        /// 写入日志文件
        /// </summary>
        private static void WriteToLogFile(string message)
        {
            try
            {
                lock (LockObject)
                {
                    File.AppendAllText(LogFilePath, message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // 如果写入文件失败，至少输出到 Debug
                System.Diagnostics.Debug.WriteLine($"写入日志文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        public static string GetLogFilePath()
        {
            return LogFilePath;
        }
    }
}
