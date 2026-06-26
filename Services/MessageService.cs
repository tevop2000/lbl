using System;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.Services
{
    /// <summary>
    /// 消息服务 - 处理消息相关的接口
    /// </summary>
    public class MessageService
    {
        /// <summary>
        /// 获取消息统计数据
        /// 对应接口: GET /system/message/getMessageStats
        /// </summary>
        /// <returns>消息统计结果</returns>
        public static async Task<MessageStatsResult> GetMessageStatsAsync()
        {
            try
            {
                Logger.Debug("获取消息统计数据");

                var response = await NewApiClient.GetAsync<MessageStatsData>("/system/message/getMessageStats");

                if (response.Code == 200)
                {
                    Logger.Debug($"获取消息统计成功，未读消息: {response.Data.UnreadCount}");

                    return new MessageStatsResult
                    {
                        Success = true,
                        Data = response.Data,
                        Message = "获取消息统计成功"
                    };
                }
                else
                {
                    Logger.Warning($"获取消息统计失败: {response.Message}");
                    return new MessageStatsResult
                    {
                        Success = false,
                        Data = null,
                        Message = response.Message ?? "获取消息统计失败"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"获取消息统计异常: {ex.Message}", ex);
                return new MessageStatsResult
                {
                    Success = false,
                    Data = null,
                    Message = $"获取消息统计异常: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 获取用户未读消息列表
        /// 对应接口: GET /system/message/getUserUnreadMessages
        /// </summary>
        /// <returns>未读消息列表结果</returns>
        public static async Task<UnreadMessagesResult> GetUnreadMessagesAsync()
        {
            try
            {
                Logger.Debug("获取用户未读消息列表");

                var response = await NewApiClient.GetAsync<MessageWithStatusVO[]>("/system/message/getUserUnreadMessages");

                if (response.Code == 200)
                {
                    Logger.Debug($"获取未读消息成功，共 {response.Data?.Length ?? 0} 条");

                    return new UnreadMessagesResult
                    {
                        Success = true,
                        Data = response.Data ?? new MessageWithStatusVO[0],
                        Message = "获取未读消息成功"
                    };
                }
                else
                {
                    Logger.Warning($"获取未读消息失败: {response.Message}");
                    return new UnreadMessagesResult
                    {
                        Success = false,
                        Data = new MessageWithStatusVO[0],
                        Message = response.Message ?? "获取未读消息失败"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"获取未读消息异常: {ex.Message}", ex);
                return new UnreadMessagesResult
                {
                    Success = false,
                    Data = new MessageWithStatusVO[0],
                    Message = $"获取未读消息异常: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 标记消息为已读
        /// 对应接口: POST /system/message/markAsRead
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <returns>标记结果</returns>
        public static async Task<MarkAsReadResult> MarkAsReadAsync(int messageId)
        {
            try
            {
                Logger.Debug($"标记消息为已读，messageId: {messageId}");

                var requestData = new
                {
                    messageId = messageId
                };

                var response = await NewApiClient.PostAsync<object>("/system/message/markAsRead", requestData);

                if (response.Code == 200)
                {
                    Logger.Debug($"标记消息已读成功，messageId: {messageId}");

                    return new MarkAsReadResult
                    {
                        Success = true,
                        Message = "标记已读成功"
                    };
                }
                else
                {
                    Logger.Warning($"标记消息已读失败: {response.Message}");
                    return new MarkAsReadResult
                    {
                        Success = false,
                        Message = response.Message ?? "标记已读失败"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"标记消息已读异常: {ex.Message}", ex);
                return new MarkAsReadResult
                {
                    Success = false,
                    Message = $"标记已读异常: {ex.Message}"
                };
            }
        }
    }

    /// <summary>
    /// 标记已读结果
    /// </summary>
    public class MarkAsReadResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    #region 数据模型

    /// <summary>
    /// 消息统计结果
    /// </summary>
    public class MessageStatsResult
    {
        public bool Success { get; set; }
        public MessageStatsData? Data { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// 消息统计数据
    /// </summary>
    public class MessageStatsData
    {
        /// <summary>
        /// 未读消息数量
        /// </summary>
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// 未读消息列表结果
    /// </summary>
    public class UnreadMessagesResult
    {
        public bool Success { get; set; }
        public MessageWithStatusVO[] Data { get; set; } = new MessageWithStatusVO[0];
        public string? Message { get; set; }
    }

    /// <summary>
    /// 消息实体（带状态）
    /// </summary>
    public class MessageWithStatusVO
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 消息标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 发送人名称
        /// </summary>
        public string SenderName { get; set; } = string.Empty;
    }

    #endregion
}
