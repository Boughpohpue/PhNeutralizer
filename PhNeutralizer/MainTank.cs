
namespace PhNeutralizer
{
    public class MainTank
    {
        public double Ph { get; set; }     
        public double TargetPh { get; set; }
        public double Volume { get; set; }
        public double CurrentLevel { get; set; }
        public double OutputPipeAutoOpenTankLevel { get; set; }

        public InputPipe InputPipe { get; set; }
        public OutputPipe OutputPipe { get; set; }
        public NeutralizerTank AcidTank { get; set; }
        public NeutralizerTank BaseTank { get; set; }
    }
}
