﻿using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Calculates percentiles (output) for specified percentages (input)
    /// </summary>
    public class HazardExposureRatioPercentileSection : PercentileBootstrapSectionBase<IntakePercentileRiskBootstrapRecord> {
        public override bool SaveTemporaryData => true;

        private readonly double _eps = 10E7D;
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public ReferenceDoseRecord Reference { get; set; }
        public UncertainDataPoint<double> MeanRisk { get; set; }
        public UncertainDataPoint<double> MeanHazardCharacterisation { get; set; }
        public UncertainDataPoint<double> MeanExposure { get; set; }
        public UncertainDataPointCollection<double> PercentilesExposure { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public bool IsInverseDistribution { get; set; }
        public bool IsHazardCharacterisationDistribution { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }

        public void Summarize(
            List<IndividualEffect> individualEffects,
            List<double> percentages,
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
            var risks = individualEffects.Select(c => c.HazardExposureRatio).ToList();
            MeanRisk = new UncertainDataPoint<double>() { ReferenceValue = risks.Average(weights) };

            var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            MeanHazardCharacterisation = new UncertainDataPoint<double>() { ReferenceValue = hazardCharacterisations.Average(weights) };

            var exposures = individualEffects.Select(c => c.ExposureConcentration).ToList();
            MeanExposure = new UncertainDataPoint<double>() { ReferenceValue = exposures.Average(weights) };

            if (isInverseDistribution) {
                var complementPercentage = percentages.Select(c => 100 - c);
                var exposureHazardRatios = individualEffects.Select(c => c.ExposureHazardRatio).ToList();
                Percentiles = new UncertainDataPointCollection<double> {
                    XValues = percentages,
                    ReferenceValues = exposureHazardRatios.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => double.IsInfinity(c) ? _eps : 1 / c)
                };
            } else {
                Percentiles = new UncertainDataPointCollection<double> {
                    XValues = percentages,
                    ReferenceValues = risks.PercentilesWithSamplingWeights(weights, percentages)
                };
            }
            var complementaryPercentages = percentages.Select(c => 100 - c).ToList();
            PercentilesExposure = new UncertainDataPointCollection<double> {
                XValues = complementaryPercentages,
                ReferenceValues = exposures.PercentilesWithSamplingWeights(weights, complementaryPercentages)
            };
        }

        /// <summary>
        /// Summarizes the results of a bootstrap cycle. 
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
            var risks = individualEffects.Select(c => c.HazardExposureRatio).ToList();
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();
            MeanRisk.UncertainValues.Add(risks.Average(weights));

            var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
            MeanHazardCharacterisation.UncertainValues.Add(hazardCharacterisations.Average(weights));

            var exposures = individualEffects.Select(c => c.ExposureConcentration).ToList();
            MeanExposure.UncertainValues.Add(exposures.Average(weights));

            if (isInverseDistribution) {
                var complementPercentage = Percentiles.XValues.Select(c => 100 - c);
                var exposureHazardRatios = individualEffects.Select(c => c.ExposureHazardRatio).ToList();
                Percentiles.AddUncertaintyValues(exposureHazardRatios.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => double.IsInfinity(c) ? _eps : 1 / c));
            } else {
                Percentiles.AddUncertaintyValues(risks.PercentilesWithSamplingWeights(weights, Percentiles.XValues.ToArray()));
            }
            PercentilesExposure.AddUncertaintyValues(exposures.PercentilesWithSamplingWeights(weights, PercentilesExposure.XValues.ToArray()));

        }

        public List<RiskPercentileRecord> GetRiskPercentileRecords() {
            var result = Percentiles?
                .Select((p, i) => new RiskPercentileRecord {
                    XValues = p.XValue / 100,
                    ExposurePercentage = 100 - p.XValue,
                    ReferenceValueExposure = PercentilesExposure[i].ReferenceValue,
                    MedianExposure = PercentilesExposure[i].UncertainValues.Percentile(50),
                    LowerBoundExposure = PercentilesExposure[i].UncertainValues.Percentile(UncertaintyLowerLimit),
                    UpperBoundExposure = PercentilesExposure[i].UncertainValues.Percentile(UncertaintyUpperLimit),
                    RisksPercentage = p.XValue,
                    ReferenceValue = p.ReferenceValue,
                    LowerBound = p.Percentile(UncertaintyLowerLimit),
                    UpperBound = p.Percentile(UncertaintyUpperLimit),
                    Median = p.MedianUncertainty,
                })
                .ToList() ?? new();

            return result;
        }
    }
}