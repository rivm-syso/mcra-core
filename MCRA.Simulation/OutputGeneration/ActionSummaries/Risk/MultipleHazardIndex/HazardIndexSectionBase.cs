using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class HazardIndexSectionBase : SummarySection {

        private static bool _summarizeTotal = false;
        public override bool SaveTemporaryData => true;
        public List<HazardIndexRecord> HazardIndexRecords { get; set; }
        public string EffectName { get; set; }
        public int NumberOfSubstances { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public double ConfidenceInterval { get; set; }
        public double[] HiBarPercentages;
        public double ThresholdHazardIndex { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public bool IsInverseDistribution { get; set; }
        public bool OnlyCumulativeOutput { get; set; }
        public bool UseIntraSpeciesFactor { get; set; }
        public double CED { get; set; } = double.NaN;
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }

        public List<HazardIndexRecord> GetHazardIndexMultipeRecords(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            RiskMetricCalculationType = riskMetricCalculationType;
            var records = new List<HazardIndexRecord>();
            if (substances.Count > 1 && individualEffects != null && isCumulative) {
                Compound riskReference = null;
                if (riskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.RpfWeighted.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                } else if (riskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.SumOfRiskRatios.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                }
                var record = createSubstanceHazardIndexRecord(individualEffects, riskReference, true, isInverseDistribution);
                records.Add(record);
            }
            foreach (var substance in substances) {
                records.Add(createSubstanceHazardIndexRecord(individualEffectsBySubstance[substance], substance, false, isInverseDistribution));
            }

            if (substances.Count > 1 && _summarizeTotal) {
                var hazardIndexBasedrecord = createSummedHazardIndexBySubstanceRecord(substances, individualEffectsBySubstance, isInverseDistribution);
                records.Add(hazardIndexBasedrecord);
            }
            return records.OrderByDescending(c => c.ProbabilityOfCriticalEffect).ToList();
        }

        public List<HazardIndexRecord> GetHazardIndexSingleRecord(
            Compound substance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            var records = new List<HazardIndexRecord>();
            if (individualEffects != null && isCumulative) {
                Compound riskReference = null;
                if (riskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.RpfWeighted.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                } else if (riskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.SumOfRiskRatios.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                }
                var record = createSubstanceHazardIndexRecord(individualEffects, riskReference, true, isInverseDistribution);
                records.Add(record);
            } else {
                records.Add(createSubstanceHazardIndexRecord(individualEffects, substance, false, isInverseDistribution));
            }
            return records;
        }

        /// <summary>
        /// Calculate hazard statistics per substances.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="isCumulativeRecord"></param>
        /// <returns></returns>
        private HazardIndexRecord createSubstanceHazardIndexRecord(
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
                .Where(c => c.IsPositive)
                .Select(c => c.SamplingWeight)
                .Sum();
            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.HazardIndex > ThresholdHazardIndex)
                .Select(c => c.SamplingWeight)
                .Sum();
            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = HiBarPercentages.Select(c => 100 - c);
                var marginOfExposures = individualEffects.Select(c => c.MarginOfExposure);
                percentiles = marginOfExposures.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => 1 / c).ToList();
            } else {
                percentiles = individualEffects.Select(c => c.HazardIndex)
                    .PercentilesWithSamplingWeights(allWeights, HiBarPercentages)
                    .ToList();
            }
            var record = new HazardIndexRecord() {
                CompoundName = substance.Name,
                CompoundCode = substance.Code,
                IsCumulativeRecord = isCumulativeRecord,
                PercentagePositives = sumWeightsPositives / sumAllWeights * 100D,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = new double[1] { ThresholdHazardIndex },
                    ReferenceValues = new List<double> { 100d * sumWeightsCriticalEffect / sumAllWeights },
                },
                HazardIndexPercentiles = new UncertainDataPointCollection<double>() {
                    XValues = HiBarPercentages,
                    ReferenceValues = new List<double> { percentiles[0], percentiles[1], percentiles[2] },
                },
            };

            return record;
        }

        /// <summary>
        /// Computes the sums of the substance hazard indexes for each individual and
        /// derives the statistics from this.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <returns></returns>
        private HazardIndexRecord createSummedHazardIndexBySubstanceRecord(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            bool isInverseDistribution
        ) {
            // Added for harmonic
            var sumSubstanceHazardIndices = Enumerable.Repeat(0D, individualEffectsBySubstance.First().Value.Count).ToList();
            foreach (var substance in substances) {
                var substanceHazardIndices = individualEffectsBySubstance[substance]
                    .Select(c => c.HazardIndex)
                    .ToList();
                for (int i = 0; i < individualEffectsBySubstance[substance].Count; i++) {
                    sumSubstanceHazardIndices[i] += substanceHazardIndices[i];
                }
            }

            var allWeights = individualEffectsBySubstance.First().Value.Select(c => c.SamplingWeight).ToList();

            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = HiBarPercentages.Select(c => 100 - c);
                var marginOfExposures = sumSubstanceHazardIndices.Select(c => 1 / c);
                percentiles = marginOfExposures.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => 1 / c).ToList();
            } else {
                percentiles = sumSubstanceHazardIndices.PercentilesWithSamplingWeights(allWeights, HiBarPercentages).ToList();
            }

            var sumAllWeights = allWeights.Sum();
            var sumWeightsPositiveEffects = sumSubstanceHazardIndices
                .Zip(allWeights, (val, weight) => (val, weight))
                .Where(r => r.val > 0)
                .Sum(r => r.weight);

            var sumWeightsCriticalEffects = sumSubstanceHazardIndices
                .Zip(allWeights, (val, weight) => (val, weight))
                .Where(r => r.val > ThresholdHazardIndex)
                .Sum(r => r.weight);

            var hazardIndexBasedrecord = new HazardIndexRecord() {
                CompoundCode = "HI Total",
                CompoundName = "HI Total (sum all substances)",
                IsCumulativeRecord = true,
                PercentagePositives = 100D * sumWeightsPositiveEffects / sumAllWeights,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = new double[1] { ThresholdHazardIndex },
                    ReferenceValues = new List<double> { 100D * sumWeightsCriticalEffects / sumAllWeights },
                },
                HazardIndexPercentiles = new UncertainDataPointCollection<double>() {
                    XValues = HiBarPercentages,
                    ReferenceValues = new List<double> { percentiles[0], percentiles[1], percentiles[2] },
                },
            };
            return hazardIndexBasedrecord;
        }
    }
}
