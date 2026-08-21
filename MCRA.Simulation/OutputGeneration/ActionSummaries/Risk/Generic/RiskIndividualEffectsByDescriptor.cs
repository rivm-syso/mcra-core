using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public class RiskIndividualEffectsByDescriptor<T> where T : IExposureContributorKey {
        public double SamplingWeight { get; set; }
        public int SimulatedIndividualId { get; set; }
        public double TotalEndPointExposure { get; set; }
        public double TotalExposure { get; set; }
        public IStratificationLevel StratificationLevel { get; set; }
        public T Descriptor { get; set; }
    }
}
