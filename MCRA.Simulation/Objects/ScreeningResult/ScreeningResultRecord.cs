using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Objects {
    public sealed class ScreeningResultRecord {
        public Food FoodAsMeasured { get; set; }
        public Food FoodAsEaten { get; set; }
        public Compound Compound { get; set; }
        public double AggregateProportion { get; set; }
        public ConsumptionDistributionParameters ConsumptionParameters { get; set; }
        public ConcentrationDistributionParameters ConcentrationParameters { get; set; }
        public ScreeningDistributionParameters CensoredValueParameters { get; set; }
        public ScreeningDistributionParameters DetectParameters { get; set; }
        public double WeightCensoredValue {get; set; }
        public double WeightDetect { get; set; }
        public double Exposure { get; set;}
        public double Cup { get; set; }
        public double CupPercentage { get; set;}
        public double CumCupFraction {get; set;}
    }
}
