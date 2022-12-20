using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using System.Collections.Generic;
using System.Linq;

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

        public List<HazardIndexRecord> GetHazardIndexMultipeRecords(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> substanceIndividualEffects,
            List<IndividualEffect> cumulativeIndividualEffects,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            var records = new List<HazardIndexRecord>();
            if (substances.Count > 1 && cumulativeIndividualEffects != null && isCumulative) {
                var rpfWeightedSubstance = new Compound() {
                    Code = RiskReferenceCompoundType.Cumulative.GetDisplayName(),
                    Name = RiskReferenceCompoundType.Cumulative.GetDisplayName()
                };
                var record = createSubstanceHazardIndexRecord(cumulativeIndividualEffects, rpfWeightedSubstance, true, isInverseDistribution);
                records.Add(record);
            }
            foreach (var substance in substances) {
                records.Add(createSubstanceHazardIndexRecord(substanceIndividualEffects[substance], substance, false, isInverseDistribution));
            }

            if (substances.Count > 1 && _summarizeTotal) {
                var hazardIndexBasedrecord = createSummedHazardIndexBySubstanceRecord(substances, substanceIndividualEffects, isInverseDistribution);
                records.Add(hazardIndexBasedrecord);
            }
            return records.OrderByDescending(c => c.ProbabilityOfCriticalEffect).ToList();
        }

        public List<HazardIndexRecord> GetHazardIndexSingleRecord(
            Compound substance,
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            var records = new List<HazardIndexRecord>();
            if (individualEffects != null && isCumulative) {
                var rpfWeightedSubstance = new Compound() {
                    Code = RiskReferenceCompoundType.Cumulative.GetDisplayName(),
                    Name = RiskReferenceCompoundType.Cumulative.GetDisplayName()
                };
                var record = createSubstanceHazardIndexRecord(individualEffects, rpfWeightedSubstance, true, isInverseDistribution);
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
                .Where(c => c.ExposureConcentration > 0)
                .Select(c => c.SamplingWeight)
                .Sum();
            var sumWeightsCriticalEffect = individualEffects
                .Where(c => c.HazardIndex(HealthEffectType) > ThresholdHazardIndex)
                .Select(c => c.SamplingWeight)
                .Sum();
            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = HiBarPercentages.Select(c => 100 - c);
                var marginOfExposures = individualEffects.Select(c => c.MarginOfExposure(HealthEffectType));
                percentiles = marginOfExposures.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => 1 / c).ToList();
            } else {
                percentiles = individualEffects.Select(c => c.HazardIndex(HealthEffectType))
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
        /// <param name="substanceIndividualEffects"></param>
        /// <returns></returns>
        private HazardIndexRecord createSummedHazardIndexBySubstanceRecord(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> substanceIndividualEffects,
            bool isInverseDistribution
        ) {
            // Added for harmonic
            var sumSubstanceHazardIndices = Enumerable.Repeat(0D, substanceIndividualEffects.First().Value.Count).ToList();
            foreach (var substance in substances) {
                var substanceHazardIndices = substanceIndividualEffects[substance]
                    .Select(c => c.HazardIndex(HealthEffectType))
                    .ToList();
                for (int i = 0; i < substanceIndividualEffects[substance].Count; i++) {
                    sumSubstanceHazardIndices[i] += substanceHazardIndices[i];
                }
            }

            var allWeights = substanceIndividualEffects.First().Value.Select(c => c.SamplingWeight).ToList();

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
