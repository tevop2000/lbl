using System;
using Newtonsoft.Json;

namespace AgentManagement.Avalonia.Models
{
    public class CvpCalculateDto
    {
        [JsonProperty("agentId")]
        public long? AgentId { get; set; }
        
        [JsonProperty("yearMonth")]
        public string? YearMonth { get; set; }
        
        [JsonProperty("rateMonthlyDetail")]
        public RateMonthlyDetail? RateMonthlyDetail { get; set; }
        
        [JsonProperty("rateMonthlyEndDetail")]
        public RateMonthlyEndDetail? RateMonthlyEndDetail { get; set; }
    }
}
