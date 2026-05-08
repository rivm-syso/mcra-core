using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmCumulativeDistributionBySubstanceSectionBase<S, T1, T2> : SummarySection
        where S : IHbmExposureContributorKey, new()
        where T1 : HbmConcentrationDistributionRecordBase<S>, new()
        where T2 : HbmBoxPlotRecordBase<S>, new() {

        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        protected readonly double _upperWhisker = 95;

        public ExposureType ExposureType { get; set; }
        public List<T1> Records { get; set; } = [];
        public (ExposureTarget, T2) BoxPlotRecord { get; set; }
        public (ExposureTarget, List<T2>) StratifiedHbmBoxPlotRecords { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public bool ShowOutliers { get; set; }

        protected static T1 CreateSummaryRecord(
            List<HbmConcentrationsByDescriptor<S>> individualConcentrations,
            S descriptor,
            TargetUnit targetUnit,
            IStratificationLevel stratificationLevel,
            double[] percentages
        ) {
            var weights = individualConcentrations
                .Where(c => c.TotalEndpointExposure > 0)
                .Select(c => c.SamplingWeight).ToList();
            var percentiles = individualConcentrations
                .Where(c => c.TotalEndpointExposure > 0)
                .Select(c => c.TotalEndpointExposure)
                .PercentilesWithSamplingWeights(weights, percentages);

            var weightsAll = individualConcentrations.Select(c => c.SamplingWeight).ToList();
            var percentilesAll = individualConcentrations
                .Select(c => c.TotalEndpointExposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var record = new T1 {
                Stratification = stratificationLevel?.Name,
                Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                CodeTargetSurface = targetUnit.Target.Code,
                BiologicalMatrix = targetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? targetUnit.BiologicalMatrix.GetDisplayName()
                    : null,
                ExposureRoute = targetUnit.ExposureRoute != ExposureRoute.Undefined
                    ? targetUnit.ExposureRoute.GetDisplayName()
                    : null,
                PercentagePositives = weights.Count / (double)individualConcentrations.Count * 100,
                MeanPositives = individualConcentrations.Sum(c => c.TotalEndpointExposure * c.SamplingWeight) / weights.Sum(),
                LowerPercentilePositives = percentiles[0],
                Median = percentiles[1],
                UpperPercentilePositives = percentiles[2],
                LowerPercentileAll = percentilesAll[0],
                MedianAll = percentilesAll[1],
                UpperPercentileAll = percentilesAll[2],
                NumberOfDays = weights.Count,
                MedianAllUncertaintyValues = [],
                MeanAll = individualConcentrations.Sum(c => c.TotalEndpointExposure * c.SamplingWeight) / weights.Sum(),
            };
            record.SetDescriptorValues(descriptor);
            return record;
        }

        protected List<T1> CreateStratifiedSummaryRecords(
            ICollection<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            TargetUnit targetUnit,
            double[] percentages
        ) {
            var results = new List<T1>();
            var groups = concentrations
                .GroupBy(c => c.StratificationLevel);
            foreach (var group in groups) {
                var record = CreateSummaryRecord(
                    [.. group],
                    descriptor,
                    targetUnit,
                    group.Key,
                    percentages
                );
                results.Add(record);
            }
            return results;
        }

        protected static T1 CreateMissingRecord(
            TargetUnit targetUnit,
            HbmConcentrationsByDescriptor<S> descriptor
        ) {
            var record = new T1 {
                CodeTargetSurface = targetUnit.Target.Code,
                BiologicalMatrix = targetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? targetUnit.BiologicalMatrix.GetDisplayName()
                    : null,
                ExposureRoute = targetUnit.ExposureRoute != ExposureRoute.Undefined
                    ? targetUnit.ExposureRoute.GetDisplayName()
                    : null,
                Unit = targetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType),
                SourceSamplingMethods = null,
                MedianAllUncertaintyValues = null
            };
            record.SetDescriptorValues(descriptor.Descriptor);
            return record;
        }

        protected T2 CreateBoxPlotRecord(
            List<HbmConcentrationsByDescriptor<S>> concentrations,
            IStratificationLevel stratificationLevel,
            S descriptor,
            TargetUnit targetUnit
        ) {
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            if (concentrations.Any(c => c.TotalEndpointExposure > 0)) {
                var weights = concentrations
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var allExposures = concentrations
                    .Select(c => c.TotalEndpointExposure)
                    .ToList();
                var percentiles = allExposures
                    .PercentilesWithSamplingWeights(weights, percentages)
                    .ToList();
                var positives = allExposures.Where(r => r > 0).ToList();
                var record = new T2() {
                    Stratification = stratificationLevel?.Name,
                    MinPositives = positives.Any() ? positives.Min() : 0,
                    MaxPositives = positives.Any() ? positives.Max() : 0,
                    Percentiles = [.. percentiles],
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / concentrations.Count,
                    Unit = targetUnit?.GetShortDisplayName()
                };
                record.SetDescriptorValues(descriptor);
                return record;
            }
            return null;
        }

        protected List<T2> CreateStratifiedBoxPlotRecords(
            List<HbmConcentrationsByDescriptor<S>> stratifiedConcentrations,
            S descriptor,
            TargetUnit targetUnit
        ) {
            var result = new List<T2>();
            var groups = stratifiedConcentrations.GroupBy(c => c.StratificationLevel);
            foreach (var group in groups) {
                var stratifiedResults = CreateBoxPlotRecord(
                    [.. group],
                    group.Key,
                    descriptor,
                    targetUnit
                );
                result.Add(stratifiedResults);
            }
            return result;
        }

        protected void UpdateRecord(
            ICollection<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            string stratifier,
            double lowerBound,
            double upperBound
        ) {
            var record = Records
                .SingleOrDefault(c => c.GetDescriptorKey() == descriptor.GetKey()
                    && c.Stratification == stratifier
                );
            var weightsAll = concentrations.Select(c => c.SamplingWeight).ToList();
            var medianAll = concentrations
                .Select(c => c.TotalEndpointExposure)
                .PercentilesWithSamplingWeights(weightsAll, 50);

            if (record != null) {
                record.MedianAllUncertaintyValues.Add(medianAll);
                record.LowerUncertaintyBound = lowerBound;
                record.UpperUncertaintyBound = upperBound;
            }
        }

        protected void UpdateStratifiedRecords(
            ICollection<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            double lowerBound,
            double upperBound
        ) {
            var groups = concentrations.GroupBy(c => c.StratificationLevel);
            foreach (var group in groups) {
                UpdateRecord(
                    [.. group],
                    descriptor,
                    group.Key.Name,
                    lowerBound,
                    upperBound
                );
            }
        }
    }
}

