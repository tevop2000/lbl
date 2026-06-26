using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Models;

namespace AgentManagement.Avalonia.ViewModels.rate.supervise
{
    public partial class UrgeMessageDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _dialogTitle = "催办提醒";

        [ObservableProperty]
        private string _reminderText = string.Empty;

        [ObservableProperty]
        private string _remarkContent = string.Empty;

        [ObservableProperty]
        private long _userId;

        [ObservableProperty]
        private string _userName = string.Empty;

        [ObservableProperty]
        private bool _isPredictionTab;

        public event Action? SendSuccess;

        public UrgeMessageDialogViewModel()
        {
        }

        partial void OnIsPredictionTabChanged(bool value)
        {
            UpdateDialogTitle();
            UpdateReminderText();
        }

        partial void OnUserNameChanged(string value)
        {
            UpdateReminderText();
        }

        private void UpdateDialogTitle()
        {
            DialogTitle = IsPredictionTab ? "催办 - 预测填报" : "催办 - 实际数据导入";
        }

        private void UpdateReminderText()
        {
            var taskType = IsPredictionTab ? "预测填报" : "实际数据导入";
            ReminderText = $"即将提醒 {UserName} 尽快完成「{taskType}」。";
        }

        public async Task SendUrgeMessageAsync()
        {
            try
            {
                Logger.Separator("发送催办消息");
                Logger.Info($"开始发送催办消息 - userId: {UserId}, userName: {UserName}, isPrediction: {IsPredictionTab}");

                var businessType = IsPredictionTab ? "PREDICT" : "ACTUAL";
                var title = IsPredictionTab ? "预测填报催办" : "实际数据导入催办";

                var requestData = new
                {
                    title = title,
                    content = RemarkContent,
                    receiverId = UserId,
                    businessType = businessType
                };

                Logger.Info($"请求数据: title={title}, content={RemarkContent}, receiverId={UserId}, businessType={businessType}");

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/system/message/sendCurge",
                    requestData);

                if (response.Code == 200)
                {
                    Logger.Success("催办消息发送成功");
                    SendSuccess?.Invoke();
                }
                else
                {
                    Logger.Error($"催办消息发送失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"发送催办消息异常: {ex.Message}", ex);
            }
        }
    }
}
