using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleRiskRatioSection : SingleRiskCharacterisationRatioSectionBase {
        public SingleRiskRatioSection() {
        }
        public SingleRiskRatioSection(RiskMetricType riskMetricType) {
            RiskMetricType = riskMetricType;
        }
    }
}
