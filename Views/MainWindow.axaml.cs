using Avalonia.Controls;
using AgentManagement.Avalonia.ViewModels;
using AgentManagement.Avalonia.Services;

namespace AgentManagement.Avalonia.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        
        // 初始化文件服务
        FileService.SetTopLevel(this);
        
        // 在窗口加载完成后设置内容（此时 DataContext 已经设置）
        Loaded += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[MainWindow] DataContext type: {DataContext?.GetType().Name ?? "null"}");
            
            if (DataContext is MainWindowViewModel vm)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] IsLoggedIn: {vm.IsLoggedIn}");
                UpdateContent(vm);
                
                // 监听 IsLoggedIn 属性变化
                vm.PropertyChanged += (s2, e2) =>
                {
                    if (e2.PropertyName == nameof(MainWindowViewModel.IsLoggedIn))
                    {
                        System.Diagnostics.Debug.WriteLine($"[MainWindow] IsLoggedIn changed to: {vm.IsLoggedIn}");
                        UpdateContent(vm);
                    }
                };
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[MainWindow] DataContext is not MainWindowViewModel!");
            }
        };
    }
    
    private void UpdateContent(MainWindowViewModel vm)
    {
        System.Diagnostics.Debug.WriteLine($"[UpdateContent] IsLoggedIn: {vm.IsLoggedIn}");
        
        if (vm.IsLoggedIn)
        {
            // 显示主界面 - 加载 MainWindowContent UserControl
            System.Diagnostics.Debug.WriteLine("[UpdateContent] Showing main content");
            MainContentControl.Content = new MainWindowContentView { DataContext = vm };
        }
        else
        {
            // 显示登录界面
            System.Diagnostics.Debug.WriteLine($"[UpdateContent] Showing login view, LoginViewModel is null: {vm.LoginViewModel == null}");
            MainContentControl.Content = new LoginView { DataContext = vm.LoginViewModel };
        }
        
        System.Diagnostics.Debug.WriteLine($"[UpdateContent] ContentControl content type: {MainContentControl.Content?.GetType().Name ?? "null"}");
    }
}
