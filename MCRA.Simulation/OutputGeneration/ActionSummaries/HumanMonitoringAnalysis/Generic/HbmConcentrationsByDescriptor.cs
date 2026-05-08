using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.Stratification;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmConcentrationsByDescriptor<T> where T : IHbmExposureContributorKey {
        public double SamplingWeight { get; set; }
        public double TotalEndpointExposure { get; set; }
        public List<HumanMonitoringSamplingMethod> SourceSamplingMethods { get; set; }
        public IStratificationLevel StratificationLevel { get; set; }
        public T Descriptor { get; set; }
    }
}
