using MCRA.Simulation.Calculators.Stratification;

namespace MCRA.Simulation.OutputGeneration {
    public class IndividualEffectsByStratificationLevel {
        public double SamplingWeight { get; set; }
        public double HazardExposureRatio { get; set; }
        public double ExposureHazardRatio { get; set; }
        public bool IsPositive { get; set; }
        public IStratificationLevel StratificationLevel { get; set; }
    }
}
