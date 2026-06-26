using System;

namespace AgentManagement.Avalonia.Utils
{
    /// <summary>
    /// 用户信息类，用于存储当前登录用户的信息
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 组织ID
        /// </summary>
        public int OrgId { get; set; }

        /// <summary>
        /// 用户级别 (1=渠道部级别, 2=大区级别)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; } = string.Empty;

        /// <summary>
        /// CRM Token（用于访问CRM系统）
        /// </summary>
        public string CrmToken { get; set; } = string.Empty;

        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLoggedIn => !string.IsNullOrEmpty(Username);

        /// <summary>
        /// 单例实例
        /// </summary>
        public static UserInfo Instance { get; } = new UserInfo();
        
        private UserInfo()
        {
        }

        /// <summary>
        /// 设置用户信息
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="orgId">组织ID</param>
        /// <param name="level">用户级别</param>
        /// <param name="realName">真实姓名</param>
        public void SetUserInfo(string username, int orgId, int level, string realName)
        {
            Username = username;
            OrgId = orgId;
            Level = level;
            RealName = realName;
        }

        /// <summary>
        /// 设置CRM Token
        /// </summary>
        /// <param name="token">CRM Token</param>
        public void SetCrmToken(string token)
        {
            CrmToken = token ?? string.Empty;
        }

        /// <summary>
        /// 获取CRM Token
        /// </summary>
        /// <returns>CRM Token</returns>
        public string GetCrmToken()
        {
            return CrmToken;
        }

        /// <summary>
        /// 清除用户信息（登出时使用）
        /// </summary>
        public void Clear()
        {
            Username = string.Empty;
            OrgId = 0;
            Level = 0;
            RealName = string.Empty;
            CrmToken = string.Empty;
        }
    }
}
