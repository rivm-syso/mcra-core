using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleHazardExposureRatioSection : SingleRiskCharacterisationRatioSectionBase {
        public override RiskMetricType RiskMetricType => RiskMetricType.HazardExposureRatio;
    }
}
