using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Constants;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public abstract class InternalExposureDistributionSectionBase<S, T1, T2> : InternalExposuresByDescriptorSection<S>
        where S : IExposureContributorKey, new()
        where T1 : InternalExposureDistributionRecordBase<S>, new()
        where T2 : InternalExposureBoxPlotRecordBase<S>, new() {

        private readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];

        private static readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;
        public List<T1> Records { get; set; }
        public List<T2> BoxPlotRecords { get; set; }
        public List<T2> StratifiedBoxPlotRecords { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public bool ShowOutliers { get; set; }
        public TargetUnit TargetUnit { get; set; }

        protected List<T1> summarizeExposureRecords(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            double[] percentages,
            PopulationStratifier outputStratifier
        ) {
            var records = new List<T1>();
            foreach (var item in exposureCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var record = getExposureRecord(
                        item,
                        percentages
                    );
                    records.Add(record);
                }
            }
            if (outputStratifier != null) {
                var groups = exposureCollection
                    .SelectMany(c => c.Exposures.Select(e => (
                        Exposure: e.Exposure,
                        SimulatedIndividual: e.SimulatedIndividual,
                        Descriptor: c.Descriptor,
                        Stratifier: outputStratifier.GetLevel(e.SimulatedIndividual)
                    )))
                    .GroupBy(c => c.Stratifier)
                    .ToList();

                foreach (var group in groups) {
                    var internalExposures = group.GroupBy(c => c.Descriptor)
                        .Select(r => new InternalExposuresByDescriptor<S> {
                            Descriptor = r.Key,
                            Exposures = [.. r.Select(e => (
                                SimulatedIndividual: e.SimulatedIndividual,
                                Exposure: e.Exposure
                            ))]
                        }).ToList();
                    var results = internalExposures
                        .Select(c => getExposureRecord(
                             c,
                             percentages,
                             outputStratifier.GetLevel(c.Exposures.First().SimulatedIndividual)
                         )).ToList();
                    records.AddRange(results);
                }
            }
            return records;
        }

        protected List<T2> summarizeBoxPlotsRecords(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            TargetUnit targetUnit,
            IStratificationLevel stratification = null
        ) {
            TargetUnit = targetUnit;
            var records = new List<T2>();
            foreach (var item in exposureCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var boxPlotRecord = getBoxPlotRecord(
                        item,
                        targetUnit,
                        stratification
                    );
                    records.Add(boxPlotRecord);
                }
            }
            return records;
        }

        protected List<T2> summarizeStratifiedBoxPlots(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            TargetUnit targetUnit,
            PopulationStratifier outputStratifier
        ) {
            var records = new List<T2>();
            var groups = exposureCollection
                .SelectMany(c => c.Exposures.Select(e => (
                    Exposure: e.Exposure,
                    SimulatedIndividual: e.SimulatedIndividual,
                    Descriptor: c.Descriptor,
                    Stratifier: outputStratifier.GetLevel(e.SimulatedIndividual)
                )))
                .GroupBy(c => c.Stratifier)
                .ToList();

            foreach (var group in groups) {
                var internalExposures = group.GroupBy(c => c.Descriptor)
                    .Select(r => new InternalExposuresByDescriptor<S> {
                        Descriptor = r.Key,
                        Exposures = [.. r.Select(e => (
                            SimulatedIndividual: e.SimulatedIndividual,
                            Exposure: e.Exposure
                        ))]
                    }).ToList();

                var results = summarizeBoxPlotsRecords(
                    internalExposures,
                    targetUnit,
                    group.Key
                );
                records.AddRange(results);
            }
            return records;
        }

        protected void summarize(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            TargetUnit targetUnit,
            PopulationStratifier outputStratifier,
            bool skipPrivacySensitiveOutputs,
            double lowerPercentage,
            double upperPercentage
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            ShowOutliers = !skipPrivacySensitiveOutputs;
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(exposureCollection.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }

            Records = summarizeExposureRecords(exposureCollection, percentages, outputStratifier);

            BoxPlotRecords = summarizeBoxPlotsRecords(exposureCollection, targetUnit);

            if (outputStratifier != null) {
                StratifiedBoxPlotRecords = summarizeStratifiedBoxPlots(exposureCollection, targetUnit, outputStratifier);
            }
        }


        private static T1 getExposureRecord(
            InternalExposuresByDescriptor<S> collection,
            double[] percentages,
            IStratificationLevel stratification = null
        ) {
            var weights = collection.Exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentiles = collection.Exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);
            var weightsAll = collection.Exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentilesAll = collection.Exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var total = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new T1() {
                Stratification = stratification?.Name,
                Percentage = weights.Count / (double)collection.Exposures.Count * 100,
                MeanAll = total / weightsAll.Sum(),
                Mean = total / weights.Sum(),
                Percentile25 = percentiles[0],
                Median = percentiles[1],
                Percentile75 = percentiles[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                NumberOfDays = weights.Count,
            };
            record.SetDescriptorValues(collection.Descriptor);
            return record;
        }

        private T2 getBoxPlotRecord(
            InternalExposuresByDescriptor<S> collection,
            TargetUnit targetUnit,
            IStratificationLevel stratification
        ) {
            var weights = collection.Exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = collection.Exposures
                .Select(c => c.Exposure)
                .ToList();
            var percentiles = allExposures
                .PercentilesWithSamplingWeights(weights, _percentages)
                .ToList();
            var positives = allExposures
                .Where(r => r > 0)
                .ToList();
            var outliers = allExposures
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c)
                .ToList();

            var record = new T2() {
                Stratification = stratification?.Name,
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / collection.Exposures.Count,
                Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            record.SetDescriptorValues(collection.Descriptor);
            return record;
        }
    }
}
