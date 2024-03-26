using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleExposureHazardRatioSection : SingleRiskCharacterisationRatioSectionBase {
        public override RiskMetricType RiskMetricType => RiskMetricType.ExposureHazardRatio;
    }
}
