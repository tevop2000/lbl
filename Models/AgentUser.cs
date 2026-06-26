using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace AgentManagement.Avalonia.Models
{
    /// <summary>
    /// 分页响应数据
    /// </summary>
    public class PageResult<T>
    {
        [JsonProperty("total")]
        public int Total { get; set; }
        
        [JsonProperty("rows")]
        public List<T> Rows { get; set; } = new List<T>();
        
        [JsonProperty("code")]
        public int Code { get; set; }
        
        [JsonProperty("msg")]
        public string Msg { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// 代理商信息模型
    /// </summary>
    public class AgentUser : INotifyPropertyChanged
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("agentId")]
        public int AgentId { get; set; }

        [JsonProperty("agentName")]
        public string AgentName { get; set; } = string.Empty;

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("nickName")]
        public string NickName { get; set; } = string.Empty;

        [JsonProperty("createTime")]
        public string CreateTime { get; set; } = string.Empty;

        [JsonProperty("productDefaultCount")]
        public int ProductDefaultCount { get; set; }

        private bool _isSelected;
        /// <summary>
        /// 是否被选中（用于产品配置管理面板）
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        /// <summary>
        /// 显示名称（用于下拉菜单）
        /// </summary>
        public string DisplayName => string.IsNullOrEmpty(NickName) ? AgentName : $"{NickName} ({AgentName})";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
