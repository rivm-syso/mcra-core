using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class RiskRatioBySubstanceSection : AtRiskSectionBase {

        protected double _lowerPercentage;
        protected double _upperPercentage;

        public List<RiskBySubstanceRecord> Records { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarize risk substances total distribution
        /// </summary>
        /// <param name="cumulativeIndividualRisks"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="lowerPercentage"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isInverseDistribution"></param>
        public void SummarizeTotalRiskBySubstances(
            List<IndividualEffect> cumulativeIndividualRisks,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _riskPercentages = new double[3] { _lowerPercentage, 50, _upperPercentage };
            _isInverseDistribution = isInverseDistribution;

            var totalExposureHazard = CalculateExposureHazardWeightedTotal(cumulativeIndividualRisks);

            Records = individualEffectsBySubstance
                .SelectMany(r => r.SubstanceIndividualEffects)
                .AsParallel()
                .Select(kvp => createSubstanceSummaryRecord(
                    kvp.Value,
                    kvp.Key,
                    totalExposureHazard
                ))
                .OrderByDescending(c => c.Contribution)
                .ThenBy(c => c.SubstanceName)
                .ThenBy(c => c.SubstanceCode)
                .ToList();

            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        /// <summary>
        /// Summarize risk substances total distribution
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="lowerPercentage"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isInverseDistribution"></param>
        public void SummarizeUpperRiskBySubstances(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution,
            double percentageForUpperTail
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _riskPercentages = new double[3] { _lowerPercentage, 50, _upperPercentage };
            _isInverseDistribution = isInverseDistribution;
            UpperPercentage = 100 - percentageForUpperTail;
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentile = individualEffects.Select(c => c.ExposureHazardRatio).PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualEffectsUpper = individualEffects
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToHashSet();
            CalculatedUpperPercentage = individualEffectsUpper.Sum(c => c.SamplingWeight) / weights.Sum() * 100;
            var totalExposure = CalculateExposureHazardWeightedTotal(individualEffectsUpper);

            Records = individualEffectsBySubstance
                .SelectMany(r => r.SubstanceIndividualEffects)
                .AsParallel()
                .Select(kvp => createSubstanceSummaryRecord(
                    kvp.Value.Where(c => simulatedIndividualIds.Contains(c.SimulatedIndividualId)).ToList(),
                    kvp.Key,
                    totalExposure
                ))
                .OrderByDescending(c => c.Contribution)
                .ThenBy(c => c.SubstanceName)
                .ThenBy(c => c.SubstanceCode)
                .ToList();

            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        public void SummarizeUncertain(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance
        ) {
            var totalExposure = CalculateExposureHazardWeightedTotal(individualEffects);
            var records = individualEffectsBySubstance
                .SelectMany(r => r.SubstanceIndividualEffects)
                .AsParallel()
                .Select(r => new RiskBySubstanceRecord() {
                    SubstanceCode = r.Key.Code,
                    Contribution = CalculateExposureHazardWeightedTotal(r.Value) / totalExposure
                })
                .ToList();
            updateContributions(records);
        }

        public void SummarizeUpperUncertain(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstance,
            double percentageForUpperTail
        ) {
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentile = individualEffects.Select(c => c.ExposureHazardRatio).PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var individualEffectsUpper = individualEffects
                .Where(c => c.ExposureHazardRatio > percentile)
                .ToList();
            var simulatedIndividualIds = individualEffectsUpper.Select(c => c.SimulatedIndividualId).ToHashSet();

            var totalExposure = CalculateExposureHazardWeightedTotal(individualEffectsUpper);
            var records = individualEffectsBySubstance
                .SelectMany(r => r.SubstanceIndividualEffects)
                .AsParallel()
                .Select(r => new RiskBySubstanceRecord() {
                    SubstanceCode = r.Key.Code,
                    Contribution = CalculateExposureHazardWeightedTotal(r.Value.Where(c => simulatedIndividualIds.Contains(c.SimulatedIndividualId)).ToList()) / totalExposure
                })
                .ToList();
            if (simulatedIndividualIds.Any()) {
                updateContributions(records);
            }
        }

        protected abstract RiskBySubstanceRecord createSubstanceSummaryRecord(
            List<IndividualEffect> individualEffects,
            Compound substance,
            double totalExposure
        );

        private void updateContributions(List<RiskBySubstanceRecord> records) {
            foreach (var record in Records) {
                var contribution = records
                    .FirstOrDefault(c => c.SubstanceCode == record.SubstanceCode)
                    ?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }

        private void setUncertaintyBounds(double lowerBound, double upperBound) {
            foreach (var item in Records) {
                item.UncertaintyLowerBound = lowerBound;
                item.UncertaintyUpperBound = upperBound;
            }
        }
    }
}
