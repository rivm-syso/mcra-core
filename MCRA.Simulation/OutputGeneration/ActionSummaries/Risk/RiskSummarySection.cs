using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {

    public class RiskSummarySection : ActionSummaryBase {
        public TargetLevelType TargetDoseLevel { get; set; }
        public bool IsHazardCharacterisationDistribution { get; set; }
        public ActionType ExposureModel { get; set; }
        public ExposureType ExposureType { get; set; }
        public RiskMetricType RiskMetricType { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }

        public void Summarize(
            ExposureType exposureType,
            TargetLevelType targetDoseLevelType,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            ActionType exposureModel,
            bool isHazardCharacterisationDistribution
        ) {
            IsHazardCharacterisationDistribution = isHazardCharacterisationDistribution;
            ExposureModel = exposureModel;
            ExposureType = exposureType;
            TargetDoseLevel = targetDoseLevelType;
            RiskMetricType = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
        }
    }
}
