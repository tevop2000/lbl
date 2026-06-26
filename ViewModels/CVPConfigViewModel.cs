using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.ViewModels
{
    /// <summary>
    /// 代理商信息模型
    /// </summary>
    public class AgentInfo : ObservableObject
    {
        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _productCount;
        public int ProductCount
        {
            get => _productCount;
            set => SetProperty(ref _productCount, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public partial class CVPConfigViewModel : ViewModelBase
    {
        /// <summary>
        /// 代理商列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<AgentInfo> _agentList = new();

        /// <summary>
        /// 当前选中的代理商ID
        /// </summary>
        [ObservableProperty]
        private string _selectedAgentId = "";

        /// <summary>
        /// 当前选中的代理商信息
        /// </summary>
        [ObservableProperty]
        private AgentInfo? _currentAgent;

        [ObservableProperty]
        private string _targetSales = "1000";

        [ObservableProperty]
        private string _actualSales = "850";

        [ObservableProperty]
        private string _batteryCost = "5000";

        [ObservableProperty]
        private string _rentCost = "8000";

        [ObservableProperty]
        private string _salaryCost = "12000";

        [ObservableProperty]
        private string _vehicleCost = "3000";

        /// <summary>
        /// 加载代理商列表（从API获取）
        /// </summary>
        public async Task LoadAgentsAsync(string selectedAgentId = "")
        {
            try
            {
                Logger.Info("开始加载代理商列表...");
                
                var response = await NewApiClient.GetAsync<ObservableCollection<Models.AgentUser>>("/rate/default/getMyAgentListWithCount");
                
                if (response.Code == 200 && response.Data != null)
                {
                    AgentList.Clear();
                    
                    foreach (var agent in response.Data)
                    {
                        var agentInfo = new AgentInfo
                        {
                            Id = agent.Id.ToString(),
                            Name = agent.DisplayName,
                            ProductCount = agent.ProductDefaultCount,
                            IsSelected = agent.Id.ToString() == selectedAgentId
                        };
                        AgentList.Add(agentInfo);
                    }

                    SelectedAgentId = selectedAgentId;
                    
                    // 设置当前选中的代理商
                    if (!string.IsNullOrEmpty(selectedAgentId))
                    {
                        CurrentAgent = AgentList.FirstOrDefault(a => a.Id == selectedAgentId);
                        if (CurrentAgent != null)
                        {
                            Logger.Info($"当前选中代理商: {CurrentAgent.Name}");
                        }
                    }
                    
                    Logger.Success($"成功加载 {AgentList.Count} 个代理商");
                }
                else
                {
                    Logger.Error($"加载代理商列表失败: {response.Message}");
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"加载代理商列表异常: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 选择代理商
        /// </summary>
        [RelayCommand]
        private void SelectAgent(string agentId)
        {
            foreach (var agent in AgentList)
            {
                agent.IsSelected = agent.Id == agentId;
            }
            SelectedAgentId = agentId;
            Logger.Info($"产品配置管理 - 选中代理商: {agentId}");
        }

        /// <summary>
        /// 编辑代理商产品配置
        /// </summary>
        [RelayCommand]
        private async Task EditAgentProductsAsync(string agentId)
        {
            try
            {
                var agent = AgentList.FirstOrDefault(a => a.Id == agentId);
                if (agent == null) return;

                Logger.Info($"编辑产品配置 - 代理商: {agent.Name}");
                
                Console.WriteLine($"[DEBUG] CVPConfigViewModel - agent.Id (string): {agent.Id}");
                int parsedId = int.Parse(agent.Id);
                Console.WriteLine($"[DEBUG] CVPConfigViewModel - parsedId (int): {parsedId}");
                
                // 创建并显示产品编辑对话框
                var dialog = new Views.AgentProductEditDialog(agent.Name, parsedId);
                
                // 监听保存成功事件
                bool savedSuccessfully = false;
                dialog.SaveSuccess += (savedAgentId) =>
                {
                    savedSuccessfully = true;
                    Logger.Info($"产品配置保存成功，需要刷新代理商列表: agentId={savedAgentId}");
                };
                
                await dialog.ShowDialog(Views.MainWindow.Instance ?? throw new InvalidOperationException("主窗口未找到"));
                
                // 如果保存成功，刷新代理商列表
                if (savedSuccessfully)
                {
                    Logger.Info("开始刷新代理商列表...");
                    string currentSelectedId = SelectedAgentId; // 保存当前选中的ID
                    await LoadAgentsAsync(currentSelectedId); // 传入当前选中的ID以保持选中状态
                    Logger.Info("代理商列表刷新完成");
                    StatusMessage = "产品配置已更新";
                }
                else
                {
                    Logger.Info("产品编辑对话框已关闭（未保存）");
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"打开产品编辑对话框失败: {ex.Message}", ex);
                StatusMessage = $"打开编辑对话框失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task SaveConfigAsync()
        {
            try
            {
                Logger.Info("保存产品配置...");
                StatusMessage = "正在保存配置...";
                
                // TODO: 调用API保存配置
                
                await Task.Delay(500); // 模拟保存
                Logger.Success("配置保存成功");
                StatusMessage = "配置保存成功！";
            }
            catch (System.Exception ex)
            {
                Logger.Error($"保存配置失败: {ex.Message}", ex);
                StatusMessage = $"保存失败: {ex.Message}";
            }
        }
    }
}
