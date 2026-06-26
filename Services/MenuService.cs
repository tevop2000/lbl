using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.Services
{
    /// <summary>
    /// 菜单服务 - 处理动态菜单加载
    /// 对应 datafication-ui 中的 src/api/menu.js
    /// </summary>
    public class MenuService
    {
        /// <summary>
        /// 获取用户路由菜单（动态加载左侧菜单）
        /// 对应接口: GET /getRouters
        /// </summary>
        /// <returns>菜单路由列表</returns>
        public static async Task<MenuResult> GetRoutersAsync()
        {
            try
            {
                Logger.Separator("获取菜单路由");
                Logger.Info($"调用接口: GET /getRouters");

                var response = await NewApiClient.GetAsync<List<MenuRoute>>("/getRouters");

                if (response.Code == 200 && response.Data != null)
                {
                    Logger.Success($"获取菜单成功，共 {response.Data.Count} 个一级菜单");
                    
                    // 打印完整的接口返回数据（JSON格式）
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(response.Data, Newtonsoft.Json.Formatting.Indented);
                    System.Diagnostics.Debug.WriteLine("===== 接口返回的菜单数据 =====");
                    System.Diagnostics.Debug.WriteLine(json);
                    System.Diagnostics.Debug.WriteLine("=============================");
                    
                    // 打印菜单结构
                    PrintMenuStructure(response.Data, 0);
                    
                    Logger.Separator("获取菜单完成");
                    
                    return new MenuResult
                    {
                        Success = true,
                        Routes = response.Data,
                        Message = "获取菜单成功"
                    };
                }
                else
                {
                    Logger.Error($"获取菜单失败: {response.Message}");
                    Logger.Separator("获取菜单结束");
                    return new MenuResult
                    {
                        Success = false,
                        Routes = new List<MenuRoute>(),
                        Message = response.Message ?? "获取菜单失败"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"获取菜单异常: {ex.Message}", ex);
                Logger.Separator("获取菜单结束");
                return new MenuResult
                {
                    Success = false,
                    Routes = new List<MenuRoute>(),
                    Message = $"获取菜单异常: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 打印菜单结构（用于调试）
        /// </summary>
        private static void PrintMenuStructure(List<MenuRoute> routes, int level)
        {
            if (routes == null || routes.Count == 0) return;

            foreach (var route in routes)
            {
                if (route.Hidden) continue;

                string indent = new string(' ', level * 2);
                string title = route.Meta?.Title ?? route.Name;
                
                Logger.Info($"{indent}{title} (路径: {route.Path})");

                if (route.Children != null && route.Children.Count > 0)
                {
                    PrintMenuStructure(route.Children, level + 1);
                }
            }
        }
    }

    #region 数据模型

    /// <summary>
    /// 菜单结果
    /// </summary>
    public class MenuResult
    {
        public bool Success { get; set; }
        public List<MenuRoute> Routes { get; set; } = new List<MenuRoute>();
        public string? Message { get; set; }
    }

    /// <summary>
    /// 菜单路由
    /// </summary>
    public class MenuRoute
    {
        /// <summary>
        /// 路由名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 路由路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// 重定向路径
        /// </summary>
        public string? Redirect { get; set; }

        /// <summary>
        /// 组件路径
        /// </summary>
        public string? Component { get; set; }

        /// <summary>
        /// 元信息
        /// </summary>
        public MenuMeta? Meta { get; set; }

        /// <summary>
        /// 子路由
        /// </summary>
        public List<MenuRoute>? Children { get; set; }

        /// <summary>
        /// 权限标识
        /// </summary>
        public string[]? Permissions { get; set; }

        /// <summary>
        /// 角色标识
        /// </summary>
        public string[]? Roles { get; set; }
    }

    /// <summary>
    /// 菜单元信息
    /// </summary>
    public class MenuMeta
    {
        /// <summary>
        /// 菜单标题
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 是否缓存
        /// </summary>
        public bool NoCache { get; set; }

        /// <summary>
        /// 链接地址
        /// </summary>
        public string? Link { get; set; }
    }

    #endregion
}
