using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ContributionBySourceRouteSectionBase : ExposureBySourceRouteSectionBase {
        public override bool SaveTemporaryData => true;
        public List<ContributionBySourceRouteRecord> Records { get; set; }

        protected List<ContributionBySourceRouteRecord> summarizeContributions(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRouteRecord>();

            var exposurePathCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var item in exposurePathCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionBySourceRouteRecord(
                        item.ExposurePath,
                        item.Exposures,
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

        private static ContributionBySourceRouteRecord getContributionBySourceRouteRecord(
            ExposurePath path,
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
            var record = new ContributionBySourceRouteRecord {
                ExposureSource = path.Source.GetShortDisplayName(),
                ExposureRoute = path.Route.GetShortDisplayName(),
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

        protected List<ContributionBySourceRouteRecord> SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRouteRecord>();

            var exposurePathCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var collection in exposurePathCollection) {
                if (collection.Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ContributionBySourceRouteRecord {
                        ExposureSource = collection.ExposurePath.Source.GetShortDisplayName(),
                        ExposureRoute = collection.ExposurePath.Route.GetShortDisplayName(),
                        Contribution = collection.Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight),
                    };
                    result.Add(record);
                }
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        protected void UpdateContributions(List<ContributionBySourceRouteRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureSource == record.ExposureSource && c.ExposureRoute == record.ExposureRoute)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
