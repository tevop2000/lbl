using Newtonsoft.Json;

namespace AgentManagement.Avalonia.Models
{
    public class RateQuarterlyStatsVO
    {
        [JsonProperty("quarterTargetSales")]
        public long? QuarterTargetSales { get; set; }

        [JsonProperty("quarterBreakSales")]
        public long? QuarterBreakSales { get; set; }

        [JsonProperty("quarterTargetNetProfit")]
        public decimal? QuarterTargetNetProfit { get; set; }

        [JsonProperty("quarterActualNetProfit")]
        public decimal? QuarterActualNetProfit { get; set; }
    }
}