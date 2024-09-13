using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionsForIndividualsAtRiskSection : ContributionsForIndividualsUpperSection {

        public override void Summarize(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            double threshold,
            bool showOutliers
        ) {
            summarizeUpperIndividualContributions(
                individualEffects,
                individualEffectsBySubstances,
                null,
                threshold,
                showOutliers
            );
        }

        public override void SummarizeUncertainty(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            double threshold,
            double uncertaintyLower,
            double uncertaintyUpper
        ) {
            summarizeUncertainUpperDistribution(
                individualEffects,
                individualEffectsBySubstances,
                null,
                threshold,
                uncertaintyLower,
                uncertaintyUpper
            );
        }
    }
}
