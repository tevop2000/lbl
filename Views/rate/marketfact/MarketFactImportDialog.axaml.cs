using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.Views.rate.marketfact
{
    public partial class MarketFactImportDialog : Window
    {
        private string? _selectedFilePath;

        public MarketFactImportDialog()
        {
            InitializeComponent();
            InitializeYearComboBox();
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

            try
            {
                ConfirmButton.IsEnabled = false;
                ConfirmButton.Content = "导入中...";

                using var content = new MultipartFormDataContent();
                var fileStream = File.OpenRead(_selectedFilePath);
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(_selectedFilePath));
                content.Add(new StringContent("true"), "updateSupport");
                content.Add(new StringContent(year.ToString()), "year");

                var apiClient = AgentManagement.Avalonia.Utils.NewApiClient.GetHttpClient();
                var response = await apiClient.PostAsync($"{AgentManagement.Avalonia.Utils.NewApiClient.BaseUrl}/rate/agentresources/importResources", content);

                fileStream.Close();

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Logger.Info($"Excel导入成功! 年份: {year}");
                    Logger.Debug($"响应内容: {responseContent}");

                    await MessageDialog.ShowSuccessAsync(this, "导入成功", "数据导入成功！");
                    Close();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.Error($"Excel导入失败: {(int)response.StatusCode} - {errorContent}");
                    await MessageDialog.ShowErrorAsync(this, "导入失败", $"导入失败: {(int)response.StatusCode}\n{errorContent}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("导入Excel时出错", ex);
                await MessageDialog.ShowErrorAsync(this, "错误", $"导入失败: {ex.Message}");
            }
            finally
            {
                ConfirmButton.IsEnabled = true;
                ConfirmButton.Content = "✓ 确认导入";
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
