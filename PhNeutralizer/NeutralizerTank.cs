
using Newtonsoft.Json;

namespace PhNeutralizer
{
    public class NeutralizerTank
    {
        public double Ph { get; set; }
        [JsonIgnore]
        public double FlowRate { get; set; }
        public double Volume { get; set; }
        public double CurrentLevel { get; set; }
        public double MaxFlowRate { get; set; }
        public bool AutoRefill { get; set; }
        public double AutoRefillLevel { get; set; }
    }
}
