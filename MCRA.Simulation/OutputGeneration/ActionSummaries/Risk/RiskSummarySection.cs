using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {

    public class RiskSummarySection : ActionSummarySectionBase {
        public TargetLevelType TargetDoseLevel { get; set; }
        public bool IsHazardCharacterisationDistribution { get; set; }
        public ActionType ExposureModel { get; set; }
        public ExposureType ExposureType { get; set; }
        public RiskMetricType RiskMetricType { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }
        public int NumberOfMissingSubstances { get; set; }
        public int NumberOfSubstances { get; set; }
    }
}
