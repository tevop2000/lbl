using System;
using Newtonsoft.Json;

namespace AgentManagement.Avalonia.Models
{
    public class RateMonthlyDetailYear
    {
        [JsonProperty("detailId")]
        public long? DetailId { get; set; }
        
        [JsonProperty("agentId")]
        public long? AgentId { get; set; }
        
        [JsonProperty("agentName")]
        public string? AgentName { get; set; }
        
        [JsonProperty("yearMonth")]
        public string? YearMonth { get; set; }
        
        [JsonProperty("targetSales")]
        public long? TargetSales { get; set; }
        
        [JsonProperty("achievementRate")]
        public decimal? AchievementRate { get; set; }
        
        [JsonProperty("breakSales")]
        public long? BreakSales { get; set; }
        
        [JsonProperty("year")]
        public string? Year { get; set; }
        
        [JsonProperty("totalCommission")]
        public decimal? TotalCommission { get; set; }
        
        [JsonProperty("improvedSales")]
        public long? ImprovedSales { get; set; }
        
        [JsonProperty("salesGrowthProfit")]
        public decimal? SalesGrowthProfit { get; set; }
        
        [JsonProperty("structureOptimizeProfit")]
        public decimal? StructureOptimizeProfit { get; set; }
        
        [JsonProperty("premiumProfit")]
        public decimal? PremiumProfit { get; set; }
        
        [JsonProperty("totalExtraProfit")]
        public decimal? TotalExtraProfit { get; set; }
        
        [JsonProperty("adjustedNetProfit")]
        public decimal? AdjustedNetProfit { get; set; }
        
        [JsonProperty("expenseRate")]
        public decimal? ExpenseRate { get; set; }
        
        [JsonProperty("netProfitRate")]
        public decimal? NetProfitRate { get; set; }
    }
}
