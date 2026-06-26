using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.ViewModels.rate.supervise;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AgentManagement.Avalonia.Views.rate.supervise
{
    public partial class UrgeMessageDialog : Window
    {
        public UrgeMessageDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void SendButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is UrgeMessageDialogViewModel viewModel)
            {
                // 先禁用按钮防止重复点击
                var button = sender as Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                }

                try
                {
                    await viewModel.SendUrgeMessageAsync();
                    // 发送成功后关闭对话框
                    Close();
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"发送催办消息失败: {ex.Message}", ex);
                    var box = MessageBoxManager.GetMessageBoxStandard("发送失败", $"发送失败: {ex.Message}", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
                finally
                {
                    // 恢复按钮状态
                    if (button != null)
                    {
                        button.IsEnabled = true;
                    }
                }
            }
        }
    }
}