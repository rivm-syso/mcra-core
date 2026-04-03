using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class InternalExposureContributionSectionBase<S, T> : InternalExposuresByDescriptorSection<S>
        where S : IExposureContributorKey, new()
        where T : InternalExposureContributionRecordBase<S>, new() {

        public override bool SaveTemporaryData => true;
        public List<T> Records { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

        protected List<T> summarize(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            PopulationStratifier outputStratifier
        ) {
            var result = exposureCollection
                .Where(c => c.Exposures.Any(e => e.Exposure > 0))
                .Select(c => getContributionRecord(
                    c,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                ))
                .ToList();
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);

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

                    var stratifiedResults = internalExposures
                        .Where(c => c.Exposures.Any(e => e.Exposure > 0))
                        .Select(c => getContributionRecord(
                            c,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound,
                            outputStratifier?.GetLevel(c.Exposures.First().SimulatedIndividual)
                        )).ToList();
                    rescale = stratifiedResults.Sum(c => c.Contribution);
                    stratifiedResults.ForEach(c => c.Contribution = c.Contribution / rescale);
                    result.AddRange(stratifiedResults);
                }
            }
            return [.. result.OrderBy(c => c.Stratification).ThenBy(c => c.GetKey())]; ;
        }

        private static T getContributionRecord(
            InternalExposuresByDescriptor<S> collection,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            IStratificationLevel stratification = null
        ) {
            var weightsAll = collection.Exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var weights = collection.Exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var total = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new T {
                Stratification = stratification?.Name,
                Contribution = total,
                Percentage = weights.Count / (double)collection.Exposures.Count * 100,
                Mean = total / weightsAll.Sum(),
                NumberOfDays = weights.Count,
                Contributions = [],
                UncertaintyLowerBound = uncertaintyLowerBound,
                UncertaintyUpperBound = uncertaintyUpperBound
            };
            record.SetDescriptorValues(collection.Descriptor);
            return record;
        }

        protected static List<T> summarizeUncertainty(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            PopulationStratifier outputStratifier = null
        ) {
            var result = new List<T>();
            foreach (var collection in exposureCollection) {
                if (collection.Exposures.Any(c => c.Exposure > 0)) {
                    var record = new T {
                        Contribution = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight),
                        Stratification = null,
                    };
                    record.SetDescriptorValues(collection.Descriptor);
                    result.Add(record);
                }
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);

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
                    var stratifiedResult = new List<T>();
                    var internalExposures = group.GroupBy(c => c.Descriptor)
                        .Select(r => new InternalExposuresByDescriptor<S> {
                            Descriptor = r.Key,
                            Exposures = [.. r.Select(e => (
                                SimulatedIndividual: e.SimulatedIndividual,
                                Exposure: e.Exposure
                            ))]
                        }).ToList();
                    foreach (var collection in internalExposures) {
                        if (collection.Exposures.Any(c => c.Exposure > 0)) {
                            var record = new T {
                                Contribution = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight),
                                Stratification = outputStratifier?.GetLevel(collection.Exposures.First().SimulatedIndividual)?.Name,
                            };
                            record.SetDescriptorValues(collection.Descriptor);
                            stratifiedResult.Add(record);
                        }
                    }
                    rescale = stratifiedResult.Sum(c => c.Contribution);
                    stratifiedResult.ForEach(c => c.Contribution = c.Contribution / rescale);
                    result.AddRange(stratifiedResult);
                }
            }
            return [.. result.OrderBy(c => c.Stratification).ThenBy(c => c.GetKey())];
        }


        protected List<InternalExposuresByDescriptor<S>> getUpperTailExposures(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            double percentageForUpperTail,
            bool isNominal = false
        ) {
            var totalExposures = exposureCollection
                .SelectMany(c => c.Exposures)
                .GroupBy(c => c.SimulatedIndividual)
                .Select(c => (SimulatedIndividual: c.Key, Exposure: c.Sum(r => r.Exposure)))
                .ToList();

            var weights = totalExposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var intakeValue = totalExposures.Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperExposures = totalExposures
                .Where(c => c.Exposure >= intakeValue)
                .Select(c => (c.Exposure, c.SimulatedIndividual))
                .ToList();

            var individualIds = upperExposures.Select(c => c.SimulatedIndividual).ToHashSet();
            if (isNominal) {
                var exposures = upperExposures.Select(c => c.Exposure).ToList();
                NumberOfIntakes = upperExposures.Count;
                CalculatedUpperPercentage = upperExposures.Sum(c => c.SimulatedIndividual.SamplingWeight)
                    / totalExposures.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
                if (NumberOfIntakes > 0) {
                    LowPercentileValue = exposures.Min();
                    HighPercentileValue = exposures.Max();
                }
            }

            var upperExposureCollection = exposureCollection
                .Select(item => new InternalExposuresByDescriptor<S> {
                    Descriptor = item.Descriptor,
                    Exposures = item.Exposures.Where(e => individualIds.Contains(e.SimulatedIndividual)).ToList()
                })
                .ToList();
            return upperExposureCollection;
        }
        protected static void updateContributions(List<T> updateRecords, List<T> records) {
            foreach (var record in updateRecords) {
                var contribution = records
                    .FirstOrDefault(c => c.GetKey() == record.GetKey()
                        && c.Stratification == record.Stratification
                    )?.Contribution * 100 ?? double.NaN;
                if (double.IsNaN(contribution)) {
                    continue;
                }
                record.Contributions.Add(contribution);
            }
        }
    }
}
