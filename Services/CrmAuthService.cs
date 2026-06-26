using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.Services
{
    /// <summary>
    /// CRM认证服务 - 处理CRM系统的登录和Token管理
    /// </summary>
    public class CrmAuthService
    {
        // CRM配置
        private const string CRM_LOGIN_URL = "https://backend.chilwee.com/sysLogin/login";
        
        /// <summary>
        /// CRM登录结果
        /// </summary>
        public class CrmLoginResult
        {
            public bool Success { get; set; }
            public string Token { get; set; } = string.Empty;
            public string RealName { get; set; } = string.Empty;
            public int UserId { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// 调用CRM登录接口
        /// </summary>
        /// <param name="username">CRM账号</param>
        /// <param name="password">CRM密码</param>
        /// <returns>CRM登录结果</returns>
        public static async Task<CrmLoginResult> LoginToCrmAsync(string username, string password)
        {
            try
            {
                Logger.Separator("开始CRM登录");
                Logger.Info($"CRM用户名: {username}");
                Logger.Info($"调用接口: POST {CRM_LOGIN_URL}");

                var loginData = new
                {
                    loginName = username,
                    password = password
                };

                // 使用HttpClient直接请求CRM接口
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var jsonContent = JsonConvert.SerializeObject(loginData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(CRM_LOGIN_URL, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    Logger.Info($"CRM登录响应: {responseString}");

                    // 解析响应
                    dynamic result = JsonConvert.DeserializeObject(responseString)!;

                    if (result.code.ToString() == "1")
                    {
                        // 登录成功
                        string token = result.data.token.ToString();
                        string realName = result.data.realName?.ToString() ?? username;
                        int userId = result.data.id != null ? (int)result.data.id : 0;

                        Logger.Success($"CRM登录成功！用户: {realName}, UserID: {userId}");
                        Logger.Success($"CRM Token: {token.Substring(0, Math.Min(30, token.Length))}...");
                        Logger.Separator("CRM登录完成");

                        return new CrmLoginResult
                        {
                            Success = true,
                            Token = token,
                            RealName = realName,
                            UserId = userId,
                            Message = "CRM登录成功"
                        };
                    }
                    else
                    {
                        // 登录失败
                        string errorMsg = result.msg?.ToString() ?? "CRM登录失败";
                        Logger.Error($"CRM登录失败: {errorMsg}");
                        Logger.Separator("CRM登录结束");

                        return new CrmLoginResult
                        {
                            Success = false,
                            Token = string.Empty,
                            RealName = string.Empty,
                            UserId = 0,
                            Message = errorMsg
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"CRM登录异常: {ex.Message}", ex);
                Logger.Separator("CRM登录结束");

                return new CrmLoginResult
                {
                    Success = false,
                    Token = string.Empty,
                    RealName = string.Empty,
                    UserId = 0,
                    Message = $"CRM登录异常: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 保存CRM Token到全局存储
        /// </summary>
        /// <param name="token">CRM Token</param>
        public static void SaveCrmToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                UserInfo.Instance.SetCrmToken(token);
                Logger.Info("CRM Token已保存到全局存储");
            }
        }

        /// <summary>
        /// 获取保存的CRM Token
        /// </summary>
        /// <returns>CRM Token</returns>
        public static string GetCrmToken()
        {
            return UserInfo.Instance.GetCrmToken();
        }
    }
}
