using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ContributionBySourceRouteSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ContributionBySourceRouteRecord> Records { get; set; }

        protected List<ContributionBySourceRouteRecord> summarizeContributions(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            List<(double SamplingWeight, double Exposure, int SimulatedIndividualId)> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            HashSet<int> individualsIds,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRouteRecord>();
            var ids = individualsIds ?? externalExposureCollections.First().ExternalIndividualDayExposures.Select(c => c.SimulatedIndividual.Id).ToHashSet();
            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposures = collection.ExternalIndividualDayExposures
                        .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                        .Select(id => (
                            SamplingWeight: id.SimulatedIndividual.SamplingWeight,
                            Exposure: id.GetTotalRouteExposure(
                                route,
                                relativePotencyFactors,
                                membershipProbabilities,
                                kineticConversionFactors,
                                isPerPerson
                            )
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
                    .Where(c => ids.Contains(c.SimulatedIndividualId))
                    .Select(c => (
                        SamplingWeight: c.SamplingWeight,
                        Exposure: c.Exposure
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

        private static ContributionBySourceRouteRecord getContributionBySourceRouteRecord(
            ExposureSource source,
            ExposureRoute route,
            List<(double SamplingWeight, double Exposure)> exposures,
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
            var record = new ContributionBySourceRouteRecord {
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

        protected List<ContributionBySourceRouteRecord> SummarizeUncertainty(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            List<(double SamplingWeight, double Exposure, int SimulatedIndividualId)> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            HashSet<int> individualsIds,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRouteRecord>();
            var ids = individualsIds ?? externalExposureCollections.First().ExternalIndividualDayExposures.Select(c => c.SimulatedIndividual.Id).ToHashSet();
            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposures = collection.ExternalIndividualDayExposures
                        .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                        .Select(id => (
                            Exposure: id.GetTotalRouteExposure(
                                route,
                                relativePotencyFactors,
                                membershipProbabilities,
                                kineticConversionFactors,
                                isPerPerson),
                            SamplingWeight: id.SimulatedIndividual.SamplingWeight
                        ))
                        .ToList();
                    if (exposures.Any(c => c.Exposure > 0)) {
                        var record = new ContributionBySourceRouteRecord {
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
                    .Where(c => ids.Contains(c.SimulatedIndividualId))
                    .Select(id => (
                        Exposure: id.Exposure,
                        SamplingWeight: id.SamplingWeight
                    )).ToList();
                var dietaryRecord = new ContributionBySourceRouteRecord {
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

        protected void UpdateContributions(List<ContributionBySourceRouteRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureSource == record.ExposureSource && c.ExposureRoute == record.ExposureRoute)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }

        protected static List<(double SamplingWeight, double Exposure, int SimulatedIndividualId)> GetInternalObservedIndividualMeans(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors, 
            bool isPerPerson
        ) {
            return dietaryIndividualDayIntakes
                .Select(c => (
                    Exposure: c.GetDietaryIntakesPerSubstance()
                        .Sum(ipc => ipc.Amount
                            * kineticConversionFactors[(ExposureRoute.Oral, ipc.Compound)]
                            * relativePotencyFactors[ipc.Compound]
                            * membershipProbabilities[ipc.Compound]
                            / (isPerPerson ? 1 : c.SimulatedIndividual.BodyWeight)
                            ),
                    SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                    SimulatedIndividualId: c.SimulatedIndividual.Id,
                    NumberOfDays: c.SimulatedIndividual.NumberOfDaysInSurvey)
                    )
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => (
                    SamplingWeight: c.First().SamplingWeight,
                    Exposure: c.Sum(r => r.Exposure) / c.First().NumberOfDays,
                    SimulatedIndividualId: c.Key
                ))
                .ToList();
        }
    }
}
