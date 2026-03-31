using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public static class ContributionBySourceSectionBase {

        public static List<ContributionBySourceRecord> SummarizeContributions(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRecord>();
            var exposureCollection = ExposureBySourceCalculator.CalculateExposures(
                    externalIndividualExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    isPerPerson
                );

            foreach (var collection in exposureCollection) {
                if (collection.Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionBySourceRecord(
                        collection,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    result.Add(record);
                }
            }

            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        private static ContributionBySourceRecord getContributionBySourceRecord(
            InternalExposuresByDescriptor<SourceContributorKey> collection,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var weightsAll = collection.Exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var weights = collection.Exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var total = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new ContributionBySourceRecord {
                ExposureSource = collection.Descriptor.Source.GetShortDisplayName(),
                Contribution = total,
                Percentage = weights.Count / (double)collection.Exposures.Count * 100,
                Mean = total / weightsAll.Sum(),
                NumberOfDays = weights.Count,
                Contributions = [],
                UncertaintyLowerBound = uncertaintyLowerBound,
                UncertaintyUpperBound = uncertaintyUpperBound
            };
            return record;
        }

        public static List<ContributionBySourceRecord> SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRecord>();
            var exposureCollection = ExposureBySourceCalculator.CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var collection in exposureCollection) {
                if (collection.Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ContributionBySourceRecord {
                        ExposureSource = collection.Descriptor.Source.GetShortDisplayName(),
                        Contribution = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight)
                    };
                    result.Add(record);
                }
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        public static void UpdateContributions(List<ContributionBySourceRecord> updateRecords, List<ContributionBySourceRecord> records) {
            foreach (var record in updateRecords) {
                var contribution = records.FirstOrDefault(c => c.ExposureSource == record.ExposureSource)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
