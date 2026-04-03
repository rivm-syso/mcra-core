using MCRA.Simulation.Calculators.Stratification;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic {
    public abstract class InternalExposurePercentileSectionBase<S, T> : InternalExposuresByDescriptorSection<S>
        where S : IExposureContributorKey, new()
        where T : InternalExposurePercentileRecordBase<S>, new() {

        public override bool SaveTemporaryData => true;

        public List<T> Records { get; set; }

        protected void summarize(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            PopulationStratifier outputStratifier,
            List<double> percentages) {
            Records = computePercentileRecords(
                exposureCollection,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                percentages
            );

            if (outputStratifier != null) {
                var stratifiedRecords = computePercentileRecords(
                    exposureCollection,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    percentages,
                    outputStratifier
                );
                Records.AddRange(stratifiedRecords);
            }
        }

        protected void summarize(
            List<double> percentages,
            PopulationStratifier outputStratifier,
            List<InternalExposuresByDescriptor<S>> exposureCollection
        ) {
            updatePercentileRecords(exposureCollection, percentages);

            if (outputStratifier != null) {
                updatePercentileRecords(exposureCollection, percentages, outputStratifier);
            }
        }

        private static List<T> computePercentileRecords(
            List<InternalExposuresByDescriptor<S>> collection,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            PopulationStratifier outputStratifier = null
        ) {
            var result = new List<T>();
            foreach (var item in collection) {
                var exposureGroups = item.Exposures
                    .Select(c => (
                        SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                        Stratification: outputStratifier?.GetLevel(c.SimulatedIndividual),
                        Exposure: c.Exposure
                    ))
                    .GroupBy(c => c.Stratification)
                    .ToList();
                foreach (var group in exposureGroups) {
                    if (group.Any(c => c.Exposure > 0)) {
                        var weights = group
                            .Select(c => c.SamplingWeight)
                            .ToList();
                        var percentiles = group
                            .Select(c => c.Exposure)
                            .PercentilesWithSamplingWeights(weights, percentages);

                        var zip = percentages.Zip(percentiles, (x, v) => new { X = x, V = v })
                            .ToList();

                        var records = zip
                            .Select(p => {
                                var record = new T() {
                                    Stratification = group.Key?.Name,
                                    UncertaintyLowerLimit = uncertaintyLowerBound,
                                    UncertaintyUpperLimit = uncertaintyUpperBound,
                                    XValue = p.X / 100,
                                    Value = p.V,
                                    Values = []
                                };
                                record.SetDescriptorValues(item.Descriptor);
                                return record;
                            })
                            .ToList();
                        result.AddRange(records);
                    }
                }
            }
            return result;
        }

        protected void updatePercentileRecords(
            List<InternalExposuresByDescriptor<S>> collection,
            List<double> percentages,
            PopulationStratifier outputStratifier = null
        ) {
            foreach (var item in collection) {
                var exposureGroups = item.Exposures
                    .Select(c => (
                        SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                        Stratification: outputStratifier?.GetLevel(c.SimulatedIndividual),
                        Exposure: c.Exposure
                    ))
                    .GroupBy(c => c.Stratification)
                    .ToList();
                foreach (var group in exposureGroups) {
                    if (group.Any(c => c.Exposure > 0)) {
                        var weights = group
                            .Select(c => c.SamplingWeight)
                            .ToList();
                        var percentiles = group
                            .Select(c => c.Exposure)
                            .PercentilesWithSamplingWeights(weights, percentages);

                        var record = new T();
                        record.SetDescriptorValues(item.Descriptor);

                        var records = Records
                           .Where(r => r.GetKey() == record.GetKey()
                               && r.Stratification == group.Key?.Name);
                        var zip = records.Zip(percentiles, (r, v) => new { Record = r, Value = v })
                            .ToList();
                        foreach (var zipItem in zip) {
                            zipItem.Record.Values.Add(zipItem.Value);
                        }
                    }
                }
            }
        }
    }
}
