using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ContributionBySourceSectionBase : ExposureBySourceSectionBase {
        public override bool SaveTemporaryData => true;
        public List<ContributionBySourceRecord> Records { get; set; }

        protected List<ContributionBySourceRecord> SummarizeContributions(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRecord>();
            var exposureSourceCollection = CalculateExposures(
                    externalIndividualExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    isPerPerson
                );

            foreach (var (Source, Exposures) in exposureSourceCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionBySourceRecord(
                        Source,
                        Exposures,
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
            ExposureSource source,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var weightsAll = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var weights = exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var total = exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new ContributionBySourceRecord {
                ExposureSource = source.GetShortDisplayName(),
                Contribution = total,
                Percentage = weights.Count / (double)exposures.Count * 100,
                Mean = total / weightsAll.Sum(),
                NumberOfDays = weights.Count,
                Contributions = [],
                UncertaintyLowerBound = uncertaintyLowerBound,
                UncertaintyUpperBound = uncertaintyUpperBound
            };
            return record;
        }

        protected List<ContributionBySourceRecord> SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRecord>();
            var exposureSourceCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (Source, Exposures) in exposureSourceCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ContributionBySourceRecord {
                        ExposureSource = Source.GetDisplayName(),
                        Contribution = Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight)
                    };
                    result.Add(record);
                }
            }

            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        protected void UpdateContributions(List<ContributionBySourceRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureSource == record.ExposureSource)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
