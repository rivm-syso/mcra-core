using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes percentiles for specified percentages.
    /// </summary>
    public abstract class RiskPercentileSectionBase : PercentileBootstrapSectionBase<IntakePercentileRiskBootstrapRecord> {
        public override bool SaveTemporaryData => true;

        public RiskMetricType RiskMetricType { get; set; }
        public RiskMetricCalculationType RiskMetricCalculationType { get; set; }
        public List<double> Percentages { get; set; }
        public List<double> SkippedPercentages { get; set; }
        public ReferenceDoseRecord Reference { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public bool IsInverseDistribution { get; set; }
        public bool IsHazardCharacterisationDistribution { get; set; }
        public bool HCSubgroupDependent { get; set; }

        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public UncertainDataPoint<double> MeanRisk { get; set; }
        public UncertainDataPoint<double> MeanHazardCharacterisation { get; set; }
        public UncertainDataPoint<double> MeanExposure { get; set; }
        public UncertainDataPointCollection<double> PercentilesExposure { get; set; }

        public void Summarize(
            List<IndividualEffect> individualEffects,
            double[] percentages,
            IHazardCharacterisationModel referenceDose,
            TargetUnit targetUnit,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isInverseDistribution,
            bool hcSubgroupDependent,
            bool hasHCSubgroups,
            bool skipPrivacySensitiveOutputs
        ) {
            RiskMetricType = riskMetricType;
            RiskMetricCalculationType = riskMetricCalculationType;
            IsInverseDistribution = isInverseDistribution;
            IsHazardCharacterisationDistribution = individualEffects
                .Select(r => r.CriticalEffectDose).Distinct().Count() > 1;
            HCSubgroupDependent = hcSubgroupDependent && hasHCSubgroups;

            Percentages = (riskMetricType == RiskMetricType.ExposureHazardRatio)
                ? percentages.ToList()
                : percentages
                    .Select(r => r > 50 ? 100 - r : r)
                    .OrderBy(r => r)
                    .ToList();

            if (skipPrivacySensitiveOutputs) {
                var sampleSize = individualEffects.Count;
                SkippedPercentages = Percentages
                    .Where(r => SimulationConstants.MinimalPercentileSampleSize(r) > sampleSize)
                    .ToList();
                Percentages = Percentages.Except(SkippedPercentages).ToList();
            }

            var risks = (riskMetricType == RiskMetricType.ExposureHazardRatio)
                ? individualEffects.Select(c => c.ExposureHazardRatio).ToList()
                : individualEffects.Select(c => c.HazardExposureRatio).ToList();
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();

            MeanRisk = new UncertainDataPoint<double>() { 
                ReferenceValue = risks.Average(weights) 
            };

            if (riskMetricType == RiskMetricType.ExposureHazardRatio) {
                if (isInverseDistribution) {
                    var complementPercentage = Percentages.Select(c => 100 - c);
                    var hazardExposureRatios = individualEffects.Select(c => c.HazardExposureRatio).ToList();
                    Percentiles = new UncertainDataPointCollection<double> {
                        XValues = Percentages,
                        ReferenceValues = hazardExposureRatios.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => 1 / c)
                    };
                } else {
                    Percentiles = new UncertainDataPointCollection<double> {
                        XValues = Percentages,
                        ReferenceValues = risks.PercentilesWithSamplingWeights(weights, Percentages)
                    };
                }
            } else {
                if (isInverseDistribution) {
                    var complementPercentage = Percentages.Select(c => 100 - c);
                    var exposureHazardRatios = individualEffects.Select(c => c.ExposureHazardRatio).ToList();
                    Percentiles = new UncertainDataPointCollection<double> {
                        XValues = Percentages,
                        ReferenceValues = exposureHazardRatios.PercentilesWithSamplingWeights(weights, complementPercentage)
                            .Select(c => double.IsInfinity(c) ? SimulationConstants.MOE_eps : 1 / c)
                    };
                } else {
                    Percentiles = new UncertainDataPointCollection<double> {
                        XValues = Percentages,
                        ReferenceValues = risks.PercentilesWithSamplingWeights(weights, Percentages)
                    };
                }
            }

            if (referenceDose != null) {
                Reference = new ReferenceDoseRecord(referenceDose.Substance);
                TargetUnit = targetUnit;

                var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
                MeanHazardCharacterisation = new UncertainDataPoint<double>() { 
                    ReferenceValue = hazardCharacterisations.Average(weights) 
                };

                var exposures = individualEffects.Select(c => c.Exposure).ToList();
                MeanExposure = new UncertainDataPoint<double>() {
                    ReferenceValue = exposures.Average(weights)
                };

                var exposurePercentages = percentages
                    .Select(c => c < 50 ? 100 - c : c)
                    .OrderBy(c => c)
                    .ToList();
                PercentilesExposure = new UncertainDataPointCollection<double> {
                    XValues = exposurePercentages,
                    ReferenceValues = exposures.PercentilesWithSamplingWeights(weights, exposurePercentages)
                };
            }
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

            var risks = (RiskMetricType == RiskMetricType.ExposureHazardRatio)
                ? individualEffects.Select(c => c.ExposureHazardRatio).ToList()
                : individualEffects.Select(c => c.HazardExposureRatio).ToList();
            var weights = individualEffects.Select(c => c.SamplingWeight).ToList();

            MeanRisk.UncertainValues.Add(risks.Average(weights));

            if (RiskMetricType == RiskMetricType.ExposureHazardRatio) {
                if (isInverseDistribution) {
                    var complementPercentage = Percentiles.XValues.Select(c => 100 - c);
                    var hazardExposureRatios = individualEffects.Select(c => c.HazardExposureRatio).ToList();
                    Percentiles.AddUncertaintyValues(hazardExposureRatios.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => 1 / c));
                } else {
                    Percentiles.AddUncertaintyValues(risks.PercentilesWithSamplingWeights(weights, Percentiles.XValues.ToArray()));
                }
            } else {
                if (isInverseDistribution) {
                    var complementPercentage = Percentiles.XValues.Select(c => 100 - c);
                    var exposureHazardRatios = individualEffects.Select(c => c.ExposureHazardRatio).ToList();
                    Percentiles.AddUncertaintyValues(exposureHazardRatios.PercentilesWithSamplingWeights(weights, complementPercentage).Select(c => double.IsInfinity(c) ? SimulationConstants.MOE_eps : 1 / c));
                } else {
                    Percentiles.AddUncertaintyValues(risks.PercentilesWithSamplingWeights(weights, Percentiles.XValues.ToArray()));
                }
            }

            if (Reference != null) {
                var hazardCharacterisations = individualEffects.Select(c => c.CriticalEffectDose).ToList();
                MeanHazardCharacterisation.UncertainValues.Add(hazardCharacterisations.Average(weights));

                var exposures = individualEffects.Select(c => c.Exposure).ToList();
                MeanExposure.UncertainValues.Add(exposures.Average(weights));

                PercentilesExposure.AddUncertaintyValues(exposures.PercentilesWithSamplingWeights(weights, PercentilesExposure.XValues.ToArray()));
            }
        }

        public List<RiskPercentileRecord> GetRiskPercentileRecords() {
            var result = Percentiles?
                .Select((p, i) => {
                    var percentilesUncertaintyRecords = (PercentilesExposure?.Any() ?? false)
                        ? PercentilesExposure[i] : null;
                    return new RiskPercentileRecord {
                        XValues = p.XValue / 100,
                        ExposurePercentage = RiskMetricType == RiskMetricType.ExposureHazardRatio ? p.XValue : 100 - p.XValue,
                        ReferenceValueExposure = percentilesUncertaintyRecords?.ReferenceValue,
                        MedianExposure = percentilesUncertaintyRecords?.UncertainValues.Percentile(50),
                        LowerBoundExposure = percentilesUncertaintyRecords?.UncertainValues.Percentile(UncertaintyLowerLimit),
                        UpperBoundExposure = percentilesUncertaintyRecords?.UncertainValues.Percentile(UncertaintyUpperLimit),
                        RisksPercentage = p.XValue,
                        ReferenceValue = p.ReferenceValue,
                        LowerBound = p.Percentile(UncertaintyLowerLimit),
                        UpperBound = p.Percentile(UncertaintyUpperLimit),
                        Median = p.MedianUncertainty,
                    };
                })
                .ToList() ?? new();
            return result;
        }
    }
}
