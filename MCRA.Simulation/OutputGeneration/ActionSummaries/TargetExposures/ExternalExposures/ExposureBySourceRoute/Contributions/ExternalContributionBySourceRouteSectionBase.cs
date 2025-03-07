using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ExternalContributionBySourceRouteSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ExternalContributionBySourceRouteRecord> Records { get; set; }

        protected List<ExternalContributionBySourceRouteRecord> summarizeContributions(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple externalExposureUnit,
            HashSet<int> individualsIds,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ExternalContributionBySourceRouteRecord>();
            var ids = individualsIds
                ?? externalExposureCollections
                    .First()
                    .ExternalIndividualDayExposures
                    .Select(c => c.SimulatedIndividual.Id)
                    .ToHashSet();

            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposures = collection.ExternalIndividualDayExposures
                        .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                        .Select(id => (
                            Exposure: id.GetExposure(route, relativePotencyFactors, membershipProbabilities, isPerPerson),
                            SamplingWeight: id.SimulatedIndividual.SamplingWeight
                        ))
                        .ToList();
                    if (exposures.Any(c => c.Exposure > 0)) {
                        var record = getContributionBySourceRouteRecord(
                            collection.ExposureSource,
                            route,
                            exposures,
                            uncertaintyLowerBound,
                            uncertaintyUpperBound
                        );
                        result.Add(record);
                    }
                }
            };
            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans
                    .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                    .Select(id => (
                        Exposure: id.DietaryIntakePerMassUnit,
                        SamplingWeight: id.SimulatedIndividual.SamplingWeight
                    )).ToList();
                var dietaryRecord = getContributionBySourceRouteRecord(
                    ExposureSource.Diet,
                    ExposureRoute.Oral,
                    oims,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
                result.Add(dietaryRecord);
            }

            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        private static ExternalContributionBySourceRouteRecord getContributionBySourceRouteRecord(
            ExposureSource source,
            ExposureRoute route,
            List<(double Exposure, double SamplingWeight)> exposures,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var weightsAll = exposures
                .Select(c => c.SamplingWeight)
                .ToList();
            var weights = exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SamplingWeight)
                .ToList();
            var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
            var record = new ExternalContributionBySourceRouteRecord {
                ExposureSource = source.GetShortDisplayName(),
                ExposureRoute = route.GetShortDisplayName(),
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

        protected List<ExternalContributionBySourceRouteRecord> SummarizeUncertainty(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple externalExposureUnit,
            HashSet<int> individualsIds,
            bool isPerPerson
        ) {
            var result = new List<ExternalContributionBySourceRouteRecord>();
            var ids = individualsIds
                ?? externalExposureCollections
                    .First()
                    .ExternalIndividualDayExposures
                    .Select(c => c.SimulatedIndividual.Id)
                    .ToHashSet();

            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposures = collection.ExternalIndividualDayExposures
                        .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                        .Select(id => (
                            Exposure: id.GetExposure(route, relativePotencyFactors, membershipProbabilities, isPerPerson),
                            SamplingWeight: id.SimulatedIndividual.SamplingWeight
                        ))
                        .ToList();
                    if (exposures.Any(c => c.Exposure > 0)) {
                        var record = new ExternalContributionBySourceRouteRecord {
                            ExposureSource = collection.ExposureSource.GetShortDisplayName(),
                            ExposureRoute = route.GetShortDisplayName(),
                            Contribution = exposures.Sum(c => c.Exposure * c.SamplingWeight),
                        };
                        result.Add(record);
                    }
                }
            };
            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans
                    .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                    .Select(id => (
                        Exposure: id.DietaryIntakePerMassUnit,
                        SamplingWeight: id.SimulatedIndividual.SamplingWeight
                    )).ToList();
                var dietaryRecord = new ExternalContributionBySourceRouteRecord {
                    ExposureRoute = ExposureRoute.Oral.GetShortDisplayName(),
                    ExposureSource = ExposureSource.Diet.GetShortDisplayName(),
                    Contribution = oims.Sum(c => c.Exposure * c.SamplingWeight)
                };
                result.Add(dietaryRecord);
            }

            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return result.OrderByDescending(c => c.Contribution).ToList();
        }

        protected void UpdateContributions(List<ExternalContributionBySourceRouteRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureSource == record.ExposureSource && c.ExposureRoute == record.ExposureRoute)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
