using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk.IndividualContributionsBySubstance;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionsForIndividualsSection : ContributionsForIndividualsSectionBase {
        public void Summarize(
            PopulationStratifier stratifier,
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            int uncertaintyCycles,
            bool showOutliers
        ) {
            ShowOutliers = showOutliers;
            summarize(
                stratifier,
                cumulativeIndividualRisks,
                individualEffectsBySubstances,
                uncertaintyCycles
            );
        }

        public void SummarizeUncertain(
            PopulationStratifier stratifier,
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances,
            double lowerBound,
            double upperBound
        ) {
            var ratioSumByIndividual = cumulativeIndividualRisks
                .ToDictionary(
                    r => r.SimulatedIndividualId,
                    r => r.ExposureHazardRatio
                );
            updateRecords(
                stratifier,
                ratioSumByIndividual,
                individualEffectsBySubstances,
                lowerBound,
                upperBound
            );

            if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                updateStratifiedRecords(
                    stratifier,
                    ratioSumByIndividual,
                    individualEffectsBySubstances,
                    lowerBound,
                    upperBound
                );
            }
        }
    }
}
