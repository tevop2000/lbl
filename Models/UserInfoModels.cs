using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AgentManagement.Avalonia.Models
{
    /// <summary>
    /// 用户信息结果
    /// </summary>
    public class UserInfoResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("ddUserId")]
        public string DdUserId { get; set; } = string.Empty;

        [JsonProperty("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonProperty("nickName")]
        public string NickName { get; set; } = string.Empty;

        [JsonProperty("avatar")]
        public string Avatar { get; set; } = string.Empty;

        [JsonProperty("roles")]
        public string[] Roles { get; set; } = new string[0];

        [JsonProperty("permissions")]
        public string[] Permissions { get; set; } = new string[0];

        [JsonProperty("isDefaultModifyPwd")]
        public bool IsDefaultModifyPwd { get; set; }

        [JsonProperty("isPasswordExpired")]
        public bool IsPasswordExpired { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("dept")]
        public DeptInfo? Dept { get; set; }

        /// <summary>
        /// 是否为业务经理角色
        /// </summary>
        public bool IsAgentRole => Roles != null && 
            !Array.Exists(Roles, r => r.Equals("lbladmin", System.StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// /getInfo 接口响应
    /// </summary>
    public class GetInfoResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; } = string.Empty;

        [JsonProperty("user")]
        public UserInfo? User { get; set; }

        [JsonProperty("roles")]
        public string[]? Roles { get; set; }

        [JsonProperty("permissions")]
        public string[]? Permissions { get; set; }

        [JsonProperty("isDefaultModifyPwd")]
        public bool IsDefaultModifyPwd { get; set; }

        [JsonProperty("isPasswordExpired")]
        public bool IsPasswordExpired { get; set; }
    }

    /// <summary>
    /// 用户详细信息
    /// </summary>
    public class UserInfo
    {
        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("deptId")]
        public long DeptId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonProperty("nickName")]
        public string NickName { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("phonenumber")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("sex")]
        public string Sex { get; set; } = "0";

        [JsonProperty("avatar")]
        public string? Avatar { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "0";

        [JsonProperty("delFlag")]
        public string DelFlag { get; set; } = "0";

        [JsonProperty("loginIp")]
        public string? LoginIp { get; set; }

        [JsonProperty("loginDate")]
        public string? LoginDate { get; set; }

        [JsonProperty("pwdUpdateDate")]
        public string? PwdUpdateDate { get; set; }

        [JsonProperty("dept")]
        public DeptInfo? Dept { get; set; }

        [JsonProperty("roles")]
        public List<RoleInfo>? Roles { get; set; }

        [JsonProperty("dduserId")]
        public string? DdUserId { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }
    }

    /// <summary>
    /// 角色信息
    /// </summary>
    public class RoleInfo
    {
        [JsonProperty("roleId")]
        public long RoleId { get; set; }

        [JsonProperty("roleName")]
        public string RoleName { get; set; } = string.Empty;

        [JsonProperty("roleKey")]
        public string RoleKey { get; set; } = string.Empty;

        [JsonProperty("roleSort")]
        public int RoleSort { get; set; }

        [JsonProperty("dataScope")]
        public string DataScope { get; set; } = "2";

        [JsonProperty("status")]
        public string Status { get; set; } = "0";

        [JsonProperty("permissions")]
        public List<string>? Permissions { get; set; }

        [JsonProperty("admin")]
        public bool Admin { get; set; }
    }
}
