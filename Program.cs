using Avalonia;
using System;
using System.Diagnostics;

namespace AgentManagement.Avalonia;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // 确保控制台可用（Windows 环境下）
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            try
            {
                // 附加到父进程的控制台，或创建新控制台
                AttachConsole(-1); // ATTACH_PARENT_PROCESS = -1             
            }
            catch
            {
                // 如果附加失败，忽略
            }
        }
        
        // 重定向 Trace 到控制台
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        Trace.AutoFlush = true;
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }
    
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            //.WithDeveloperTools() // 暂时注释，避免编译错误
#endif
            .WithInterFont()
            .LogToTrace();
}
