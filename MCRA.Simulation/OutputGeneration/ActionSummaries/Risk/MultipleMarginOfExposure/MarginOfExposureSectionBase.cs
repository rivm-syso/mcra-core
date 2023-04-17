using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class MarginOfExposureSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        private readonly double _eps = 10E7D;

        public List<MarginOfExposureRecord> MoeRecords { get; set; }

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

        public List<MarginOfExposureRecord> GetMarginOfExposureSingleRecord(
           Compound substance,
           List<IndividualEffect> individualEffects,
           RiskMetricCalculationType riskMetricCalculationType,
           bool isInverseDistribution,
           bool isCumulative
       ) {
            var records = new List<MarginOfExposureRecord>();

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
                var record = createSubstanceMarginOfExposureRecord(individualEffects, riskReference, true, isInverseDistribution);
                records.Add(record);
            } else {
                records.Add(createSubstanceMarginOfExposureRecord(individualEffects, substance, false, isInverseDistribution));
            }
            return records;
        }

        /// <summary>
        /// Calculate IMOE statistics per substances.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="substance"></param>
        /// <param name="isCumulativeRecord"></param>
        /// <param name="isInverseDistribution"></param>
        /// <returns></returns>
        protected MarginOfExposureRecord createSubstanceMarginOfExposureRecord(
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
                .Where(c => c.MarginOfExposure < ThresholdMarginOfExposure)
                .Select(c => c.SamplingWeight)
                .Sum();

            // TODO: don't compute percentiles here, but use risk percentiles calculated
            // in the action calculator (passed as part of action result).
            var percentiles = new List<double>();
            if (isInverseDistribution) {
                var complementPercentages = MoeBarPercentages.Select(c => 100 - c);
                var hazardIndices = individualEffects.Select(c => c.HazardIndex);
                percentiles = hazardIndices.PercentilesWithSamplingWeights(allWeights, complementPercentages).Select(c => c == 0 ? _eps : 1 / c).ToList();
            } else {
                percentiles = individualEffects.Select(c => c.MarginOfExposure)
                    .PercentilesWithSamplingWeights(allWeights, MoeBarPercentages)
                    .ToList();
            }

            var result = new MarginOfExposureRecord() {
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
            return result;
        }
    }
}
