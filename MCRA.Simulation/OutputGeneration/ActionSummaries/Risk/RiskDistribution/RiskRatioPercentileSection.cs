using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes percentiles for specified percentages.
    /// </summary>
    public class RiskRatioPercentileSection : RiskPercentileSectionBase {

        public void Summarize(
            List<IndividualEffect> individualEffects,
            double[] percentages,
            IHazardCharacterisationModel referenceDose,
            TargetUnit targetUnit,
            RiskMetricCalculationType riskMetricCalculationType,
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            bool hcSubgroupDependent,
            bool hasHCSubgroups,
            bool skipPrivacySensitiveOutputs
        ) {
            Summarize(
                individualEffects,
                percentages,
                referenceDose,
                targetUnit,
                riskMetricType,
                riskMetricCalculationType,
                isInverseDistribution,
                hcSubgroupDependent,
                hasHCSubgroups,
                skipPrivacySensitiveOutputs
            );
        }
    }
}
