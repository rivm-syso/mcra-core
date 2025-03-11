using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ContributionBySourceSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ContributionBySourceRecord> Records { get; set; }

        protected List<ContributionBySourceRecord> SummarizeContributions(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            List<(double SamplingWeight, double Exposure, int SimulatedIndividualId, ExposureSource Source)> observedIndividualMeans,
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
            var results = new List<(double SamplingWeight, double Exposure, int SimulatedIndividualId, ExposureSource Source)>();
            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposuresPerSource = GetExposuresPerSource(
                            relativePotencyFactors,
                            membershipProbabilities,
                            kineticConversionFactors,
                            isPerPerson,
                            collection,
                            route
                        );
                    if (individualsIds != null) {
                        exposuresPerSource = exposuresPerSource.Where(c => individualsIds.Contains(c.SimulatedIndividualId))
                            .Select(c => c)
                            .ToList();
                    }
                    results.AddRange(exposuresPerSource);
                }
            }

            if (observedIndividualMeans != null) {
                var oims = individualsIds != null
                    ? observedIndividualMeans.Where(c => individualsIds.Contains(c.SimulatedIndividualId))
                        .Select(c => c)
                        .ToList()
                    : observedIndividualMeans;
                results.AddRange(oims);
            }

            var result = results.GroupBy(c => c.Source)
                .Select(gr => getContributionBySourceRecord(
                    gr.Key,
                    gr.GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => (
                            SamplingWeight: c.First().SamplingWeight,
                            Exposure: c.Sum(r => r.Exposure)
                        )).ToList(),
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                )).ToList();

            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        private static ContributionBySourceRecord getContributionBySourceRecord(
            ExposureSource source,
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
            ICollection<ExternalExposureCollection> externalExposureCollections,
            List<(double SamplingWeight, double Exposure, int SimulatedIndividualId, ExposureSource Source)> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            HashSet<int> individualsIds,
            bool isPerPerson
        ) {
            var results = new List<(double SamplingWeight, double Exposure, int SimulatedIndividualId, ExposureSource Source)>();
            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposuresPerSource = GetExposuresPerSource(
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        isPerPerson,
                        collection,
                        route
                    );
                    if (individualsIds != null) {
                        exposuresPerSource = exposuresPerSource.Where(c => individualsIds.Contains(c.SimulatedIndividualId))
                            .Select(c => c)
                            .ToList();
                    }
                    results.AddRange(exposuresPerSource);
                }
            }
            if (observedIndividualMeans != null) {
                var oims = individualsIds != null
                    ? observedIndividualMeans.Where(c => individualsIds.Contains(c.SimulatedIndividualId))
                        .Select(c => c)
                        .ToList()
                    : observedIndividualMeans;
                results.AddRange(oims);
            }

            var result = results.GroupBy(c => c.Source)
                .Select(gr => new ContributionBySourceRecord {
                    ExposureSource = gr.Key.GetShortDisplayName(),
                    Contribution = gr.Sum(r => r.Exposure * r.SamplingWeight)
                }).ToList();

            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
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

        protected static List<(double SamplingWeight, double Exposure, int SimulatedIndividualId, ExposureSource Source)> GetInternalObservedIndividualMeans(
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
                            / (isPerPerson ? 1 : c.Individual.BodyWeight)
                            ),
                    SamplingWeight: c.IndividualSamplingWeight,
                    SimulatedIndividualId: c.SimulatedIndividualId,
                    NumberOfDays: c.Individual.NumberOfDaysInSurvey)
                    )
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => (
                    SamplingWeight: c.First().SamplingWeight,
                    Exposure: c.Sum(r => r.Exposure) / c.First().NumberOfDays,
                    SimulatedIndividualId: c.Key,
                    Source: ExposureSource.Diet
                ))
                .ToList();
        }

        protected static List<(double SamplingWeight, double Exposure, int SimulatedIndividualId, ExposureSource Source)> GetExposuresPerSource(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson,
            ExternalExposureCollection collection,
            ExposureRoute route
        ) {
            return collection.ExternalIndividualDayExposures
                .Select(id => (
                    SamplingWeight: id.IndividualSamplingWeight,
                    Exposure: id.GetTotalRouteExposure(
                        route,
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        isPerPerson
                    ),
                    SimulatedIndividualId: id.SimulatedIndividualId,
                    Source: collection.ExposureSource
                ))
                .ToList();
        }
    }
}
