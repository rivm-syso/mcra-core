﻿using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureThresholdRatioSectionBase : SummarySection {

        private static bool _summarizeTotal = false;
        public override bool SaveTemporaryData => true;
        public List<ExposureThresholdRatioRecord> ExposureThresholdRatioRecords { get; set; }
        public string EffectName { get; set; }
        public int NumberOfSubstances { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public double ConfidenceInterval { get; set; }
        public double[] HiBarPercentages;
        public double Threshold { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public bool IsInverseDistribution { get; set; }
        public bool OnlyCumulativeOutput { get; set; }
        public bool UseIntraSpeciesFactor { get; set; }
        public double CED { get; set; } = double.NaN;
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }

        public List<ExposureThresholdRatioRecord> GetRiskMultipeRecords(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            RiskMetricCalculationType = riskMetricCalculationType;
            var records = new List<ExposureThresholdRatioRecord>();
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
                var record = createSubstanceRiskRecord(individualEffects, riskReference, true, isInverseDistribution);
                records.Add(record);
            }
            foreach (var substance in substances) {
                records.Add(createSubstanceRiskRecord(individualEffectsBySubstance[substance], substance, false, isInverseDistribution));
            }

            if (substances.Count > 1 && _summarizeTotal) {
                var riskBasedrecord = createSummedRiskBySubstanceRecord(substances, individualEffectsBySubstance, isInverseDistribution);
                records.Add(riskBasedrecord);
            }
            return records.OrderByDescending(c => c.ProbabilityOfCriticalEffect).ToList();
        }

        public List<ExposureThresholdRatioRecord> GetRiskSingleRecord(
            Compound substance,
            List<IndividualEffect> individualEffects,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool isCumulative
        ) {
            var records = new List<ExposureThresholdRatioRecord>();
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
                var record = createSubstanceRiskRecord(individualEffects, riskReference, true, isInverseDistribution);
                records.Add(record);
            } else {
                records.Add(createSubstanceRiskRecord(individualEffects, substance, false, isInverseDistribution));
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
        private ExposureThresholdRatioRecord createSubstanceRiskRecord(
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
                .Where(c => c.ExposureThresholdRatio > Threshold)
                .Select(c => c.SamplingWeight)
                .Sum();
            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = HiBarPercentages.Select(c => 100 - c);
                var risks = individualEffects.Select(c => c.ThresholdExposureRatio);
                percentiles = risks.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => 1 / c).ToList();
            } else {
                percentiles = individualEffects.Select(c => c.ExposureThresholdRatio)
                    .PercentilesWithSamplingWeights(allWeights, HiBarPercentages)
                    .ToList();
            }
            var record = new ExposureThresholdRatioRecord() {
                CompoundName = substance.Name,
                CompoundCode = substance.Code,
                IsCumulativeRecord = isCumulativeRecord,
                PercentagePositives = sumWeightsPositives / sumAllWeights * 100D,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = new double[1] { Threshold },
                    ReferenceValues = new List<double> { 100d * sumWeightsCriticalEffect / sumAllWeights },
                },
                ExposureRisks = new UncertainDataPointCollection<double>() {
                    XValues = HiBarPercentages,
                    ReferenceValues = new List<double> { percentiles[0], percentiles[1], percentiles[2] },
                },
            };

            return record;
        }

        /// <summary>
        /// Computes the sums of the substance exposure/threshold value for each individual and
        /// derives the statistics from this.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="individualEffectsBySubstance"></param>
        /// <returns></returns>
        private ExposureThresholdRatioRecord createSummedRiskBySubstanceRecord(
            ICollection<Compound> substances,
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            bool isInverseDistribution
        ) {
            // Added for harmonic
            var sumSubstanceRisks = Enumerable.Repeat(0D, individualEffectsBySubstance.First().Value.Count).ToList();
            foreach (var substance in substances) {
                var substanceExposureThresholdRatios = individualEffectsBySubstance[substance]
                    .Select(c => c.ExposureThresholdRatio)
                    .ToList();
                for (int i = 0; i < individualEffectsBySubstance[substance].Count; i++) {
                    sumSubstanceRisks[i] += substanceExposureThresholdRatios[i];
                }
            }

            var allWeights = individualEffectsBySubstance.First().Value.Select(c => c.SamplingWeight).ToList();

            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = HiBarPercentages.Select(c => 100 - c);
                var risks = sumSubstanceRisks.Select(c => 1 / c);
                percentiles = risks.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => 1 / c).ToList();
            } else {
                percentiles = sumSubstanceRisks.PercentilesWithSamplingWeights(allWeights, HiBarPercentages).ToList();
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

            var exposureHazardRatioBasedrecord = new ExposureThresholdRatioRecord() {
                CompoundCode = "Exp/Threshold Total",
                CompoundName = "Exp/Threshold Total (sum all substances)",
                IsCumulativeRecord = true,
                PercentagePositives = 100D * sumWeightsPositiveEffects / sumAllWeights,
                ProbabilityOfCriticalEffects = new UncertainDataPointCollection<double>() {
                    XValues = new double[1] { Threshold },
                    ReferenceValues = new List<double> { 100D * sumWeightsCriticalEffects / sumAllWeights },
                },
                ExposureRisks = new UncertainDataPointCollection<double>() {
                    XValues = HiBarPercentages,
                    ReferenceValues = new List<double> { percentiles[0], percentiles[1], percentiles[2] },
                },
            };
            return exposureHazardRatioBasedrecord;
        }
    }
}