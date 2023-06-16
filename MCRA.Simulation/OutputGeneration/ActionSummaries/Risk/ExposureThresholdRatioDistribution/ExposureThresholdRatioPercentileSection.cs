using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Calculates percentiles (output) for specified percentages (input)
    /// </summary>
    public class ExposureThresholdRatioPercentileSection : PercentileBootstrapSectionBase<IntakePercentileRiskBootstrapRecord> {
        public override bool SaveTemporaryData => true;

        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public ReferenceDoseRecord Reference { get; set; }
        public UncertainDataPoint<double> MeanOfExposureThresholdRatio { get; set; }
        public UncertainDataPoint<double> MeanHazardCharacterisation { get; set; }
        public UncertainDataPoint<double> MeanExposure { get; set; }
        public UncertainDataPointCollection<double> PercentilesExposure { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public bool IsInverseDistribution { get; set; }
        public bool IsHazardCharacterisationDistribution { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }

        public void Summarize(
            List<IndividualEffect> individualEffects,
            double[] percentages,
            IHazardCharacterisationModel referenceDose,
            HealthEffectType healthEffectType,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution
        ) {
            RiskMetricCalculationType = riskMetricCalculationType;
            HealthEffectType = healthEffectType;
            IsInverseDistribution = isInverseDistribution;
            IsHazardCharacterisationDistribution = individualEffects.Select(r => r.CriticalEffectDose).Distinct().Count() > 1;
            Reference = ReferenceDoseRecord.FromHazardCharacterisation(referenceDose);

            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var risks = individualEffects.Select(c => c.ExposureThresholdRatio).ToList();
            MeanOfExposureThresholdRatio = new UncertainDataPoint<double>() { ReferenceValue = risks.Average(weights) };

            var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            MeanHazardCharacterisation = new UncertainDataPoint<double>() { ReferenceValue = hazardCharacterisations.Average(weights) };

            var exposures = individualEffects.Select(c => c.ExposureConcentration).ToList();
            MeanExposure = new UncertainDataPoint<double>() { ReferenceValue = exposures.Average(weights) };

            if (isInverseDistribution) {
                var complementPercentage = percentages.Select(c => 100 - c);
                var thresholdExposureRatios = individualEffects.Select(c => c.ThresholdExposureRatio).ToList();
                Percentiles = new UncertainDataPointCollection<double> {
                    XValues = percentages,
                    ReferenceValues = thresholdExposureRatios.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => 1 / c)
                };
            } else {
                Percentiles = new UncertainDataPointCollection<double> {
                    XValues = percentages,
                    ReferenceValues = risks.PercentilesWithSamplingWeights(weights, percentages)
                };
            }
            PercentilesExposure = new UncertainDataPointCollection<double> {
                XValues = percentages,
                ReferenceValues = exposures.PercentilesWithSamplingWeights(weights, percentages)
            };
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle for Risk (HI)
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="isInverseDistribution"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        public void SummarizeUncertainty(
            List<IndividualEffect> individualEffects,
            bool isInverseDistribution,
            double lowerBound,
            double upperBound
        ) {
            UncertaintyLowerLimit = lowerBound;
            UncertaintyUpperLimit = upperBound;

            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var risks = individualEffects.Select(c => c.ExposureThresholdRatio).ToList();
            MeanOfExposureThresholdRatio.UncertainValues.Add(risks.Average(weights));

            var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            MeanHazardCharacterisation.UncertainValues.Add(hazardCharacterisations.Average(weights));

            var exposures = individualEffects.Select(c => c.ExposureConcentration).ToList();
            MeanExposure.UncertainValues.Add(exposures.Average(weights));

            if (isInverseDistribution) {
                var complementPercentage = Percentiles.XValues.Select(c => 100 - c);
                var thresholdExposureRatios = individualEffects.Select(c => c.ThresholdExposureRatio).ToList();
                Percentiles.AddUncertaintyValues(thresholdExposureRatios.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => 1 / c));
            } else {
                Percentiles.AddUncertaintyValues(risks.PercentilesWithSamplingWeights(weights, Percentiles.XValues.ToArray()));
            }
            PercentilesExposure.AddUncertaintyValues(exposures.PercentilesWithSamplingWeights(weights, PercentilesExposure.XValues.ToArray()));

        }

        public List<ExposureThresholdRatioPercentileRecord> GetHIPercentileRecords() {
            var result = Percentiles?
                .Select((p, i) => new ExposureThresholdRatioPercentileRecord {
                    XValues = p.XValue / 100,
                    ReferenceValue = p.ReferenceValue,
                    LowerBound = p.Percentile(UncertaintyLowerLimit),
                    UpperBound = p.Percentile(UncertaintyUpperLimit),
                    Median = p.MedianUncertainty,
                    ReferenceValueExposure = PercentilesExposure[i].ReferenceValue,
                    MedianExposure = PercentilesExposure[i].UncertainValues.Percentile(50),
                    LowerBoundExposure = PercentilesExposure[i].UncertainValues.Percentile(UncertaintyLowerLimit),
                    UpperBoundExposure = PercentilesExposure[i].UncertainValues.Percentile(UncertaintyUpperLimit),
                })
                .ToList() ?? new();

            return result;
        }
    }
}
