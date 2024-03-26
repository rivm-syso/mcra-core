using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes percentiles for specified percentages.
    /// </summary>
    public class HazardExposureRatioPercentileSection : RiskPercentileSectionBase {

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
                RiskMetricType.HazardExposureRatio,
                riskMetricCalculationType,
                isInverseDistribution,
                hcSubgroupDependent,
                skipPrivacySensitiveOutputs
            );
        }
    }
}
