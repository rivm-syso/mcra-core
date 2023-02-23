using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class MarginOfExposureSectionBase : SummarySection {

        private static bool _summarizeTotal = false;

        public override bool SaveTemporaryData => true;

        private readonly double _eps = 10E7D;

        public List<MarginOfExposureRecord> MOERecords { get; set; }

        public string EffectName { get; set; }
        public int NumberOfSubstances { get; set; }
        public double[] MoeBarPercentages;
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double ConfidenceInterval { get; set; }
        public double ThresholdMarginOfExposure { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public HealthEffectType HealthEffectType { get; set; }

        public bool IsInverseDistribution { get; set; }
        public bool OnlyCumulativeOutput { get; set; }

        public double CED { get; set; } = double.NaN;

        public List<MarginOfExposureRecord> GetMarginOfExposureMultipleRecords(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> substanceIndividualEffects,
            List<IndividualEffect> cumulativeIndividualEffects,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            var records = new List<MarginOfExposureRecord>();

            if (substances.Count > 1 && cumulativeIndividualEffects != null && isCumulative) {
                var rpfWeightedSubstance = new Compound() {
                    Name = RiskReferenceCompoundType.Cumulative.GetDisplayName(),
                    Code = RiskReferenceCompoundType.Cumulative.GetDisplayName(),
                };
                var record = createSubstanceMarginOfExposureRecord(cumulativeIndividualEffects, rpfWeightedSubstance, true, isInverseDistribution);
                records.Add(record);
            }

            foreach (var substance in substances) {
                var record = createSubstanceMarginOfExposureRecord(substanceIndividualEffects[substance], substance, false, isInverseDistribution);
                records.Add(record);
            }

            if (substances.Count > 1 && _summarizeTotal) {
                createSummedMarginOfExposureBySubstanceRecord(substances, substanceIndividualEffects, isInverseDistribution);
            }
            return records.OrderByDescending(c => c.ProbabilityOfCriticalEffect).ToList();
        }


        private MarginOfExposureRecord createSummedMarginOfExposureBySubstanceRecord(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> substanceIndividualEffects,
            bool isInverseDistribution
        ) {
            // Added for harmonic
            var sumSubstanceMarginOfExposures = Enumerable.Repeat(0D, substanceIndividualEffects.First().Value.Count).ToList();
            foreach (var substance in substances) {
                var inverseMOE = substanceIndividualEffects[substance]
                    .Select(c => c.HazardIndex(HealthEffectType))
                    .ToList();
                for (int i = 0; i < substanceIndividualEffects[substance].Count; i++) {
                    sumSubstanceMarginOfExposures[i] += inverseMOE[i];
                }
            }

            var moe = sumSubstanceMarginOfExposures
                .Select(r => r != 0 ? 1 / r : calculateMarginOfExposure(double.MaxValue, 0))
                .ToList();

            var allWeights = substanceIndividualEffects.First().Value.Select(c => c.SamplingWeight).ToList();


            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = MoeBarPercentages.Select(c => 100 - c);
                percentiles = sumSubstanceMarginOfExposures.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => c == 0 ? _eps : 1 / c).ToList();
            } else {
                percentiles = moe.PercentilesWithSamplingWeights(allWeights, MoeBarPercentages).ToList();
            }

            var sumAllWeights = allWeights.Sum();
            var sumWeightsPositiveEffects = sumSubstanceMarginOfExposures
                .Zip(allWeights, (val, weight) => (val, weight))
                .Where(r => r.val > 0)
                .Sum(r => r.weight);

            var sumWeightsCriticalEffects = moe
                .Zip(allWeights, (val, weight) => (val, weight))
                .Where(r => r.val < ThresholdMarginOfExposure)
                .Sum(r => r.weight);

            var record = new MarginOfExposureRecord() {
                CompoundName = RiskReferenceCompoundType.InverseHazardIndex.GetShortDisplayName(),
                CompoundCode = RiskReferenceCompoundType.InverseHazardIndex.GetShortDisplayName(),
                IsCumulativeRecord = true,
                PercentagePositives = 100D * sumWeightsPositiveEffects / sumAllWeights,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = new double[1] { ThresholdMarginOfExposure },
                    ReferenceValues = new List<double> { 100d * sumWeightsCriticalEffects / sumAllWeights },
                },
                MarginOfExposurePercentiles = new UncertainDataPointCollection<double>() {
                    XValues = MoeBarPercentages,
                    ReferenceValues = new List<double> { percentiles[0], percentiles[1], percentiles[2] },
                }
            };

            return record;
        }

        public List<MarginOfExposureRecord> GetMarginOfExposureSingleRecord(
           Compound substance,
           List<IndividualEffect> individualEffects,
           bool isInverseDistribution,
           bool isCumulative
       ) {
            var records = new List<MarginOfExposureRecord>();

            if (individualEffects != null && isCumulative) {
                var rpfWeightedSubstance = new Compound() {
                    Name = RiskReferenceCompoundType.Cumulative.GetDisplayName(),
                    Code = RiskReferenceCompoundType.Cumulative.GetDisplayName(),
                };
                var record = createSubstanceMarginOfExposureRecord(individualEffects, rpfWeightedSubstance, true, isInverseDistribution);
                records.Add(record);
            } else {
                records.Add(createSubstanceMarginOfExposureRecord(individualEffects, substance, false, isInverseDistribution));
            }
            return records;
        }

        /// <summary>
        /// Calculate imoe statistics per substances.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="isCumulativeRecord"></param>
        /// <returns></returns>
        private MarginOfExposureRecord createSubstanceMarginOfExposureRecord(
            List<IndividualEffect> individualEffects,
            Compound substance,
            bool isCumulativeRecord,
            bool isInverseDistribution
        ) {
            var allWeights = individualEffects
                .Select(c => c.SamplingWeight)
                .ToList();

            var sumAllWeights = allWeights.Sum();
            var sumWeightsPositives = individualEffects
                .Where(c => c.ExposureConcentration > 0)
                .Select(c => c.SamplingWeight)
                .Sum();
            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.MarginOfExposure(HealthEffectType) < ThresholdMarginOfExposure)
                .Select(c => c.SamplingWeight)
                .Sum();

            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = MoeBarPercentages.Select(c => 100 - c);
                var hazardIndices = individualEffects.Select(c => c.HazardIndex(HealthEffectType));
                percentiles = hazardIndices.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => c == 0 ? _eps : 1 / c).ToList();
            } else {
                percentiles = individualEffects.Select(c => c.MarginOfExposure(HealthEffectType))
                    .PercentilesWithSamplingWeights(allWeights, MoeBarPercentages)
                    .ToList();
            }

            var records = new MarginOfExposureRecord() {
                CompoundName = $"{substance.Name}",
                CompoundCode = $"{substance.Code}",
                IsCumulativeRecord = isCumulativeRecord,
                PercentagePositives = sumWeightsPositives / sumAllWeights * 100D,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = new double[1] { ThresholdMarginOfExposure },
                    ReferenceValues = new List<double> { 100d * sumWeightsCriticalEffect / sumAllWeights },
                },
                MarginOfExposurePercentiles = new UncertainDataPointCollection<double>() {
                    XValues = MoeBarPercentages,
                    ReferenceValues = new List<double> { percentiles[0], percentiles[1], percentiles[2] },
                },
            };
            return records;
        }

        /// <summary>
        /// Risk or benefit.
        /// </summary>
        /// <param name="iced"></param>
        /// <param name="iexp"></param>
        /// <returns></returns>
        private double calculateMarginOfExposure(double iced, double iexp) {
            if (HealthEffectType == HealthEffectType.Benefit) {
                return iced > iexp / _eps ? iexp / iced : _eps;
            } else {
                return iexp > iced / _eps ? iced / iexp : _eps;
            }
        }
    }
}
