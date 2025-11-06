using MCRA.General;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class RiskMaximumCumulativeRatioSection : SummarySection {

        public List<DriverSubstanceRecord> DriverSubstanceTargets { get; set; }
        public bool RiskBased { get; set; }
        public double Threshold { get; set; }
        public RiskMetricType RiskMetricType { get; set; }
        public bool SkipPrivacySensitiveOutputs { get; set; }

        public void Summarize(
            List<DriverSubstance> driverSubstances,
            ExposureApproachType exposureApproachType,
            bool skipPrivacySensitiveOutputs,
            double? threshold,
            RiskMetricType riskMetricType = RiskMetricType.ExposureHazardRatio
        ) {
            SkipPrivacySensitiveOutputs = skipPrivacySensitiveOutputs;
            if (exposureApproachType == ExposureApproachType.RiskBased) {
                RiskBased = true;
            }
            Threshold = threshold ?? 1;
            RiskMetricType = riskMetricType;
            DriverSubstanceTargets = [];
            foreach (var item in driverSubstances) {
                DriverSubstanceTargets.Add(new DriverSubstanceRecord() {
                    SubstanceCode = item.Substance.Code,
                    SubstanceName = item.Substance.Name,
                    Ratio = item.MaximumCumulativeRatio,
                    CumulativeExposure = item.CumulativeExposure,
                    Target = item.Target?.Code ?? string.Empty,
                });
            }
        }
    }
}
