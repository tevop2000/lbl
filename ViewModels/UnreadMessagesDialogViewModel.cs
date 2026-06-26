using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Services;

namespace AgentManagement.Avalonia.ViewModels
{
    public partial class UnreadMessagesDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<MessageWithStatusVO> _messages = new ObservableCollection<MessageWithStatusVO>();

        [ObservableProperty]
        private bool _isLoading = false;

        // 消息操作的回调，用于通知外部更新未读数量
        public event Action? OnMessageRead;

        public UnreadMessagesDialogViewModel()
        {
        }

        public async Task LoadMessagesAsync()
        {
            try
            {
                IsLoading = true;
                Logger.Separator("加载未读消息列表");

                var result = await MessageService.GetUnreadMessagesAsync();
                if (result.Success)
                {
                    Messages.Clear();
                    foreach (var message in result.Data)
                    {
                        Messages.Add(message);
                    }
                    Logger.Success($"加载未读消息成功，共 {Messages.Count} 条");
                }
                else
                {
                    Logger.Warning($"加载未读消息失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载未读消息异常: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task MarkAsReadAsync(MessageWithStatusVO message)
        {
            if (message == null)
                return;

            try
            {
                Logger.Separator($"标记消息为已读，messageId: {message.Id}");

                var result = await MessageService.MarkAsReadAsync(message.Id);
                if (result.Success)
                {
                    // 从列表中移除该消息
                    Messages.Remove(message);
                    Logger.Success($"标记消息已读成功，messageId: {message.Id}");
                    
                    // 通知外部更新未读数量
                    OnMessageRead?.Invoke();
                }
                else
                {
                    Logger.Warning($"标记消息已读失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"标记消息已读异常: {ex.Message}", ex);
            }
        }
    }
}
