﻿using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureHazardRatioModelledFoodSection : AtRiskSectionBase {
        public override bool SaveTemporaryData => true;

        private double _lowerPercentage;
        private double _upperPercentage;

        public List<RiskByModelledFoodRecord> Records { get; set; }

        /// <summary>
        /// Summarize risk modelled foods
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="lowerPercentage"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isInverseDistribution"></param>
        public void SummarizeRiskByFoods(
            IDictionary<Food, List<IndividualEffect>> individualEffects,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isInverseDistribution
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _riskPercentages = [_lowerPercentage, 50, _upperPercentage];
            _isInverseDistribution = isInverseDistribution;

            var allIndividualEffects = individualEffects
                .AsParallel()
                .SelectMany(c => c.Value)
                .ToList();

            var totalExposure = CalculateExposureHazardWeightedTotal(allIndividualEffects);

            var recordsBag = new ConcurrentBag<RiskByModelledFoodRecord>();

            Parallel.ForEach(individualEffects, kvp => {
                var record = createExposureHazardRatioFoodRecord(kvp.Value, kvp.Key, totalExposure);
                recordsBag.Add(record);
            });

            Records = recordsBag
                .OrderByDescending(c => c.Contribution)
                .ThenBy(c => c.FoodName)
                .ThenBy(c => c.FoodCode)
                .ToList();

            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        private RiskByModelledFoodRecord createExposureHazardRatioFoodRecord(
            List<IndividualEffect> individualEffects,
            Food food,
            double totalExposure
        ) {
            var (percentiles, percentilesAll, weights, allWeights, total, sumSamplingWeights) = CalculateExposureHazardPercentiles(
                individualEffects
            );
            var records = new RiskByModelledFoodRecord() {
                FoodName = food.Name,
                FoodCode = food.Code,
                Contributions = [],
                MeanAll = weights.Any() ? total / sumSamplingWeights : 0,
                Contribution = total / totalExposure,
                Percentile25 = percentiles[0],
                Median = percentiles[1],
                Percentile75 = percentiles[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                Mean = weights.Any() ? total / weights.Sum() : 0,
                FractionPositives = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count),
                PositivesCount = weights.Count,
            };
            return records;
        }

        public void SummarizeFoodsUncertainty(Dictionary<Food, List<IndividualEffect>> individualEffects) {
            var allIndividualEffects = individualEffects
                .AsParallel()
                .SelectMany(c => c.Value)
                .ToList();
            var totalExposure = CalculateExposureHazardWeightedTotal(allIndividualEffects);
            var records = individualEffects.Keys
                .Select(food => {
                    return new RiskByModelledFoodRecord() {
                        FoodCode = food.Code,
                        Contribution = CalculateExposureHazardWeightedTotal(individualEffects[food]) / totalExposure
                    };
                })
                .ToList();
            updateContributions(records);
        }

        private void updateContributions(List<RiskByModelledFoodRecord> records) {
            foreach (var record in Records) {
                var contribution = records
                    .FirstOrDefault(c => c.FoodCode == record.FoodCode)
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
