
namespace PhNeutralizer
{
    public class TankFactory
    {
        public MainTank Create()
        {
            return new MainTank()
            {
                Ph = 7.4,
                Volume = 10000,
                CurrentLevel = 2550,
                OutputPipeAutoOpenTankLevel = 2500,
                InputPipe = new InputPipe()
                {
                    Ph = 4.5,
                    FlowRate = 4.0,
                    MaxFlowRate = 10.0
                },
                OutputPipe = new OutputPipe()
                {
                    ValvePosition = OutputPipeValvePosition.AlwaysOpen,
                    FlowRate = 3.0,
                    MaxFlowRate = 10.0
                },
                AcidTank = new NeutralizerTank()
                {
                    Ph = 1,
                    Volume = 500,
                    CurrentLevel = 350,
                    MaxFlowRate = 5.0,
                    AutoRefill = true,
                    AutoRefillLevel = 50
                },
                BaseTank = new NeutralizerTank()
                {
                    Ph = 14.0,
                    Volume = 500,
                    CurrentLevel = 350,
                    MaxFlowRate = 5.0,
                    AutoRefill = true,
                    AutoRefillLevel = 50
                }
            };
        }
    }
}
