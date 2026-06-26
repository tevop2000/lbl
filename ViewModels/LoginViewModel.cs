using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Views;

namespace AgentManagement.Avalonia.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = "就绪";

        public LoginViewModel()
        {
            // 尝试加载记住的用户名
            LoadRememberedUsername();
            //LoginAsync();
        }

        private void LoadRememberedUsername()
        {
            try
            {
                // TODO: 从配置文件或注册表加载记住的用户名
                // 这里可以先设置为空，让用户手动输入
            }
            catch (Exception ex)
            {
                Logger.Error($"加载记住的用户名失败: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            //Username = "董浩良";
            //Password = "12345678";
            // 验证输入
            if (string.IsNullOrWhiteSpace(Username))
            {
                ShowError("请输入用户名");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowError("请输入密码");
                return;
            }

            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;
                StatusMessage = "正在登录...";

                Logger.Info($"开始登录 - 用户名: {Username}");

                // 调用登录服务
                var result = await AuthService.LoginAsync(Username, Password);

                if (result.Success)
                {
                    Logger.Success("登录成功！");
                    StatusMessage = "登录成功，正在跳转...";

                    // 如果选择了记住我，保存用户名
                    if (RememberMe)
                    {
                        SaveRememberedUsername();
                    }

                    // 短暂延迟显示成功消息
                    await Task.Delay(500);
                    
                    // 通知主窗口登录成功（切换界面）
                    if (MainWindow.Instance?.DataContext is MainWindowViewModel mainVm)
                    {
                        mainVm.OnLoginSuccess();
                    }
                    
                    Logger.Info("已切换到主界面");
                }
                else
                {
                    ShowError(result.Message ?? "登录失败，请检查用户名和密码");
                    StatusMessage = "就绪";
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"登录异常: {ex.Message}", ex);
                ShowError($"登录异常: {ex.Message}");
                StatusMessage = "就绪";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowError(string message)
        {
            HasError = true;
            ErrorMessage = message;
            Logger.Error(message);
        }

        private void SaveRememberedUsername()
        {
            try
            {
                // TODO: 保存用户名到配置文件或注册表
                Logger.Info($"保存记住的用户名: {Username}");
            }
            catch (Exception ex)
            {
                Logger.Error($"保存记住的用户名失败: {ex.Message}", ex);
            }
        }
    }
}
