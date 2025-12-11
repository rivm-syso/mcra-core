using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class EnvironmentalBurdenOfDiseaseResultRecord {
        public List<string> SourceIndicatorList { get; set; }
        public BodIndicator BodIndicator { get; set; }
        public Effect Effect { get; set; }
        public Population Population { get; set; }
        public IExposureResponseModel ExposureResponseModel { get; set; }
        public ExposureUnitTriple ErfDoseUnit { get; set; }
        public Compound Substance { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public List<EnvironmentalBurdenOfDiseaseResultBinRecord> EnvironmentalBurdenOfDiseaseResultBinRecords { get; set; }
        public double StandardisedPopulationSize { get; set; }
    }
}
