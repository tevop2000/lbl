using Newtonsoft.Json;

namespace AgentManagement.Avalonia.Models
{
    public class RateMonthlyDetailQuarterly
    {
        [JsonProperty("detailId")]
        public long? DetailId { get; set; }

        [JsonProperty("agentId")]
        public long? AgentId { get; set; }

        [JsonProperty("agentName")]
        public string? AgentName { get; set; }

        [JsonProperty("yearMonth")]
        public string? YearMonth { get; set; }

        [JsonProperty("quarterly")]
        public string? Quarterly { get; set; }

        [JsonProperty("targetSales")]
        public long? TargetSales { get; set; }

        [JsonProperty("actualSales")]
        public long? ActualSales { get; set; }

        [JsonProperty("salesAmount")]
        public decimal? SalesAmount { get; set; }

        [JsonProperty("totalCost")]
        public decimal? TotalCost { get; set; }

        [JsonProperty("achievementRate")]
        public decimal? AchievementRate { get; set; }

        [JsonProperty("breakSales")]
        public long? BreakSales { get; set; }

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

        [JsonProperty("isFinish")]
        public int? IsFinish { get; set; }
    }
}