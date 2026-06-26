using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.ViewModels
{
    public partial class CrmPortalViewModel : ViewModelBase
    {
        private const string CRM_HOME_URL = "https://crm.chilwee.com/#/menu/index";
        
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isLoggedIn;

        [ObservableProperty]
        private string _statusMessage = "请输入CRM账号和密码";

        [ObservableProperty]
        private bool _isLoggingIn;

        [ObservableProperty]
        private string _loginButtonText = "登录";

        public CrmPortalViewModel()
        {
            // 检查是否已有CRM Token
            CheckExistingCrmToken();
        }

        /// <summary>
        /// 检查是否已有CRM Token，如果有则自动登录
        /// </summary>
        private void CheckExistingCrmToken()
        {
            string existingToken = Services.CrmAuthService.GetCrmToken();
            
            if (!string.IsNullOrEmpty(existingToken))
            {
                Logger.Info("检测到已有的CRM Token，尝试自动登录...");
                IsLoggedIn = true;
                StatusMessage = "已自动登录CRM系统，点击下方按钮在浏览器中打开";
            }
            else
            {
                Logger.Info("未检测到CRM Token，显示登录界面");
                IsLoggedIn = false;
            }
        }

        /// <summary>
        /// 登录CRM
        /// </summary>
        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "请输入账号和密码";
                return;
            }

            IsLoggingIn = true;
            StatusMessage = "正在登录CRM...";

            try
            {
                var result = await Services.CrmAuthService.LoginToCrmAsync(Username, Password);

                if (result.Success)
                {
                    // 保存Token
                    Services.CrmAuthService.SaveCrmToken(result.Token);
                    
                    IsLoggedIn = true;
                    StatusMessage = $"登录成功！欢迎，{result.RealName}。点击下方按钮在浏览器中打开CRM";
                    
                    Logger.Success("CRM登录成功，准备打开浏览器...");
                }
                else
                {
                    StatusMessage = $"登录失败: {result.Message}";
                    Logger.Error($"CRM登录失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"登录异常: {ex.Message}";
                Logger.Error($"CRM登录异常: {ex.Message}", ex);
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        /// <summary>
        /// 在浏览器中打开CRM
        /// </summary>
        [RelayCommand]
        private void OpenCrmInBrowser()
        {
            if (IsLoggedIn)
            {
                try
                {
                    // 使用默认浏览器打开CRM
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = CRM_HOME_URL,
                        UseShellExecute = true
                    });
                    
                    Logger.Info("已在默认浏览器中打开CRM系统");
                    StatusMessage = "已在浏览器中打开CRM系统";
                }
                catch (Exception ex)
                {
                    Logger.Error($"打开浏览器失败: {ex.Message}", ex);
                    StatusMessage = $"打开浏览器失败: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 登出CRM
        /// </summary>
        [RelayCommand]
        private void Logout()
        {
            UserInfo.Instance.SetCrmToken(string.Empty);
            IsLoggedIn = false;
            Username = string.Empty;
            Password = string.Empty;
            StatusMessage = "已登出，请重新登录";
            Logger.Info("CRM已登出");
        }
    }
}
