using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.ViewModels.Controls;
using AgentManagement.Avalonia.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AgentManagement.Avalonia.ViewModels.rate.supervise
{
    public partial class indexViewModel : ObservableObject
    {
        [ObservableProperty]
        private string currentView = "当前视角：战区总 · 战区";

        [ObservableProperty]
        private int channelCount = 1;

        [ObservableProperty]
        private int businessManagerCount = 0;

        [ObservableProperty]
        private int predictionAgentCount = 0;

        [ObservableProperty]
        private double completionRate = 0;

        [ObservableProperty]
        private string completionRateInfo = "覆盖 0 个代理商 × 12 月";

        [ObservableProperty]
        private double importRate = 0;

        [ObservableProperty]
        private string importRateInfo = "4 个已过月份";

        [ObservableProperty]
        private int urgentManagerCount = 0;

        [ObservableProperty]
        private string urgentManagerInfo = "任一维度 < 100%";

        [ObservableProperty]
        private bool isPredictionTabSelected = true;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string filterStatus = "全部";

        [ObservableProperty]
        private long? currentAgentId = null;

        [ObservableProperty]
        private string currentAgentName = "未选择";

        public string PredictionTabTextColor => IsPredictionTabSelected ? "#3B82F6" : "#64748B";
        public string PredictionTabIndicatorColor => IsPredictionTabSelected ? "#3B82F6" : "Transparent";
        public string ImportTabTextColor => IsPredictionTabSelected ? "#64748B" : "#3B82F6";
        public string ImportTabIndicatorColor => IsPredictionTabSelected ? "Transparent" : "#3B82F6";

        public string AllFilterTextColor => FilterStatus == "全部" ? "#FFFFFF" : "#64748B";
        public string AllFilterBackgroundColor => FilterStatus == "全部" ? "#3B82F6" : "#F1F5F9";
        public string CompletedFilterTextColor => FilterStatus == "已完成" ? "#FFFFFF" : "#64748B";
        public string CompletedFilterBackgroundColor => FilterStatus == "已完成" ? "#3B82F6" : "#F1F5F9";

        public ObservableCollection<SuperviseItem> PredictionItems { get; } = new ObservableCollection<SuperviseItem>();
        public ObservableCollection<SuperviseItem> ImportItems { get; } = new ObservableCollection<SuperviseItem>();
        
        public ObservableCollection<SuperviseItem> CurrentItems
        {
            get
            {
                var items = IsPredictionTabSelected ? PredictionItems : ImportItems;
                if (FilterStatus == "已完成")
                {
                    return new ObservableCollection<SuperviseItem>(items.Where(x => x.Status == "已完成"));
                }
                return items;
            }
        }

        private int _currentYear = DateTime.Now.Year;

        public indexViewModel()
        {
            // 构造函数中不加载数据，等待选择器初始化后回调
        }

        /// <summary>
        /// 当代理商选择变化时调用
        /// </summary>
        public async Task OnAgentChangedAsync(AgentItem? agent, AgentUser? manager, DeptInfo? region, DeptInfo? channel, DeptInfo? warZone)
        {
            long? agentId = agent?.AgentId;

            // 更新显示的名称 - 优先使用业务经理的昵称
            if (agent != null)
            {
                CurrentAgentName = agent.AgentName;
            }
            else
            {
                CurrentAgentName = "未选择";
            }
            
            CurrentAgentId = agentId;
            
            // 只有当 agentId 有效时才加载数据
            if (agentId.HasValue && agentId.Value > 0)
            {
                await LoadDataAsync();
            }
            else
            {
                // 清空所有数据
                //ClearData();
            }
        }

        /// <summary>
        /// 初始化加载数据（当页面首次加载时调用）
        /// </summary>
        public async Task InitializeDataAsync()
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        private void ClearData()
        {
            PredictionItems.Clear();
            ImportItems.Clear();
            BusinessManagerCount = 0;
            PredictionAgentCount = 0;
            CompletionRate = 0;
            CompletionRateInfo = "覆盖 0 个代理商 × 12 月";
            ImportRate = 0;
            UrgentManagerCount = 0;
            OnPropertyChanged(nameof(CurrentItems));
        }

        partial void OnIsPredictionTabSelectedChanged(bool value)
        {
            OnPropertyChanged(nameof(PredictionTabTextColor));
            OnPropertyChanged(nameof(PredictionTabIndicatorColor));
            OnPropertyChanged(nameof(ImportTabTextColor));
            OnPropertyChanged(nameof(ImportTabIndicatorColor));
            OnPropertyChanged(nameof(CurrentItems));
        }

        partial void OnFilterStatusChanged(string value)
        {
            OnPropertyChanged(nameof(AllFilterTextColor));
            OnPropertyChanged(nameof(AllFilterBackgroundColor));
            OnPropertyChanged(nameof(CompletedFilterTextColor));
            OnPropertyChanged(nameof(CompletedFilterBackgroundColor));
            OnPropertyChanged(nameof(CurrentItems));
        }

        [RelayCommand]
        private void SetFilterStatus(string status)
        {
            FilterStatus = status;
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {

            if (IsLoading) return;
            try
            {
                IsLoading = true;
                Logger.Separator("加载督办中心数据");

                int? predictionNeedUrgeCount = null;
                int? importNeedUrgeCount = null;

                var predictionTask = LoadPredictionDataAsync();
                var importTask = LoadImportDataAsync();

                await Task.WhenAll(predictionTask, importTask);

                predictionNeedUrgeCount = await predictionTask;
                importNeedUrgeCount = await importTask;

                if (predictionNeedUrgeCount.HasValue || importNeedUrgeCount.HasValue)
                {
                    UrgentManagerCount = (predictionNeedUrgeCount ?? 0) + (importNeedUrgeCount ?? 0);
                }

                Logger.Success("加载督办中心数据完成");
                Logger.Separator("加载督办中心数据结束");
            }
            catch (Exception ex)
            {
                Logger.Error($"加载数据异常: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<int?> LoadPredictionDataAsync()
        {

            try
            {
                Logger.Info("开始加载预测填报数据");
                System.Diagnostics.Debug.WriteLine("[supervise] 开始加载预测填报数据");
                var queryParams = CurrentAgentId.HasValue
                ? $"year={_currentYear}&agentId={CurrentAgentId.Value}"
                : $"year={_currentYear}";

                var response = await NewApiClient.GetAsync<PredictionCompletionStatsResponse>(
                    $"/rate/detail/getPredictionCompletionRate?{queryParams}");

                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data;
                    BusinessManagerCount = data.ManagerCount;
                    PredictionAgentCount = data.AgentCount;
                    CompletionRate = (double)data.TotalCompletionRate;
                    CompletionRateInfo = $"覆盖 {data.AgentCount} 个代理商 × 12 月";

                    PredictionItems.Clear();
                    if (data.DetailList != null)
                    {
                        foreach (var item in data.DetailList)
                        {
                            var parts = item.DeptFullPath?.Split('-');
                            var channelName = parts?.Length > 0 ? parts[0] : "未知渠道";
                            var region = parts?.Length > 1 ? parts[1] : "未知战区";

                            var progress = (double)item.CompletionRate;
                            var status = progress >= 100 ? "已完成" : (progress >= 50 ? "进行中" : "待处理");
                            var statusType = progress >= 100 ? StatusType.Completed : (progress >= 50 ? StatusType.InProgress : StatusType.Urgent);

                            var superviseItem = new SuperviseItem
                            {
                                ChannelName = channelName,
                                Region = region,
                                UserName = item.UserName,
                                UserId = item.UserId,
                                Year = _currentYear,
                                BusinessManagerCount = 1,
                                AgentCount = item.AgentCount,
                                Progress = (int)Math.Round(progress),
                                ProgressText = $"{item.CompletedMonths}/{item.TotalMonths} · {progress}%",
                                Status = status,
                                StatusType = statusType,
                                IsPredictionTab = true
                            };
                            superviseItem.LoadDetailMonths(item.TotalMonths, item.CompletedMonths);
                            PredictionItems.Add(superviseItem);
                        }
                    }

                    Logger.Success($"预测填报数据加载完成: 业务经理{data.ManagerCount}个, 代理商{data.AgentCount}个, 完成率{data.TotalCompletionRate}%");
                    OnPropertyChanged(nameof(CurrentItems));
                    return data.NeedUrgeCount;
                }
                else
                {
                    Logger.Error($"加载预测填报数据失败: {response.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载预测填报数据异常: {ex.Message}", ex);
                return null;
            }
        }

        private async Task<int?> LoadImportDataAsync()
        {

            try
            {
                Logger.Info("开始加载实际数据导入数据");

                var queryParams = CurrentAgentId.HasValue
                ? $"year={_currentYear}&agentId={CurrentAgentId.Value}"
                : $"year={_currentYear}";

                var response = await NewApiClient.GetAsync<AnalysisCompletionStatsResponse>(
                    $"/rate/enddetail/getAnalysisCompletionRate?{queryParams}");

                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data;
                    ImportRate = (double)data.QuarterCompletionRate;
                    ImportRateInfo = "4 个已过月份";

                    ImportItems.Clear();
                    if (data.DetailList != null)
                    {
                        foreach (var item in data.DetailList)
                        {
                            var parts = item.DeptFullPath?.Split('-');
                            var channelName = parts?.Length > 0 ? parts[0] : "未知渠道";
                            var region = parts?.Length > 1 ? parts[1] : "未知战区";

                            var progress = (double)item.CompletionRate;
                            var status = progress >= 100 ? "已完成" : (progress >= 50 ? "进行中" : "待处理");
                            var statusType = progress >= 100 ? StatusType.Completed : (progress >= 50 ? StatusType.InProgress : StatusType.Urgent);

                            var superviseItem = new SuperviseItem
                            {
                                ChannelName = channelName,
                                Region = region,
                                UserName = item.UserName,
                                UserId = item.UserId,
                                Year = _currentYear,
                                BusinessManagerCount = 1,
                                AgentCount = item.AgentCount,
                                Progress = (int)Math.Round(progress),
                                ProgressText = $"{item.CompletedMonths}/{item.TotalMonths} · {progress:F1}%",
                                Status = status,
                                StatusType = statusType,
                                IsPredictionTab = false
                            };
                            superviseItem.LoadDetailMonths(item.TotalMonths, item.CompletedMonths);
                            ImportItems.Add(superviseItem);
                        }
                    }

                    Logger.Success($"实际数据导入数据加载完成: 完成率{data.QuarterCompletionRate}%");
                    OnPropertyChanged(nameof(CurrentItems));
                    return data.NeedUrgeCount;
                }
                else
                {
                    Logger.Error($"加载实际数据导入数据失败: {response.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载实际数据导入数据异常: {ex.Message}", ex);
                return null;
            }
        }

        public partial class SuperviseItem : ObservableObject
        {
            [ObservableProperty]
            private string channelName = string.Empty;

            [ObservableProperty]
            private string region = string.Empty;

            [ObservableProperty]
            private string userName = string.Empty;

            [ObservableProperty]
            private int businessManagerCount;

            [ObservableProperty]
            private int agentCount;

            [ObservableProperty]
            private int progress;

            [ObservableProperty]
            private string progressText = string.Empty;

            [ObservableProperty]
            private string status = string.Empty;

            [ObservableProperty]
            private StatusType statusType;

            [ObservableProperty]
            private bool isPredictionTab;

            [ObservableProperty]
            private bool isExpanded;

            [ObservableProperty]
            private ObservableCollection<DetailMonthItem> detailMonths = new ObservableCollection<DetailMonthItem>();

            [ObservableProperty]
            private long userId;

            [ObservableProperty]
            private bool isLoadingDetail;

            [ObservableProperty]
            private int year;

            [ObservableProperty]
            private string agentName = string.Empty;

            [ObservableProperty]
            private double detailCompletionRate;

            [ObservableProperty]
            private ObservableCollection<SuperviseAgentItem> agentItems = new ObservableCollection<SuperviseAgentItem>();

            public string ExpandButtonText => IsExpanded ? "∨" : "→";

            public bool ShowUrgeButton => Progress < 100;

        public string ProgressColor => "#10B981";

        [RelayCommand]
        private async Task ShowUrgeDialogAsync()
        {
            var mainWindow = App.MainWindow;
            if (mainWindow == null)
            {
                Logger.Warning("无法获取主窗口，无法显示催办对话框");
                return;
            }

            var dialog = new Views.rate.supervise.UrgeMessageDialog();
            var dialogViewModel = new UrgeMessageDialogViewModel
            {
                UserId = this.UserId,
                UserName = this.UserName,
                IsPredictionTab = this.IsPredictionTab
            };
            dialog.DataContext = dialogViewModel;
            
            // 监听发送成功事件
            dialogViewModel.SendSuccess += async () =>
            {
                await ShowSuccessMessageAsync();
            };
            
            await dialog.ShowDialog(mainWindow);
        }

        private async Task ShowSuccessMessageAsync()
        {
            var box = MessageBoxManager.GetMessageBoxStandard("发送成功", "催办提醒已发送成功！", ButtonEnum.Ok, Icon.Success);
            await box.ShowAsync();
        }

            public string StatusBackground
            {
                get
                {
                    return StatusType switch
                    {
                        StatusType.Completed => "#ECFDF5",
                        StatusType.InProgress => "#FFFBEB",
                        StatusType.Urgent => "#FEF2F2",
                        _ => "#F1F5F9"
                    };
                }
            }

            public string StatusColor
            {
                get
                {
                    return StatusType switch
                    {
                        StatusType.Completed => "#059669",
                        StatusType.InProgress => "#F59E0B",
                        StatusType.Urgent => "#EF4444",
                        _ => "#64748B"
                    };
                }
            }

            public void LoadDetailMonths(int totalMonths, int completedMonths)
            {
                DetailMonths.Clear();
                for (int i = 1; i <= totalMonths; i++)
                {
                    DetailMonths.Add(new DetailMonthItem
                    {
                        Month = i,
                        IsCompleted = i <= completedMonths
                    });
                }
            }

            [RelayCommand]
            private async Task LoadDetailMonthsFromApi()
            {
                try
                {
                    IsLoadingDetail = true;
                    Logger.Separator("获取业务经理月份详情");
                    
                    var endpoint = IsPredictionTab 
                        ? "/rate/detail/getAgentMonthCompletion" 
                        : "/rate/enddetail/getAgentMonthCompletion";
                    
                    Logger.Info($"调用接口: {endpoint}, userId: {UserId}, year: {Year}");

                    var response = await NewApiClient.GetAsync<List<AgentMonthCompletionVO>>($"{endpoint}?userId={UserId}&year={Year}");

                    if (response.Code == 200 && response.Data != null)
                    {
                        Logger.Success("获取业务经理月份详情成功");
                        
                        AgentItems.Clear();
                        foreach (var data in response.Data)
                        {
                            var agentItem = new SuperviseAgentItem
                            {
                                AgentName = data.AgentName,
                                CompletionRate = data.CompletionRate
                            };
                            
                            var monthDict = data.MonthCompletion ?? new Dictionary<string, bool>();
                            
                            for (int month = 1; month <= 12; month++)
                            {
                                var monthKey = $"{Year}-{month:D2}";
                                var isCompleted = monthDict.TryGetValue(monthKey, out bool completed) && completed;
                                
                                agentItem.Months.Add(new DetailMonthItem
                                {
                                    Month = month,
                                    IsCompleted = isCompleted
                                });
                            }
                            
                            AgentItems.Add(agentItem);
                        }
                        
                        Logger.Separator("获取业务经理月份详情完成");
                    }
                    else
                    {
                        Logger.Error($"获取业务经理月份详情失败: {response.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"获取业务经理月份详情异常: {ex.Message}", ex);
                }
                finally
                {
                    IsLoadingDetail = false;
                }
            }
        }

        public partial class SuperviseAgentItem : ObservableObject
        {
            [ObservableProperty]
            private string agentName = string.Empty;

            [ObservableProperty]
            private double completionRate;

            [ObservableProperty]
            private ObservableCollection<DetailMonthItem> months = new ObservableCollection<DetailMonthItem>();
        }

        public partial class DetailMonthItem : ObservableObject
        {
            [ObservableProperty]
            private int month;

            [ObservableProperty]
            private bool isCompleted;

            public string BackgroundColor => IsCompleted ? "#10B981" : "#E2E8F0";

            public string TextColor => IsCompleted ? "#FFFFFF" : "#64748B";
        }

        public enum StatusType
        {
            Completed,
            InProgress,
            Urgent
        }

        #region API响应模型

        private class PredictionCompletionStatsResponse
        {
            public int ManagerCount { get; set; }
            public int AgentCount { get; set; }
            public decimal TotalCompletionRate { get; set; }
            public int NeedUrgeCount { get; set; }
            public List<PredictionCompletionRateItem>? DetailList { get; set; }
        }

        private class AnalysisCompletionStatsResponse
        {
            public decimal QuarterCompletionRate { get; set; }
            public int NeedUrgeCount { get; set; }
            public List<PredictionCompletionRateItem>? DetailList { get; set; }
        }

        private class PredictionCompletionRateItem
        {
            public long UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public int AgentCount { get; set; }
            public int TotalMonths { get; set; }
            public int CompletedMonths { get; set; }
            public decimal CompletionRate { get; set; }
            public string DeptFullPath { get; set; } = string.Empty;
        }

        private class AgentMonthCompletionVO
        {
            public long AgentId { get; set; }
            public string AgentName { get; set; } = string.Empty;
            public Dictionary<string, bool>? MonthCompletion { get; set; }
            public int CompletedMonths { get; set; }
            public int TotalMonths { get; set; }
            public double CompletionRate { get; set; }
        }

        #endregion
    }
}
