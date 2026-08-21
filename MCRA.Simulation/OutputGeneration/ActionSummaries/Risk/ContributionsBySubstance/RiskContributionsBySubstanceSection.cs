using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.Stratification;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskContributionsBySubstanceSection : RiskContributionsBySubstanceSectionBase {

        /// <summary>
        /// Summarize risk substances total distribution.
        /// </summary>
        public void Summarize(
            PopulationStratifier stratifier,
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            RiskMetricType riskMetricType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution,
            int uncertaintyCycles
        ) {
            _lowerBound = uncertaintyLowerBound;
            _upperBound = uncertaintyUpperBound;
            _uncertaintyCycles = uncertaintyCycles;
            _riskPercentages = [lowerPercentage, 50, upperPercentage];
            _isInverseDistribution = isInverseDistribution;
            var individualEffects = getIndividualEffects(individualEffectsBySubstance, stratifier);

            ContributionRecords = individualEffects.SelectMany(c => 
                c.Effects,
                    (c, dict) => (c.Target, dict.Key, dict.Value)
                )
                .AsParallel()
                .Select(kvp => createContributionRecord(
                        individualEffects.First().Target,
                        kvp.Value,
                        kvp.Key,
                        riskMetricType,
                        null
                    ))
                    .OrderByDescending(c => c.Contribution)
                    .ThenBy(c => c.SubstanceName)
                    .ThenBy(c => c.SubstanceCode)
                    .ToList();

            if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                var stratifiedRecords = individualEffects.SelectMany(c => 
                    c.Effects, 
                        (c, dict) => (c.Target, dict.Key, dict.Value)
                    )
                    .AsParallel()
                    .SelectMany(kvp => createContributionRecords(
                        individualEffects.First().Target,
                        kvp.Value,
                        kvp.Key,
                        riskMetricType,
                        true
                    ))
                    .OrderByDescending(c => c.Contribution)
                    .ThenBy(c => c.SubstanceName)
                    .ThenBy(c => c.SubstanceCode)
                    .ToList();
                ContributionRecords.AddRange(stratifiedRecords);
            }
        }

        public void SummarizeUncertain(
            PopulationStratifier stratifier,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance
        ) {
            var individualEffects = getIndividualEffects(individualEffectsBySubstance, stratifier);
            updateRecords(individualEffects);

            if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                updateStratifiedRecords(individualEffects);
            }
        }
    }
}
