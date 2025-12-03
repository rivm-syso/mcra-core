using MCRA.General;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class ExposureResponseResult {
        public TargetUnit TargetUnit { get; set; }
        public double ErfDoseUnitAlignmentFactor { get; set; }
        public IExposureResponseModel ExposureResponseFunctionModel { get; set; }
        public List<ExposureResponseResultRecord> ExposureResponseResultRecords { get; set; }
        public EffectMetric EffectMetric => ExposureResponseFunctionModel.EffectMetric;
    }
}
