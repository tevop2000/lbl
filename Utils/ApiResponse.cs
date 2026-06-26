using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace AgentManagement.Avalonia.Utils
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}
