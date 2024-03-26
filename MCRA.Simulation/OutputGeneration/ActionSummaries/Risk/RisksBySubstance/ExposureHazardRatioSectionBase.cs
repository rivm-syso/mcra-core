using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ExposureHazardRatioSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        private static readonly bool _summarizeTotal = false;

        public string EffectName { get; set; }
        public RiskMetricType RiskMetricType { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }
        public bool IsInverseDistribution { get; set; }
        public List<(ExposureTarget Target, List<SubstanceRiskDistributionRecord> Records)> RiskRecords { get; set; } = new();
        public double ConfidenceInterval { get; set; }
        public double[] RiskBarPercentages;
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public double Threshold { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }

        public List<SubstanceRiskDistributionRecord> GetRiskMultipeRecords(
            ExposureTarget target,
            ICollection<Compound> substances,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            var records = new List<SubstanceRiskDistributionRecord>();
            if (substances.Count > 1 && individualEffects != null && isCumulative) {
                Compound riskReference = null;
                if (RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.RpfWeighted.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                } else if (RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.SumOfRiskRatios.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                }
                var record = createSubstanceRiskRecord(target, individualEffects, riskReference, true, isInverseDistribution);
                records.Add(record);
            }
            var individualEffectsDict = individualEffectsBySubstanceCollections?
                .SingleOrDefault(c => c.Target == target).IndividualEffects;
            if (individualEffectsDict?.Any() ?? false) {
                foreach (var substance in substances) {
                    if (individualEffectsDict.TryGetValue(substance, out var results)) {
                        records.Add(createSubstanceRiskRecord(target, results, substance, false, isInverseDistribution));
                    }
                }
            }

            if (substances.Count > 1 && _summarizeTotal) {
                var riskBasedrecord = createSummedRiskBySubstanceRecord(substances, individualEffectsDict, isInverseDistribution);
                records.Add(riskBasedrecord);
            }
            return records.OrderByDescending(c => c.ProbabilityOfCriticalEffect).ToList();
        }

        /// <summary>
        /// Calculate hazard statistics per substances.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="isCumulativeRecord"></param>
        /// <param name="isInverseDistribution"></param>
        /// <returns></returns>
        protected SubstanceRiskDistributionRecord createSubstanceRiskRecord(
            ExposureTarget target,
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
                .Where(c => c.ExposureHazardRatio > Threshold)
                .Select(c => c.SamplingWeight)
                .Sum();

            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = RiskBarPercentages.Select(c => 100 - c);
                var risks = individualEffects.Select(c => c.HazardExposureRatio);
                percentiles = risks.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => 1 / c).ToList();
            } else {
                percentiles = individualEffects.Select(c => c.ExposureHazardRatio)
                    .PercentilesWithSamplingWeights(allWeights, RiskBarPercentages)
                    .ToList();
            }

            var record = new SubstanceRiskDistributionRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                ExpressionType = target != null && target.ExpressionType != ExpressionType.None
                    ? target.ExpressionType.GetDisplayName() : null,
                IsCumulativeRecord = isCumulativeRecord,
                NumberOfIndividuals = individualEffects.Count,
                PercentagePositives = sumWeightsPositives / sumAllWeights * 100D,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = new double[1] { Threshold },
                    ReferenceValues = new List<double> { 100d * sumWeightsCriticalEffect / sumAllWeights },
                },
                RiskPercentiles = new UncertainDataPointCollection<double>() {
                    XValues = RiskBarPercentages,
                    ReferenceValues = new List<double> { percentiles[0], percentiles[1], percentiles[2] },
                },
            };
            return record;
        }

        /// <summary>
        /// Computes the sums of the substance risks for each individual and
        /// derives the statistics from this.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <returns></returns>
        private SubstanceRiskDistributionRecord createSummedRiskBySubstanceRecord(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            bool isInverseDistribution
        ) {
            // Added for harmonic
            var sumSubstanceRisks = Enumerable.Repeat(0D, individualEffectsBySubstance.First().Value.Count).ToList();
            foreach (var substance in substances) {
                if (individualEffectsBySubstance.TryGetValue(substance, out var results)) {
                    var substanceExposureHazardRatios = results
                        .Select(c => c.ExposureHazardRatio)
                        .ToList();
                    for (int i = 0; i < individualEffectsBySubstance[substance].Count; i++) {
                        sumSubstanceRisks[i] += substanceExposureHazardRatios[i];
                    }
                }
            }

            var allWeights = individualEffectsBySubstance.First().Value.Select(c => c.SamplingWeight).ToList();

            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = RiskBarPercentages.Select(c => 100 - c);
                var risks = sumSubstanceRisks.Select(c => 1 / c);
                percentiles = risks.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => 1 / c).ToList();
            } else {
                percentiles = sumSubstanceRisks.PercentilesWithSamplingWeights(allWeights, RiskBarPercentages).ToList();
            }

            var sumAllWeights = allWeights.Sum();
            var sumWeightsPositiveEffects = sumSubstanceRisks
                .Zip(allWeights, (val, weight) => (val, weight))
                .Where(r => r.val > 0)
                .Sum(r => r.weight);

            var sumWeightsCriticalEffects = sumSubstanceRisks
                .Zip(allWeights, (val, weight) => (val, weight))
                .Where(r => r.val > Threshold)
                .Sum(r => r.weight);

            var exposureHazardRatioBasedrecord = new SubstanceRiskDistributionRecord() {
                SubstanceCode = "Risk total",
                SubstanceName = "Risk total (sum all substances)",
                IsCumulativeRecord = true,
                NumberOfIndividuals = sumSubstanceRisks.Count,
                PercentagePositives = 100D * sumWeightsPositiveEffects / sumAllWeights,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = new double[1] { Threshold },
                    ReferenceValues = new List<double> { 100D * sumWeightsCriticalEffects / sumAllWeights },
                },
                RiskPercentiles = new UncertainDataPointCollection<double>() {
                    XValues = RiskBarPercentages,
                    ReferenceValues = new List<double> { percentiles[0], percentiles[1], percentiles[2] },
                },
            };
            return exposureHazardRatioBasedrecord;
        }
    }
}
