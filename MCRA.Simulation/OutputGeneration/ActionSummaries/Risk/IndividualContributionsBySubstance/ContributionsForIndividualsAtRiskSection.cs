using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionsForIndividualsAtRiskSection : ContributionsForIndividualsUpperSection {

        public override void Summarize(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            RiskMetricType riskMetricType,
            double threshold,
            bool showOutliers
        ) {
            summarizeUpperIndividualContributions(
                individualEffects,
                individualEffectsBySubstances,
                riskMetricType,
                null,
                threshold,
                showOutliers
            );
        }

        public override void SummarizeUncertainty(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            RiskMetricType riskMetricType, 
            double threshold,
            double uncertaintyLower,
            double uncertaintyUpper
        ) {
            summarizeUncertainUpperDistribution(
                individualEffects,
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
