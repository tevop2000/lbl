using System;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Models;

namespace AgentManagement.Avalonia.Services
{
    /// <summary>
    /// 认证服务 - 处理用户登录、登出、获取用户信息等操作
    /// 对应 datafication-ui 中的 src/api/login.js
    /// </summary>
    public class AuthService
    {
        /// <summary>
        /// 用户登录成功事件
        /// </summary>
        public static event Action? UserLoggedIn;
        
        /// <summary>
        /// 触发用户登录成功事件
        /// </summary>
        public static void OnUserLoggedIn()
        {
            UserLoggedIn?.Invoke();
        }
        /// <summary>
        /// 用户登录
        /// 对应接口: POST /login
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="code">验证码（可选）</param>
        /// <param name="uuid">验证码UUID（可选）</param>
        /// <returns>登录结果，包含 Token</returns>
        public static async Task<LoginResult> LoginAsync(string username, string password, string? code = null, string? uuid = null)
        {
            try
            {
                Logger.Separator("开始登录");
                Logger.Info($"用户名: {username}");
                Logger.Info($"密码: {new string('*', password.Length)}");
                Logger.Info($"调用接口: POST /login");

                // 调用登录接口（不需要 Token）
                // 注意：后端返回格式是 {"code":200,"msg":"操作成功","token":"..."}
                // 不是标准的 ApiResponse 格式，所以需要特殊处理
                
                var loginData = new
                {
                    username = username,
                    password = password,
                    code = code ?? "",
                    uuid = uuid ?? ""
                };

                // 直接使用 HttpClient 调用，不通过 NewApiClient 的封装
                var jsonContent = new System.Net.Http.StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(loginData),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var httpClient = new System.Net.Http.HttpClient();
                var httpResponse = await httpClient.PostAsync($"{NewApiClient.BaseUrl}/login", jsonContent);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                Logger.Info($"原始响应: {responseContent}");
                
                // 直接反序列化为 LoginResponse
                var loginResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                
                if (loginResponse != null && loginResponse.Code == 200 && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    Logger.Success("登录成功！");
                    Logger.Success($"Token: {loginResponse.Token.Substring(0, Math.Min(30, loginResponse.Token.Length))}...");
                    
                    // 设置全局 Token
                    NewApiClient.SetAuthToken(loginResponse.Token);
                    
                    Logger.Separator("登录完成");
                    
                    return new LoginResult
                    {
                        Success = true,
                        Token = loginResponse.Token,
                        Message = "登录成功"
                    };
                }
                else
                {
                    string errorMsg = loginResponse?.Msg ?? "登录失败";
                    Logger.Error($"登录失败: {errorMsg}");
                    Logger.Separator("登录结束");
                    return new LoginResult
                    {
                        Success = false,
                        Token = null,
                        Message = errorMsg
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"登录异常: {ex.Message}", ex);
                Logger.Separator("登录结束");
                return new LoginResult
                {
                    Success = false,
                    Token = null,
                    Message = $"登录异常: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 获取用户详细信息
        /// 对应接口: GET /getInfo
        /// </summary>
        /// <returns>用户信息，包含角色、权限等</returns>
        public static async Task<UserInfoResult> GetUserInfoAsync()
        {
            try
            {
                Logger.Separator("获取用户信息");
                Logger.Info($"调用接口: GET /getInfo");

                // 直接使用 HttpClient 调用，因为后端返回格式不是标准的 ApiResponse
                var httpClient = new System.Net.Http.HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", NewApiClient.GetAuthToken());
                
                var httpResponse = await httpClient.GetAsync($"{NewApiClient.BaseUrl}/getInfo");
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                
                Logger.Info($"原始响应: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
                
                // 打印完整的响应内容到控制台
                Console.WriteLine("\n========== /getInfo 接口完整返回数据 ==========");
                Console.WriteLine(responseContent);
                Console.WriteLine("==============================================\n");
                
                // 直接反序列化为 GetInfoResponse
                var getInfoResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.GetInfoResponse>(responseContent);

                if (getInfoResponse != null && getInfoResponse.Code == 200 && getInfoResponse.User != null)
                {
                    var user = getInfoResponse.User;
                    Logger.Success("获取用户信息成功");
                    Logger.Info($"用户ID: {user.UserId}");
                    Logger.Info($"用户名: {user.UserName}");
                    Logger.Info($"昵称: {user.NickName}");
                    Logger.Info($"钉钉ID: {user.DdUserId ?? "未设置"}");
                    Logger.Info($"角色数: {getInfoResponse.Roles?.Length ?? 0}");
                    Logger.Info($"权限数: {getInfoResponse.Permissions?.Length ?? 0}");
                    Logger.Separator("获取用户信息完成");

                    return new UserInfoResult
                    {
                        Success = true,
                        UserId = user.UserId,
                        DdUserId = user.DdUserId ?? "",
                        UserName = user.UserName,
                        NickName = user.NickName,
                        Avatar = user.Avatar ?? "",
                        Dept = user.Dept,
                        Roles = getInfoResponse.Roles ?? new string[0],
                        Permissions = getInfoResponse.Permissions ?? new string[0],
                        IsDefaultModifyPwd = getInfoResponse.IsDefaultModifyPwd,
                        IsPasswordExpired = getInfoResponse.IsPasswordExpired,
                        Message = "获取用户信息成功"
                    };
                }
                else
                {
                    string errorMsg = getInfoResponse?.Msg ?? "获取用户信息失败";
                    Logger.Error($"获取用户信息失败: {errorMsg}");
                    Logger.Separator("获取用户信息结束");
                    return new UserInfoResult
                    {
                        Success = false,
                        Message = errorMsg
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"获取用户信息异常: {ex.Message}", ex);
                Logger.Separator("获取用户信息结束");
                return new UserInfoResult
                {
                    Success = false,
                    Message = $"获取用户信息异常: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 用户登出
        /// 对应接口: POST /logout
        /// </summary>
        /// <returns>登出结果</returns>
        public static async Task<LogoutResult> LogoutAsync()
        {
            try
            {
                Logger.Separator("开始登出");
                Logger.Info($"调用接口: POST /logout");

                var response = await NewApiClient.PostAsync<object>("/logout", null!);

                // 清除本地 Token
                NewApiClient.ClearAuthToken();

                if (response.Code == 200)
                {
                    Logger.Success("登出成功");
                    Logger.Separator("登出完成");
                    return new LogoutResult
                    {
                        Success = true,
                        Message = "登出成功"
                    };
                }
                else
                {
                    // 即使后端返回错误，也清除本地 Token
                    Logger.Warning($"登出接口返回错误，但已清除本地 Token: {response.Message}");
                    Logger.Separator("登出完成");
                    return new LogoutResult
                    {
                        Success = true, // 前端登出成功
                        Message = "已退出登录"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"登出异常，但已清除本地 Token: {ex.Message}");
                Logger.Error($"堆栈: {ex.StackTrace}");
                // 即使异常，也清除本地 Token
                NewApiClient.ClearAuthToken();
                Logger.Separator("登出完成");
                return new LogoutResult
                {
                    Success = true, // 前端登出成功
                    Message = "已退出登录"
                };
            }
        }
    }

    #region 数据模型

    /// <summary>
    /// 登录响应（直接包含 token，不在 data 中）
    /// </summary>
    public class LoginResponse
    {
        public int Code { get; set; }
        public string? Msg { get; set; }
        public string? Token { get; set; }
    }

    /// <summary>
    /// 登录结果
    /// </summary>
    public class LoginResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// 登出结果
    /// </summary>
    public class LogoutResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    #endregion
}
