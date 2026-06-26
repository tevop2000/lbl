using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Models;
using AgentManagement.Avalonia.Utils;
using System.Timers;

namespace AgentManagement.Avalonia.ViewModels.Controls
{
    /// <summary>
    /// 部门-业务经理-代理商级联选择器 ViewModel
    /// </summary>
    public partial class DeptManagerAgentSelectorViewModel : ObservableObject
    {
        private UserInfoResult? _currentUser;

        [ObservableProperty]
        private ObservableCollection<DeptInfo> _departmentList = new();

        [ObservableProperty]
        private DeptInfo? _selectedDepartment;

        // 级联部门选择器属性
        [ObservableProperty]
        private DeptInfo? _selectedWarZone;

        [ObservableProperty]
        private DeptInfo? _selectedChannelDept;

        [ObservableProperty]
        private DeptInfo? _selectedRegionManager;

        [ObservableProperty]
        private ObservableCollection<AgentUser> _managerList = new();

        [ObservableProperty]
        private AgentUser? _selectedManager;

        [ObservableProperty]
        private ObservableCollection<AgentItem> _agentList = new();

        [ObservableProperty]
        private AgentItem? _selectedAgent;

        [ObservableProperty]
        private bool _isManagerReadOnly;

        [ObservableProperty]
        private string _managerDisplayText = string.Empty;

        /// <summary>
        /// 当代理商选择变化时的回调
        /// </summary>
        public Action<AgentItem?, AgentUser?, DeptInfo?, DeptInfo?, DeptInfo?>? OnAgentSelectedCallback { get; set; }

        /// <summary>
        /// 初始化选择器
        /// </summary>
        /// <param name="currentUser">当前登录用户信息</param>
        public async Task InitializeAsync(UserInfoResult currentUser)
        {
            _currentUser = currentUser;
            
            // 判断是业务经理角色
            IsManagerReadOnly = currentUser.IsAgentRole;

            if (IsManagerReadOnly)
            {
                // 代理商角色：业务经理显示当前用户名字并禁止选择
                ManagerDisplayText = currentUser.NickName;
                
                // 加载部门列表
                await LoadDepartmentListAsync();
                
                // 如果有部门信息，自动选中
                if (_currentUser.Dept != null)
                {
                    SelectedRegionManager = _currentUser.Dept;
                }
                
                // 加载代理商列表（使用当前用户 ID）
                await LoadAgentListAsync(currentUser.UserId);
            }
            else
            {
                // 非代理商角色：只加载部门列表，不加载业务经理列表
                // 等待用户选择部门后，再根据部门 ID 加载业务经理
                await LoadDepartmentListAsync();
                // 注意：不设置任何默认值，让用户手动选择
            }
        }

        /// <summary>
        /// 当用户完成部门选择后调用此方法加载业务经理
        /// </summary>
        public async Task LoadManagersAfterDeptSelectionAsync()
        {
            if (_currentUser?.IsAgentRole == true)
            {
                return; // agent 角色不需要加载
            }

            // 优先使用选中的大区，其次是渠道部，最后是战区
            var selectedDept = SelectedRegionManager ?? SelectedChannelDept ?? SelectedWarZone;
            
            if (selectedDept != null)
            {
                System.Diagnostics.Debug.WriteLine($"DeptManagerAgentSelector: 开始加载部门 {selectedDept.Id} 的业务经理列表...");
                
                // 清空之前的选择
                SelectedManager = null;
                SelectedAgent = null;
                ManagerList.Clear();
                AgentList.Clear();
                
                await LoadManagerListByDeptIdAsync(selectedDept.Id);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DeptManagerAgentSelector: 未选中任何部门，跳过加载业务经理");
            }
        }

        /// <summary>
        /// 加载部门列表
        /// </summary>
        private async Task LoadDepartmentListAsync()
        {
            try
            {
                Logger.Info("开始加载部门列表...");
                
                var response = await NewApiClient.GetAsync<dynamic>("/system/dept/list");
                
                if (response.Code == 200 && response.Data != null)
                {
                    DepartmentList.Clear();
                    
                    var data = response.Data as Newtonsoft.Json.Linq.JArray;
                    if (data != null)
                    {
                        foreach (var item in data)
                        {
                            var dept = new DeptInfo
                            {
                                Id = item["deptId"]?.ToObject<long>() ?? 0,
                                DeptName = item["deptName"]?.ToString() ?? string.Empty,
                                ParentId = item["parentId"]?.ToObject<long>() ?? 0,
                                OrderNum = item["orderNum"]?.ToObject<int>() ?? 0
                            };
                            DepartmentList.Add(dept);
                        }
                    }
                    
                    Logger.Success($"部门列表加载成功，共 {DepartmentList.Count} 条");
                }
                else
                {
                    Logger.Warning($"加载部门列表失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载部门列表失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 加载业务经理列表
        /// </summary>
        private async Task LoadManagerListAsync()
        {
            try
            {
                Logger.Info("开始加载业务经理列表...");
                
                // TODO: 调用业务经理接口，可能需要根据角色过滤
                var response = await NewApiClient.GetAsync<dynamic>("/system/user/list?pageNum=1&pageSize=1000");
                
                if (response.Code == 200 && response.Data != null)
                {
                    ManagerList.Clear();
                    
                    var rows = response.Data["rows"] as Newtonsoft.Json.Linq.JArray;
                    if (rows != null)
                    {
                        foreach (var item in rows)
                        {
                            var manager = new AgentUser
                            {
                                UserId = item["userId"]?.ToObject<int>() ?? 0,
                                AgentName = item["userName"]?.ToString() ?? string.Empty,
                                NickName = item["nickName"]?.ToString() ?? string.Empty
                            };
                            ManagerList.Add(manager);
                        }
                    }
                    
                    Logger.Success($"业务经理列表加载成功，共 {ManagerList.Count} 条");
                }
                else
                {
                    Logger.Warning($"加载业务经理列表失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载业务经理列表失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据部门 ID 加载业务经理列表
        /// </summary>
        /// <param name="deptId">部门 ID</param>
        private async Task LoadManagerListByDeptIdAsync(long deptId)
        {
            try
            {
                Logger.Info($"开始加载部门 {deptId} 的业务经理列表...");
                
                // 直接调用接口获取原始 JSON
                var httpClient = new System.Net.Http.HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", NewApiClient.GetAuthToken());
                
                var response = await httpClient.GetAsync($"{NewApiClient.BaseUrl}/system/user/list?deptId={deptId}&pageNum=1&pageSize=1000");
                var content = await response.Content.ReadAsStringAsync();
                
                // 解析 JSON
                var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(content);
                
                if (jsonObject != null)
                {
                    var code = jsonObject["code"]?.ToObject<int>() ?? 0;
                    
                    if (code == 200)
                    {
                        ManagerList.Clear();
                        
                        var rows = jsonObject["rows"] as Newtonsoft.Json.Linq.JArray;
                        if (rows != null)
                        {
                            foreach (var item in rows)
                            {
                                var manager = new AgentUser
                                {
                                    UserId = item["userId"]?.ToObject<int>() ?? 0,
                                    AgentName = item["userName"]?.ToString() ?? string.Empty,
                                    NickName = item["nickName"]?.ToString() ?? string.Empty
                                };
                                ManagerList.Add(manager);
                            }
                            
                            Logger.Success($"部门 {deptId} 的业务经理列表加载成功，共 {ManagerList.Count} 条");
                            System.Diagnostics.Debug.WriteLine($"DeptManagerAgentSelector: ManagerList 已更新，当前有 {ManagerList.Count} 个业务经理");
                        }
                        else
                        {
                            Logger.Warning($"未找到 rows 数组");
                        }
                    }
                    else
                    {
                        var msg = jsonObject["msg"]?.ToString() ?? "未知错误";
                        Logger.Warning($"加载业务经理列表失败: {msg}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载业务经理列表失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 当选中战区变化时
        /// </summary>
        partial void OnSelectedWarZoneChanged(DeptInfo? value)
        {
            // 不自动加载，等待用户完成选择
        }

        /// <summary>
        /// 当选中渠道部变化时
        /// </summary>
        partial void OnSelectedChannelDeptChanged(DeptInfo? value)
        {
            // 不自动加载，等待用户完成选择
        }

        /// <summary>
        /// 当选中大区变化时
        /// </summary>
        partial void OnSelectedRegionManagerChanged(DeptInfo? value)
        {
            // 不自动加载，等待用户完成选择
        }

        /// <summary>
        /// 当选中部门变化时，加载对应的业务经理列表
        /// </summary>
        partial void OnSelectedDepartmentChanged(DeptInfo? value)
        {
            if (value != null && !_currentUser?.IsAgentRole == true)
            {
                // 非代理商角色：根据选中的部门 ID 加载业务经理列表
                _ = LoadManagerListByDeptIdAsync(value.Id);
                
                // 清空代理商列表
                AgentList.Clear();
                SelectedAgent = null;
            }
        }

        /// <summary>
        /// 当选中业务经理变化时，加载代理商列表
        /// </summary>
        partial void OnSelectedManagerChanged(AgentUser? value)
        {
            System.Diagnostics.Debug.WriteLine($"DeptManagerAgentSelector: OnSelectedManagerChanged called, value={value?.NickName ?? "null"}, IsAgentRole={_currentUser?.IsAgentRole}");
            
            if (value != null && _currentUser?.IsAgentRole != true)
            {
                System.Diagnostics.Debug.WriteLine($"DeptManagerAgentSelector: 选择业务经理 {value.NickName} (ID: {value.UserId})，开始加载代理商列表...");
                
                // 非代理商角色：根据选中的业务经理 ID 加载代理商列表
                _ = LoadAgentListAsync(value.UserId);
                
                // 通知外部业务经理已变化（即使没有选择代理商）
                OnAgentSelectedCallback?.Invoke(null, value, SelectedRegionManager, SelectedChannelDept, SelectedWarZone);
            }
            else if (value == null)
            {
                System.Diagnostics.Debug.WriteLine("DeptManagerAgentSelector: 清空业务经理选择");
                // 清空代理商列表
                AgentList.Clear();
                SelectedAgent = null;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DeptManagerAgentSelector: 代理商角色，不加载代理商列表");
            }
        }

        /// <summary>
        /// 当选中代理商变化时
        /// </summary>
        partial void OnSelectedAgentChanged(AgentItem? value)
        {
            OnAgentSelectedCallback?.Invoke(value, SelectedManager, SelectedRegionManager, SelectedChannelDept, SelectedWarZone);
        }

        /// <summary>
        /// 加载代理商列表
        /// </summary>
        /// <param name="userId">用户 ID（业务经理 ID 或当前用户 ID）</param>
        private async Task LoadAgentListAsync(long userId)
        {
            try
            {
                Logger.Info($"开始加载代理商列表 - userId: {userId}");
                
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/default/getMyAgentListWithCount?userId={userId}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    AgentList.Clear();
                    
                    var data = response.Data as Newtonsoft.Json.Linq.JArray;
                    if (data != null)
                    {
                        foreach (var item in data)
                        {
                            var agent = new AgentItem
                            {
                                AgentId = item["agentId"]?.ToObject<long>() ?? 0,
                                AgentName = item["agentName"]?.ToString() ?? string.Empty,
                                Count = item["count"]?.ToObject<int>() ?? 0
                            };
                            AgentList.Add(agent);
                        }
                    }
                    
                    Logger.Success($"代理商列表加载成功，共 {AgentList.Count} 条");
                }
                else
                {
                    Logger.Warning($"加载代理商列表失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载代理商列表异常: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// 代理商项
    /// </summary>
    public class AgentItem
    {
        public long AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public int Count { get; set; }
        
        public override string ToString() => $"{AgentName} ({Count})";
    }
}
