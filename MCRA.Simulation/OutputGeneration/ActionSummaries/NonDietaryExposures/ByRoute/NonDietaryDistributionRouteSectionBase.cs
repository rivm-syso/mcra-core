using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class NonDietaryDistributionRouteSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        protected double [] Percentages {  get; set; }
        public List<NonDietaryDistributionRouteRecord> SummarizeAcute(
                ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                ICollection<ExposureRoute> nonDietaryExposureRoutes,
                bool isPerPerson
            ) {
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes.Sum(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.SimulatedIndividual.SamplingWeight);
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = nonDietaryExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = nonDietaryIndividualDayIntakes
                    .Select(idi => (
                        SamplingWeight: idi.SimulatedIndividual.SamplingWeight,
                        IntakePerMassUnit: idi.GetTotalIntakesPerRouteSubstance()
                            .Where(c => c.Route == route)
                            .Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.SimulatedIndividual.BodyWeight)
                    ))
                    .ToList();

                    var weights = exposuresPerRoute.Where(c => c.IntakePerMassUnit > 0)
                        .Select(idi => idi.SamplingWeight).ToList();
                    var percentiles = exposuresPerRoute.Where(c => c.IntakePerMassUnit > 0)
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, Percentages);

                    var weightsAll = exposuresPerRoute.Select(idi => idi.SamplingWeight).ToList();
                    var percentilesAll = exposuresPerRoute
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weightsAll, Percentages);

                    return new NonDietaryDistributionRouteRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = exposuresPerRoute.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
                        Percentage = weights.Count / (double)exposuresPerRoute.Count * 100,
                        Mean = exposuresPerRoute.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / weights.Sum(),
                        Percentile25 = percentiles[0],
                        Median = percentiles[1],
                        Percentile75 = percentiles[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        NumberOfDays = weights.Count,
                        Contributions = []
                    };
                }).ToList();
            return result
                .Where(r => r.Mean > 0)
                .OrderBy(s => s.ExposureRoute, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(r => r.Contribution)
                .ToList();
        }

        public List<NonDietaryDistributionRouteRecord> SummarizeChronic(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> nonDietaryExposureRoutes,
            bool isPerPerson
        ) {
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes
                .GroupBy(r => r.SimulatedIndividual.Id)
                .Sum(c => c.Sum(r => r.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().SimulatedIndividual.SamplingWeight / c.Count());
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = nonDietaryExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = nonDietaryIndividualDayIntakes
                        .GroupBy(gr => gr.SimulatedIndividual.Id)
                        .Select(idi => (
                            SamplingWeight: idi.First().SimulatedIndividual.SamplingWeight,
                            IntakePerMassUnit: idi.First().GetTotalIntakesPerRouteSubstance()
                                .Where(c => c.Route == route)
                                .Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.First().SimulatedIndividual.BodyWeight)
                        ))
                        .ToList();

                    var weights = exposuresPerRoute.Where(c => c.IntakePerMassUnit > 0)
                        .Select(idi => idi.SamplingWeight).ToList();
                    var percentiles = exposuresPerRoute.Where(c => c.IntakePerMassUnit > 0)
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, Percentages);

                    var weightsAll = exposuresPerRoute.Select(idi => idi.SamplingWeight).ToList();
                    var percentilesAll = exposuresPerRoute
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weightsAll, Percentages);

                    return new NonDietaryDistributionRouteRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = exposuresPerRoute.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
                        Percentage = weights.Count / (double)exposuresPerRoute.Count * 100,
                        Mean = exposuresPerRoute.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / weights.Sum(),
                        Percentile25 = percentiles[0],
                        Median = percentiles[1],
                        Percentile75 = percentiles[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        NumberOfDays = weights.Count,
                        Contributions = []
                    };
                }).ToList();
            return result
                .Where(r => r.Mean > 0)
                .OrderBy(s => s.ExposureRoute, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(r => r.Contribution)
                .ToList();
        }

        public List<NonDietaryDistributionRouteRecord> SummarizeAcuteUncertainty(
               ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
               IDictionary<Compound, double> relativePotencyFactors,
               IDictionary<Compound, double> membershipProbabilities,
               ICollection<ExposureRoute> nonDietaryExposureRoutes,
               bool isPerPerson
           ) {
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes.Sum(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.SimulatedIndividual.SamplingWeight);
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = nonDietaryExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = nonDietaryIndividualDayIntakes
                    .Select(idi => (
                        SamplingWeight: idi.SimulatedIndividual.SamplingWeight,
                        IntakePerMassUnit: idi.GetTotalIntakesPerRouteSubstance()
                            .Where(c => c.Route == route)
                            .Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.SimulatedIndividual.BodyWeight)
                    ))
                    .ToList();
                return new NonDietaryDistributionRouteRecord {
                    ExposureRoute = route.GetShortDisplayName(),
                    Contribution = exposuresPerRoute.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
                };
            }).ToList();
            return result;
        }

        public List<NonDietaryDistributionRouteRecord> SummarizeChronicUncertainty(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> nonDietaryExposureRoutes,
            bool isPerPerson
        ) {
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes
                .GroupBy(gr => gr.SimulatedIndividual.Id)
                .Sum(c => c.Sum(r => r.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().SimulatedIndividual.SamplingWeight / c.Count());
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = nonDietaryExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                var exposuresPerRoute = nonDietaryIndividualDayIntakes
                    .GroupBy(gr => gr.SimulatedIndividual.Id)
                    .Select(idi => (
                        SamplingWeight: idi.First().SimulatedIndividual.SamplingWeight,
                        IntakePerMassUnit: idi.First().GetTotalIntakesPerRouteSubstance()
                            .Where(c => c.Route == route)
                            .Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.First().SimulatedIndividual.BodyWeight)
                    ))
                    .ToList();
                return new NonDietaryDistributionRouteRecord {
                    ExposureRoute = route.GetShortDisplayName(),
                    Contribution = exposuresPerRoute.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
                };
            }).ToList();
            return result;
        }
    }
}
