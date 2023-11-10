using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposureRatioModelledFoodSubstanceSection : AtRiskSectionBase {

        private double _lowerPercentage;
        private double _upperPercentage;
        public List<RiskByFoodSubstanceRecord> Records { get; set; }
        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarize risk modelled foods
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="lowerPercentage"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isInverseDistribution"></param>
        public void SummarizeRiskByModelledFoodSubstances(
             IDictionary<(Food Food, Compound Compound), List<IndividualEffect>> individualEffects,
             double lowerPercentage,
             double upperPercentage,
             double uncertaintyLowerBound,
             double uncertaintyUpperBound,
             bool isInverseDistribution
         ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _isInverseDistribution = isInverseDistribution;
            _riskPercentages = new double[3] { _lowerPercentage, 50, _upperPercentage };

            var allIndividualEffects = individualEffects
                .AsParallel()
                .SelectMany(c => c.Value)
                .ToList();

            var totalExposure = CalculateExposureHazardWeightedTotal(allIndividualEffects);
            var resultRecords = new ConcurrentBag<RiskByFoodSubstanceRecord>();

            Parallel.ForEach(individualEffects, kvp => {
                var record = createHazardExposureRatioModelledFoodSubstanceRecord(
                    kvp.Value,
                    kvp.Key.Food,
                    kvp.Key.Compound,
                    totalExposure
                );
                resultRecords.Add(record);
            });

            Records = resultRecords.OrderByDescending(c => c.Contribution).ToList();

            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        private RiskByFoodSubstanceRecord createHazardExposureRatioModelledFoodSubstanceRecord(
            List<IndividualEffect> individualEffects,
            Food food,
            Compound substance,
            double totalExposure
        ) {
            var (percentiles, percentilesAll, weights, allWeights, total, sumSamplingWeights) = CalculatesHazardExposurePercentiles(
                individualEffects
            );

            var record = new RiskByFoodSubstanceRecord() {
                FoodName = food.Name,
                FoodCode = food.Code,
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Contributions = new List<double>(),
                Total = weights.Any() ? total / sumSamplingWeights : SimulationConstants.MOE_eps,
                Contribution = total / totalExposure,
                Percentile25 = percentiles[0],
                Median = percentiles[1],
                Percentile75 = percentiles[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                Mean = weights.Any() ? total / weights.Sum() : SimulationConstants.MOE_eps,
                FractionPositives = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count),
                PositivesCount = weights.Count,
            };
            return record;
        }

        public void SummarizeModelledFoodSubstancesUncertainty(IDictionary<(Food Food, Compound Substance), List<IndividualEffect>> individualEffects) {
            var allIndividualEffects = individualEffects
                .AsParallel()
                .SelectMany(c => c.Value)
                .ToList();
            var totalExposure = CalculateExposureHazardWeightedTotal(allIndividualEffects);
            var records = individualEffects.Keys
                 .Select(key => new RiskByFoodSubstanceRecord() {
                     FoodCode = key.Food.Code,
                     SubstanceCode = key.Substance.Code,
                     Contribution = CalculateExposureHazardWeightedTotal(individualEffects[key]) / totalExposure
                 })
                 .ToList();
            updateContributions(records);
        }

        private void updateContributions(List<RiskByFoodSubstanceRecord> records) {
            foreach (var record in Records) {
                var contribution = records
                    .FirstOrDefault(c => c.SubstanceCode == record.SubstanceCode && c.FoodCode == record.FoodCode)
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
