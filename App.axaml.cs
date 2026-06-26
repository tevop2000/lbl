using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using AgentManagement.Avalonia.ViewModels;
using AgentManagement.Avalonia.Views;

namespace AgentManagement.Avalonia;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 直接显示主窗口（内部包含登录和主界面）
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            MainWindow = mainWindow;
            desktop.MainWindow = mainWindow;
            mainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}