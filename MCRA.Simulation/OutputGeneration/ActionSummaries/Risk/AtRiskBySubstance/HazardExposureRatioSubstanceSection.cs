using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposureRatioSubstanceSection : SummarySection {

        private readonly double _eps = 10E7D;
        private double[] _riskPercentages;
        private bool _isInverseDistribution;
        private double _lowerPercentage;
        private double _upperPercentage;

        public List<RiskBySubstanceRecord> Records { get; set; }
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
       public void SummarizeRiskBySubstances(
            Dictionary<Compound, List<IndividualEffect>> individualEffects,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> memberships,
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

            var rpfDict = new ConcurrentDictionary<Compound, double>(relativePotencyFactors);
            var mbsDict = new ConcurrentDictionary<Compound, double>(memberships);

            var totalExposure = individualEffects
                .AsParallel()
                .Sum(c => c.Value.Sum(s => s.Exposure * s.SamplingWeight) * rpfDict[c.Key] * mbsDict[c.Key]);

            var recordsBag = new ConcurrentBag<RiskBySubstanceRecord>();

            Parallel.ForEach(individualEffects, kvp => {
                var record = createHazardExposureRatioSubstanceRecord(
                    kvp.Value,
                    kvp.Key,
                    totalExposure,
                    rpfDict[kvp.Key],
                    mbsDict[kvp.Key]
                );
                recordsBag.Add(record);
            });

            Records = recordsBag.OrderByDescending(c => c.Contribution)
                .ThenBy(c => c.SubstanceName)
                .ThenBy(c => c.SubstanceCode)
                .ToList();

            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        private RiskBySubstanceRecord createHazardExposureRatioSubstanceRecord(
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
                    .Select(c => c.ExposureHazardRatio);
                percentilesAll = risksAll.PercentilesWithSamplingWeights(allWeights, complementPercentages)
                    .Select(c => double.IsInfinity(c) ? _eps : 1 / c)
                    .ToList();
                var risks = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.ExposureHazardRatio);
                percentiles = risks.PercentilesWithSamplingWeights(weights, complementPercentages)
                    .Select(c => double.IsInfinity(c) ? _eps : 1 / c)
                    .ToList();
                total = individualEffects.Sum(c => (double.IsInfinity(c.ExposureHazardRatio) ? _eps : 1 / c.ExposureHazardRatio) * c.SamplingWeight);
            } else {
                percentilesAll = individualEffects
                    .Select(c => c.HazardExposureRatio)
                    .PercentilesWithSamplingWeights(allWeights, _riskPercentages)
                    .ToList();
                percentiles = individualEffects
                    .Where(c => c.IsPositive)
                    .Select(c => c.HazardExposureRatio)
                    .PercentilesWithSamplingWeights(weights, _riskPercentages)
                    .ToList();
                total = individualEffects.Sum(c => (double.IsInfinity(c.HazardExposureRatio) ? _eps : c.HazardExposureRatio) * c.SamplingWeight);
            }
            var record = new RiskBySubstanceRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Contributions = new List<double>(),
                Total = total / sumSamplingWeights,
                Contribution = individualEffects.Sum(c => c.Exposure * c.SamplingWeight) * rpf * membership / totalExposure,
                Percentile25 = percentiles[0],
                Median = percentiles[1],
                Percentile75 = percentiles[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                Mean = total / weights.Sum(),
                FractionPositives = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count),
                PositivesCount = weights.Count,
            };
            return record;
        }

        public void SummarizeSubstancesUncertainty(
            Dictionary<Compound, List<IndividualEffect>> individualEffects,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> memberships
        ) {
            var totalExposure = individualEffects
               .Sum(c => c.Value.Sum(s => s.Exposure * s.SamplingWeight) * relativePotencyFactors[c.Key] * memberships[c.Key]);

            var records = individualEffects.Keys
                .Select(substance => new RiskBySubstanceRecord() {
                    SubstanceCode = substance.Code,
                    Contribution = individualEffects[substance].Sum(c => c.Exposure * c.SamplingWeight) * relativePotencyFactors[substance] * memberships[substance] / totalExposure
                })
                .ToList();
            updateContributions(records);
        }

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
