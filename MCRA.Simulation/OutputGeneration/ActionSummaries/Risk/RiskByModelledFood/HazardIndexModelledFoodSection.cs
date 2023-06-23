using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardIndexModelledFoodSection : SummarySection {

        private HealthEffectType _healthEffectType;
        private double[] _hiPercentages;
        private bool _isInverseDistribution;
        private double _lowerPercentage;
        private double _upperPercentage;

        public List<HazardIndexModelledFoodRecord> Records { get; set; }
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
        /// 
        public void SummarizeRiskByFoods(
            Dictionary<Food, List<IndividualEffect>> individualEffects,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            HealthEffectType healthEffectType,
            bool isInverseDistribution
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _hiPercentages = new double[3];
            _hiPercentages[0] = _lowerPercentage;
            _hiPercentages[1] = 50;
            _hiPercentages[2] = _upperPercentage;
            _isInverseDistribution = isInverseDistribution;
            _healthEffectType = healthEffectType;
            var totalExposure = individualEffects
                .SelectMany(c => c.Value)
                .Sum(c => c.ExposureConcentration * c.SamplingWeight);
            Records = individualEffects.Keys.Select(food => {
                return createHazardIndexFoodRecord(individualEffects[food], food, totalExposure);
            }).OrderByDescending(c => c.Contribution).ToList();
            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        private HazardIndexModelledFoodRecord createHazardIndexFoodRecord(
            List<IndividualEffect> individualEffects,
            Food food,
            double totalExposure
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
                var complementPercentages = _hiPercentages.Select(c => 100 - c);
                var marginOfExposuresAll = individualEffects
                    .Select(c => c.MarginOfExposure);
                percentilesAll = marginOfExposuresAll.PercentilesWithSamplingWeights(allWeights, complementPercentages)
                    .Select(c => 1 / c)
                    .ToList();
                var marginOfExposures = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.MarginOfExposure);
                percentiles = marginOfExposures.PercentilesWithSamplingWeights(weights, complementPercentages)
                    .Select(c => 1 / c)
                    .ToList();
                total = individualEffects.Sum(c => 1 / c.MarginOfExposure * c.SamplingWeight);
            } else {
                percentilesAll = individualEffects
                    .Select(c => c.HazardIndex)
                    .PercentilesWithSamplingWeights(allWeights, _hiPercentages)
                    .ToList();
                percentiles = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.HazardIndex)
                    .PercentilesWithSamplingWeights(weights, _hiPercentages)
                    .ToList();
                total = individualEffects.Sum(c => c.HazardIndex * c.SamplingWeight);
            }
            var records = new HazardIndexModelledFoodRecord() {
                FoodName = food.Name,
                FoodCode = food.Code,
                Contributions = new List<double>(),
                Total = total / sumSamplingWeights,
                Contribution = individualEffects.Sum(c => c.ExposureConcentration * c.SamplingWeight) / totalExposure,
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
            return records;
        }

        public void SummarizeFoodsUncertainty(Dictionary<Food, List<IndividualEffect>> individualEffects) {
            var totalExposure = individualEffects
                .SelectMany(c => c.Value)
                .Sum(c => c.ExposureConcentration * c.SamplingWeight);
            var records = individualEffects.Keys.Select(food => {
                return new HazardIndexModelledFoodRecord() {
                    FoodCode = food.Code,
                    Contribution = individualEffects[food].Sum(c => c.ExposureConcentration * c.SamplingWeight) / totalExposure
                };
            }).ToList();
            updateContributions(records);
        }

        private void updateContributions(List<HazardIndexModelledFoodRecord> records) {
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
