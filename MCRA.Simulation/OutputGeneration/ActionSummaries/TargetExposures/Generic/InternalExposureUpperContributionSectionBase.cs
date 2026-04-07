using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class InternalExposureUpperContributionSectionBase<S, T> : InternalExposureContributionSectionBase<S, T>
        where S : IExposureContributorKey, new()
        where T : InternalExposureContributionRecordBase<S>, new() {
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

        protected List<T> summarize(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            PopulationStratifier outputStratifier,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isNominal
        ) {
            var results = summarizeUpperTailNominal(
                exposureCollection,
                percentageForUpperTail,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                isNominal
            );

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
                    var internalExposures = group
                        .GroupBy(g => g.Descriptor)
                        .Select(g => new InternalExposuresByDescriptor<S>() {
                            Descriptor = g.Key,
                            Exposures = [.. g.Select(e => (e.SimulatedIndividual, e.Exposure))]
                        }).ToList();

                    var result = summarizeUpperTailNominal(
                        internalExposures,
                        percentageForUpperTail,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound,
                        false,
                        outputStratifier

                    );
                    results.AddRange(result);
                }
            }
            return results;
        }

        private List<T> summarizeUpperTailNominal(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound, 
            bool isNominal,
            PopulationStratifier outputStratifier = null
        ) {
            var upperExposureCollection = getUpperTailExposures(
                exposureCollection,
                percentageForUpperTail,
                isNominal
            );

            var results = summarize(
                upperExposureCollection,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                outputStratifier
            );
            return results;
        }

        protected List<T> summarizeUncertainty(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            PopulationStratifier outputStratifier,
            double percentageForUpperTail
        ) {
            var results = summarizeUpperTailUncertainty(exposureCollection, null, percentageForUpperTail);

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
                    var internalExposures = group
                        .GroupBy(g => g.Descriptor)
                        .Select(g => new InternalExposuresByDescriptor<S>() {
                            Descriptor = g.Key,
                            Exposures = [.. g.Select(e => (e.SimulatedIndividual, e.Exposure))]
                        }).ToList();

                    var stratifiedResult = summarizeUpperTailUncertainty(
                        internalExposures,
                        outputStratifier,
                        percentageForUpperTail
                    );
                    results.AddRange(stratifiedResult);
                }
            }
            return results;
        }

        private List<T> summarizeUpperTailUncertainty(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            PopulationStratifier outputStratifier,
            double percentageForUpperTail
        ) {
            var upperExposureCollection = getUpperTailExposures(
                    exposureCollection,
                    percentageForUpperTail,
                    false
                );

            var results = new List<T>();
            foreach (var collection in upperExposureCollection) {
                if (collection.Exposures.Any(c => c.Exposure > 0)) {
                    var record = new T {
                        Contribution = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight),
                        Stratification = outputStratifier?.GetLevel(collection.Exposures.First().SimulatedIndividual)?.Name,
                    };
                    record.SetDescriptorValues(collection.Descriptor);
                    results.Add(record);
                }
            }
            var rescale = results.Sum(c => c.Contribution);
            results.ForEach(c => c.Contribution = c.Contribution / rescale);
            return results;
        }

        protected override List<T> summarize(
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
                    uncertaintyUpperBound,
                    outputStratifier?.GetLevel(c.Exposures.First().SimulatedIndividual)
                ))
                .ToList();
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderBy(c => c.Stratification).ThenBy(c => c.GetKey())]; ;
        }

        protected List<InternalExposuresByDescriptor<S>> getUpperTailExposures(
            List<InternalExposuresByDescriptor<S>> exposureCollection,
            double percentageForUpperTail,
            bool isNominal
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
                    Exposures = [.. item.Exposures.Where(e => individualIds.Contains(e.SimulatedIndividual))]
                })
                .ToList();
            return upperExposureCollection;
        }
    }
}
