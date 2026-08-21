using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionsForIndividualsAtRiskSection : ContributionsForIndividualsUpperSection {

        public override void Summarize(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            RiskMetricType riskMetricType,
            double threshold,
            int uncertaintyCycles,
            bool showOutliers
        ) {
            summarizeUpperIndividualContributions(
                cumulativeIndividualRisks,
                individualEffectsBySubstances,
                riskMetricType,
                null,
                threshold,
                uncertaintyCycles,
                showOutliers
            );
        }

        public override void SummarizeUncertainty(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            RiskMetricType riskMetricType,
            double threshold,
            double uncertaintyLower,
            double uncertaintyUpper
        ) {
            summarizeUncertainUpperDistribution(
                cumulativeIndividualRisks,
                individualEffectsBySubstances,
                riskMetricType,
                null,
                threshold,
                uncertaintyLower,
                uncertaintyUpper
            );
        }
    }
}
