using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace AgentManagement.Avalonia.Models
{
    /// <summary>
    /// 部门信息模型
    /// </summary>
    public class DeptInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("parentId")]
        public long ParentId { get; set; }

        [JsonProperty("ancestors")]
        public string Ancestors { get; set; } = string.Empty;

        [JsonProperty("deptName")]
        public string DeptName { get; set; } = string.Empty;

        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("orderNum")]
        public int OrderNum { get; set; }

        [JsonProperty("leader")]
        public string Leader { get; set; } = string.Empty;

        [JsonProperty("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = "0";

        [JsonProperty("delFlag")]
        public string DelFlag { get; set; } = "0";

        [JsonProperty("children")]
        public ObservableCollection<DeptInfo> Children { get; set; } = new ObservableCollection<DeptInfo>();

        /// <summary>
        /// 显示名称（用于下拉菜单）- 优先使用 label，如果没有则使用 deptName
        /// </summary>
        public string DisplayName => !string.IsNullOrEmpty(Label) ? Label : DeptName;

        /// <summary>
        /// 是否为根节点
        /// </summary>
        public bool IsRoot => ParentId == 0;
    }
}
