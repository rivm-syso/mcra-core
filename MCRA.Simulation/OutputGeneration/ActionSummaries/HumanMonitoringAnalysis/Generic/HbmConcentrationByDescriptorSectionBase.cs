using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmConcentrationByDescriptorSectionBase<S, T1, T2> : SummarySection
        where S : IHbmExposureContributorKey, new()
        where T1 : HbmConcentrationDistributionRecordBase<S>, new()
        where T2 : HbmBoxPlotRecordBase<S>, new() {

        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        protected readonly double _upperWhisker = 95;

        public ExposureType ExposureType { get; set; }
        public List<T1> Records { get; set; } = [];
        public SerializableDictionary<ExposureTarget, List<T2>> HbmBoxPlotRecords { get; set; } = [];
        public SerializableDictionary<ExposureTarget, List<T2>> StratifiedHbmBoxPlotRecords { get; set; } = [];
        public double? RestrictedUpperPercentile { get; set; }
        public bool ShowOutliers { get; set; }
        public bool DetailsSection { get; set; }

        protected static T1 CreateSummaryRecord(
            List<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            TargetUnit targetUnit,
            double[] percentages,
            bool stratified
        ) {
            if (concentrations != null) {
                var record = CreateSummaryRecord(
                    concentrations,
                    descriptor,
                    targetUnit,
                    stratified ? concentrations.FirstOrDefault()?.StratificationLevel : null,
                    percentages
                );
                if (!concentrations.Any()) {
                    return CreateMissingRecord(targetUnit, descriptor);
                }
                return record;
            }
            return null;
        }

        protected static T1 CreateSummaryRecord(
            List<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            TargetUnit targetUnit,
            IStratificationLevel stratificationLevel,
            double[] percentages
        ) {
            var sourceSamplingMethods = concentrations
                .SelectMany(c => c.SourceSamplingMethods)
                .GroupBy(c => c)
                .Select(c => c.Key.Name)
                .ToList();

            var weights = concentrations
                .Where(c => c.TotalEndpointExposure > 0)
                .Select(c => c.SamplingWeight)
                .ToList();
            var percentiles = concentrations
                .Where(c => c.TotalEndpointExposure > 0)
                .Select(c => c.TotalEndpointExposure)
                .PercentilesWithSamplingWeights(weights, percentages);

            var weightsAll = concentrations.Select(c => c.SamplingWeight).ToList();
            var percentilesAll = concentrations
                .Select(c => c.TotalEndpointExposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var record = new T1 {
                Stratification = stratificationLevel?.Name,
                CodeTargetSurface = targetUnit.Target.Code,
                BiologicalMatrix = targetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? targetUnit.BiologicalMatrix.GetDisplayName()
                    : null,
                ExposureRoute = targetUnit.ExposureRoute != ExposureRoute.Undefined
                    ? targetUnit.ExposureRoute.GetDisplayName()
                    : null,
                Unit = targetUnit.GetShortDisplayName(),
                ExpressionType = targetUnit?.ExpressionType != ExpressionType.None ? targetUnit?.ExpressionType.GetDisplayName() : "",
                MeanAll = concentrations.Sum(c => c.TotalEndpointExposure * c.SamplingWeight) / weightsAll.Sum(),
                PercentagePositives = weights.Count / (double)concentrations.Count * 100,
                MeanPositives = concentrations.Sum(c => c.TotalEndpointExposure * c.SamplingWeight) / weights.Sum(),
                LowerPercentilePositives = percentiles[0],
                Median = percentiles[1],
                UpperPercentilePositives = percentiles[2],
                LowerPercentileAll = percentilesAll[0],
                MedianAll = percentilesAll[1],
                UpperPercentileAll = percentilesAll[2],
                NumberOfDays = weights.Count,
                SourceSamplingMethods = string.Join(", ", sourceSamplingMethods),
                MedianAllUncertaintyValues = []
            };
            record.SetDescriptorValues(descriptor);
            return record;
        }

        protected List<T1> CreateStratifiedSummaryRecords(
            ICollection<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            TargetUnit targetUnit,
            double[] percentages,
            bool stratify
        ) {
            var results = new List<T1>();
            var groups = concentrations
                .GroupBy(c => stratify ? c.StratificationLevel : null);
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
            S descriptor
        ) {
            var record = new T1 {
                CodeTargetSurface = targetUnit.Target.Code,
                BiologicalMatrix = targetUnit.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? targetUnit.BiologicalMatrix.GetDisplayName()
                    : null,
                ExposureRoute = targetUnit.ExposureRoute != ExposureRoute.Undefined
                    ? targetUnit.ExposureRoute.GetDisplayName()
                    : null,
                Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                SourceSamplingMethods = null,
                MedianAllUncertaintyValues = null
            };
            record.SetDescriptorValues(descriptor);
            return record;
        }

        protected T2 CreateBoxPlotRecord(
            List<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            IStratificationLevel stratificationLevel,
            TargetUnit targetUnit
        ) {
            if (concentrations.Any(c => c.TotalEndpointExposure > 0)) {
                var sourceSamplingMethods = concentrations
                    .SelectMany(c => c.SourceSamplingMethods)
                    .GroupBy(c => c)
                    .Select(c => c.Key.Name)
                    .ToList();
                var weights = concentrations
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var allExposures = concentrations
                    .Select(c => c.TotalEndpointExposure)
                    .ToList();
                var percentiles = allExposures
                    .PercentilesWithSamplingWeights(weights, _percentages)
                    .ToList();
                var positives = allExposures.Where(r => r > 0).ToList();
                var outliers = allExposures
                    .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                        || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                    .Select(c => c)
                    .ToList();
                var record = new T2() {
                    Stratification = stratificationLevel?.Name,
                    MinPositives = positives.Any() ? positives.Min() : 0,
                    MaxPositives = positives.Any() ? positives.Max() : 0,
                    Percentiles = percentiles.ToList(),
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / concentrations.Count,
                    Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                    Outliers = outliers,
                    NumberOfOutLiers = outliers.Count,
                };
                record.SetDescriptorValues(descriptor);
                return record;
            }
            return null;
        }

        protected List<T2> CreateStratifiedBoxPlotRecords(
            ICollection<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            TargetUnit targetUnit
        ) {
            var result = new List<T2>();
            var groups = concentrations
                .GroupBy(c => c.StratificationLevel);
            foreach (var group in groups) {
                var results = CreateBoxPlotRecord(
                    [.. group],
                    descriptor,
                    group.Key,
                    targetUnit
                );
                result.AddRange(results);
            }
            return result;
        }

        protected void UpdateRecord(
            ICollection<HbmConcentrationsByDescriptor<S>> concentrations,
            S descriptor,
            ExposureTarget target,
            string stratifier,
            double lowerBound,
            double upperBound
        ) {
            var record = Records
                .SingleOrDefault(c => c.GetDescriptorKey() == descriptor.GetKey()
                    && c.CodeTargetSurface == target.Code
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
            ExposureTarget target,
            double lowerBound,
            double upperBound
        ) {
            var groups = concentrations.GroupBy(c => c.StratificationLevel);
            foreach (var group in groups) {
                UpdateRecord(
                    [.. group],
                    descriptor,
                    target,
                    group.Key.Name,
                    lowerBound,
                    upperBound
                );
            }
        }
    }
}

