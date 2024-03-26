using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes percentiles for specified percentages.
    /// </summary>
    public class ExposureHazardRatioPercentileSection : RiskPercentileSectionBase {

        public void Summarize(
            List<IndividualEffect> individualEffects,
            double[] percentages,
            IHazardCharacterisationModel referenceDose,
            TargetUnit targetUnit,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool hcSubgroupDependent,
            bool skipPrivacySensitiveOutputs
        ) {
            base.Summarize(
                individualEffects,
                percentages,
                referenceDose,
                targetUnit,
                RiskMetricType.ExposureHazardRatio,
                riskMetricCalculationType,
                isInverseDistribution,
                hcSubgroupDependent,
                skipPrivacySensitiveOutputs
            );
        }
    }
}
