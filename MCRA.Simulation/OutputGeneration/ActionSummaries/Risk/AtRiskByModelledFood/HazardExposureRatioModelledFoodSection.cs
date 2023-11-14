using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposureRatioModelledFoodSection : AtRiskSectionBase {
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
            _riskPercentages = new double[3] { _lowerPercentage, 50, _upperPercentage };
            _isInverseDistribution = isInverseDistribution;

            var allIndividualEffects = individualEffects
                .AsParallel()
                .SelectMany(c => c.Value)
                .ToList();

            var totalExposure = CalculateExposureHazardWeightedTotal(allIndividualEffects);

            var recordsBag = new ConcurrentBag<RiskByModelledFoodRecord>();

            Parallel.ForEach(individualEffects, kvp => {
                var record = createHazardExposureRatioFoodRecord(kvp.Value, kvp.Key, totalExposure);
                recordsBag.Add(record);
            });

            Records = recordsBag
                .OrderByDescending(c => c.Contribution)
                .ThenBy(c => c.FoodName)
                .ThenBy(c => c.FoodCode)
                .ToList();

            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }
        private RiskByModelledFoodRecord createHazardExposureRatioFoodRecord(
            List<IndividualEffect> individualEffects,
            Food food,
            double totalExposure
        ) {
            var (percentiles, percentilesAll, weights, allWeights, total, sumSamplingWeights) = CalculatesHazardExposurePercentiles(
                individualEffects
            );

            var record = new RiskByModelledFoodRecord() {
                FoodName = food.Name,
                FoodCode = food.Code,
                Contributions = new List<double>(),
                MeanAll = weights.Any() ? total / sumSamplingWeights : SimulationConstants.MOE_eps,
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
        public void SummarizeFoodsUncertainty(Dictionary<Food, List<IndividualEffect>> individualEffects) {
            var allIndividualEffects = individualEffects
                .AsParallel()
                .SelectMany(c => c.Value)
                .ToList();
            var totalExposure = CalculateExposureHazardWeightedTotal(allIndividualEffects); 
            var records = individualEffects.Keys
                .Select(food => new RiskByModelledFoodRecord() {
                    FoodCode = food.Code,
                    Contribution = CalculateExposureHazardWeightedTotal(individualEffects[food]) / totalExposure
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
