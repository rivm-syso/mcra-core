﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ThresholdExposureRatioSubstanceSection : SummarySection {

        private readonly double _eps = 10E7D;
        private HealthEffectType _healthEffectType;
        private double[] _riskPercentages;
        private bool _isInverseDistribution;
        private double _lowerPercentage;
        private double _upperPercentage;

        public List<ThresholdExposureRatioSubstanceRecord> Records { get; set; }
        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarize risk modelled foods
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="lowerPercentage"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="isInverseDistribution"></param>
       public void SummarizeRiskBySubstances(
            Dictionary<Compound, List<IndividualEffect>> individualEffects,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> memberships,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            HealthEffectType healthEffectType,
            bool isInverseDistribution
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _riskPercentages = new double[3];
            _riskPercentages[0] = _lowerPercentage;
            _riskPercentages[1] = 50;
            _riskPercentages[2] = _upperPercentage;
            _isInverseDistribution = isInverseDistribution;
            _healthEffectType = healthEffectType;
            
            var totalExposure = individualEffects
                .Select(c =>  (
                    substance: c.Key,
                    value: c.Value
                ))
               .Sum(c => c.value.Sum(s=>s.ExposureConcentration * s.SamplingWeight) * relativePotencyFactors[c.substance] * memberships[c.substance]);

            Records = individualEffects.Keys.Select(substance => {
                return createThresholdExposureRatioSubstanceRecord(
                    individualEffects[substance], 
                    substance, 
                    totalExposure,
                    relativePotencyFactors[substance],
                    memberships[substance]
                );
            }).OrderByDescending(c => c.Contribution).ToList();
            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }
       
        private ThresholdExposureRatioSubstanceRecord createThresholdExposureRatioSubstanceRecord(
            List<IndividualEffect> individualEffects,
            Compound substance,
            double totalExposure,
            double rpf,
            double membership
        ) {
            var allWeights = individualEffects
                .Select(c => c.SamplingWeight)
                .ToList();
            var sumSamplingWeights = allWeights.Sum();
            var weights = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.SamplingWeight)
                .ToList();
            var samplingWeightsZeros = sumSamplingWeights - weights.Sum();

            var percentilesAll = new List<double>();
            var percentiles = new List<double>();
            var total = 0d;
            if (_isInverseDistribution) {
                var complementPercentages = _riskPercentages.Select(c => 100 - c);
                var risksAll = individualEffects
                    .Select(c => c.ExposureThresholdRatio);
                percentilesAll = risksAll.PercentilesWithSamplingWeights(allWeights, complementPercentages)
                    .Select(c => double.IsInfinity(c) ? _eps : 1 / c)
                    .ToList();
                var risks = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.ExposureThresholdRatio);
                percentiles = risks.PercentilesWithSamplingWeights(weights, complementPercentages)
                    .Select(c => double.IsInfinity(c) ? _eps : 1 / c)
                    .ToList();
                total = individualEffects.Sum(c => (double.IsInfinity(c.ExposureThresholdRatio) ? _eps : 1 / c.ExposureThresholdRatio) * c.SamplingWeight);
            } else {
                percentilesAll = individualEffects
                    .Select(c => c.ThresholdExposureRatio)
                    .PercentilesWithSamplingWeights(allWeights, _riskPercentages)
                    .ToList();
                percentiles = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.ThresholdExposureRatio)
                    .PercentilesWithSamplingWeights(weights, _riskPercentages)
                    .ToList();
                total = individualEffects.Sum(c => (double.IsInfinity(c.ThresholdExposureRatio) ? _eps : c.ThresholdExposureRatio) * c.SamplingWeight);
            }
            var record = new ThresholdExposureRatioSubstanceRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Contributions = new List<double>(),
                Total = total / sumSamplingWeights,
                Contribution = individualEffects.Sum(c => c.ExposureConcentration * c.SamplingWeight) * rpf * membership / totalExposure,
                Percentile25 = percentiles[0],
                Median = percentiles[1],
                Percentile75 = percentiles[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                Mean = total / weights.Sum(),
                Zero = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count),
                N = weights.Count,
            };
            return record;
        }

        public void SummarizeSubstancesUncertainty(
            Dictionary<Compound, List<IndividualEffect>> individualEffects,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> memberships
        ) {
            var totalExposure = individualEffects
                .Select(c => (
                    substance: c.Key,
                    value: c.Value
                ))
               .Sum(c => c.value.Sum(s => s.ExposureConcentration * s.SamplingWeight) * relativePotencyFactors[c.substance] * memberships[c.substance]);

            var records = individualEffects.Keys
                .Select(substance => new ThresholdExposureRatioSubstanceRecord() {
                    SubstanceCode = substance.Code,
                    Contribution = individualEffects[substance].Sum(c => c.ExposureConcentration * c.SamplingWeight) * relativePotencyFactors[substance] * memberships[substance] / totalExposure
                })
                .ToList();
            updateContributions(records);
        }

        private void updateContributions(List<ThresholdExposureRatioSubstanceRecord> records) {
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