using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AgentManagement.Avalonia.Utils
{
    public static class NewApiClient
    {
        // 基础 URL - 可以根据实际情况修改
        public static string BaseUrl { get; set; } = "http://battery.sinkeriot.com:7001";
        //public static string BaseUrl { get; set; } = "http://localhost:8089";
      
        // 认证 Token
        private static string? _authToken;

        // HttpClient 实例
        private static readonly HttpClient _httpClient = new HttpClient();

        static NewApiClient()
        {
            // 设置默认超时时间
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// 设置认证 Token
        /// </summary>
        public static void SetAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// 获取认证 Token
        /// </summary>
        public static string? GetAuthToken()
        {
            return _authToken;
        }

        /// <summary>
        /// 清除认证 Token
        /// </summary>
        public static void ClearAuthToken()
        {
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// 获取 HttpClient 实例
        /// </summary>
        public static HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        /// <summary>
        /// 发送 GET 请求
        /// </summary>
        public static async Task<ApiResponse<T>> GetAsync<T>(string url)
        {
            try
            {
                var fullUrl = $"{BaseUrl}{url}";
                Logger.Info($"GET {fullUrl}");

                var response = await _httpClient.GetAsync(fullUrl);
                var content = await response.Content.ReadAsStringAsync();

                // 只打印摘要信息，避免打印大量数据
                if (url.Contains("/system/user/deptTree"))
                {
                    Logger.Debug($"响应: Code=200, Data=[部门树数据，已省略详细内容]");
                }
                else
                {
                    Logger.Debug($"响应: {content}");
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<T>>(content);
                return result ?? new ApiResponse<T> { Code = 500, Message = "解析响应失败" };
            }
            catch (Exception ex)
            {
                Logger.Error($"GET 请求失败: {ex.Message}", ex);
                return new ApiResponse<T> { Code = 500, Message = ex.Message };
            }
        }

        /// <summary>
        /// 发送 POST 请求
        /// </summary>
        public static async Task<ApiResponse<T>> PostAsync<T>(string url, object? data)
        {
            try
            {
                var fullUrl = $"{BaseUrl}{url}";
                Logger.Info($"POST {fullUrl}");

                string jsonContent = "{}";
                if (data != null)
                {
                    jsonContent = JsonConvert.SerializeObject(data);
                }

                Logger.Debug($"请求数据: {jsonContent}");

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Logger.Debug($"响应: {responseContent}");

                var result = JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
                return result ?? new ApiResponse<T> { Code = 500, Message = "解析响应失败" };
            }
            catch (Exception ex)
            {
                Logger.Error($"POST 请求失败: {ex.Message}", ex);
                return new ApiResponse<T> { Code = 500, Message = ex.Message };
            }
        }

        /// <summary>
        /// 发送 POST 请求并返回字节数组（用于下载文件）
        /// </summary>
        public static async Task<byte[]?> PostAsyncBytes(string url, object? data)
        {
            try
            {
                var fullUrl = $"{BaseUrl}{url}";
                Logger.Info($"POST (bytes) {fullUrl}");

                string jsonContent = "{}";
                if (data != null)
                {
                    jsonContent = JsonConvert.SerializeObject(data);
                }

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(fullUrl, content);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"POST (bytes) 请求失败: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public static async Task<byte[]?> DownloadFileAsync(string url, string method = "GET")
        {
            try
            {
                var fullUrl = $"{BaseUrl}{url}";
                Logger.Info($"{method} (download) {fullUrl}");

                HttpResponseMessage response;
                if (method.ToUpper() == "POST")
                {
                    var content = new StringContent("{}", Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync(fullUrl, content);
                }
                else
                {
                    response = await _httpClient.GetAsync(fullUrl);
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"下载文件失败: {ex.Message}", ex);
                return null;
            }
        }
    }
}
