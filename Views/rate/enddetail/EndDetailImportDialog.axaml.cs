using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.Views.rate.enddetail
{
    public partial class EndDetailImportDialog : Window
    {
        private string? _selectedFilePath;
        private CancellationTokenSource? _loadingCts;
        private bool _isLoading;
        
        public string ImportApiPath { get; set; } = "/rate/saletarget/importData";

        public EndDetailImportDialog()
        {
            InitializeComponent();
            InitializeYearComboBox();
        }
        
        public EndDetailImportDialog(string importApiPath) : this()
        {
            ImportApiPath = importApiPath;
        }

        private void InitializeYearComboBox()
        {
            var currentYear = DateTime.Now.Year;
            var years = new List<int>
            {
                currentYear - 1,
                currentYear,
                currentYear + 1
            };

            YearComboBox.ItemsSource = years;
            YearComboBox.SelectedItem = currentYear;
        }

        private async void UploadArea_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "选择Excel文件",
                    AllowMultiple = false,
                    FileTypeFilter = new List<FilePickerFileType>
                    {
                        new FilePickerFileType("Excel文件") { Patterns = new[] { "*.xlsx", "*.xls" } },
                        new FilePickerFileType("所有文件") { Patterns = new[] { "*.*" } }
                    }
                });

                if (files.Count == 0) return;

                _selectedFilePath = files[0].Path.LocalPath;
                SelectedFileNameText.Text = $"已选择: {Path.GetFileName(_selectedFilePath)}";
                ConfirmButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.Error("选择文件时出错", ex);
                await MessageDialog.ShowErrorAsync(this, "错误", $"选择文件失败: {ex.Message}");
            }
        }

        private async void ConfirmButton_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                await MessageDialog.ShowErrorAsync(this, "错误", "请先选择有效的Excel文件");
                return;
            }

            if (YearComboBox.SelectedItem is not int year)
            {
                await MessageDialog.ShowErrorAsync(this, "错误", "请选择年份");
                return;
            }

            bool importSuccess = false;
            
            try
            {
                // 显示加载状态
                ShowLoading(true);
                LoadingStatusText.Text = "正在上传文件...";
                LoadingHintText.Text = "请稍候，文件正在上传到服务器";

                using var content = new MultipartFormDataContent();
                var fileStream = File.OpenRead(_selectedFilePath);
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(_selectedFilePath));
                content.Add(new StringContent("true"), "updateSupport");
                content.Add(new StringContent(year.ToString()), "year");

                var apiClient = AgentManagement.Avalonia.Utils.NewApiClient.GetHttpClient();
                
                // 更新状态为正在处理
                Dispatcher.UIThread.Post(() =>
                {
                    LoadingStatusText.Text = "正在处理数据...";
                    LoadingHintText.Text = "服务器正在解析您的Excel文件";
                });

                var response = await apiClient.PostAsync($"{AgentManagement.Avalonia.Utils.NewApiClient.BaseUrl}{ImportApiPath}", content);

                fileStream.Close();

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Logger.Info($"Excel导入成功! 年份: {year}");
                    Logger.Debug($"响应内容: {responseContent}");

                    string summary = "数据导入成功！";
                    importSuccess = true;
                    
                    // 先隐藏加载状态，再显示成功提示
                    ShowLoading(false);
                    await MessageDialog.ShowSuccessAsync(this, "导入成功", summary);
                    Close();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.Error($"Excel导入失败: {(int)response.StatusCode} - {errorContent}");
                    ShowLoading(false);
                    await MessageDialog.ShowErrorAsync(this, "导入失败", $"导入失败: {(int)response.StatusCode}\n{errorContent}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("导入Excel时出错", ex);
                ShowLoading(false);
                await MessageDialog.ShowErrorAsync(this, "错误", $"导入失败: {ex.Message}");
            }
            finally
            {
                // 如果不是成功的情况，确保隐藏加载状态
                if (!importSuccess)
                {
                    ShowLoading(false);
                }
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!_isLoading)
            {
                Close();
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!_isLoading)
            {
                Close();
            }
        }

        private void ShowLoading(bool isLoading)
        {
            _isLoading = isLoading;

            // 更新 UI
            Dispatcher.UIThread.Post(() =>
            {
                LoadingOverlay.IsVisible = isLoading;
                
                // 禁用其他交互控件
                ConfirmButton.IsEnabled = !isLoading;
                CancelButton.IsEnabled = !isLoading;
                YearComboBox.IsEnabled = !isLoading;
                
                // 控制旋转动画
                if (isLoading)
                {
                    StartLoadingAnimation();
                }
                else
                {
                    StopLoadingAnimation();
                }
            });
        }

        private async void StartLoadingAnimation()
        {
            _loadingCts = new CancellationTokenSource();
            var token = _loadingCts.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (LoadingEllipse.RenderTransform is RotateTransform rotateTransform)
                        {
                            rotateTransform.Angle = (rotateTransform.Angle + 10) % 360;
                        }
                    });
                    
                    await Task.Delay(50, token);
                }
            }
            catch (OperationCanceledException)
            {
                // 动画被取消
            }
        }

        private void StopLoadingAnimation()
        {
            _loadingCts?.Cancel();
            _loadingCts = null;
            if (LoadingEllipse.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.Angle = 0;
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (_isLoading)
            {
                e.Cancel = true;
            }
            else
            {
                StopLoadingAnimation();
                base.OnClosing(e);
            }
        }
    }
}
