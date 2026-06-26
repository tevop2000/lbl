using System;
using Newtonsoft.Json;

namespace AgentManagement.Avalonia.Models
{
    public class DetailCalculateYearDto
    {
        [JsonProperty("agentId")]
        public long? AgentId { get; set; }
        
        [JsonProperty("yearMonth")]
        public string? YearMonth { get; set; }
        
        [JsonProperty("rateMonthlyDetailYear")]
        public RateMonthlyDetailYear? RateMonthlyDetailYear { get; set; }
    }
}
