using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class NonDietaryDistributionRouteSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public double _lowerPercentage;
        public double _upperPercentage;
        public List<NonDietaryDistributionRouteRecord> SummarizeAcute(
                ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                ICollection<ExposureRouteType> nonDietaryExposureRoutes,
                bool isPerPerson
            ) {
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes.Sum(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);

            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var result = nonDietaryExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = nonDietaryIndividualDayIntakes
                    .Select(idi => (
                        SamplingWeight: idi.IndividualSamplingWeight,
                        IntakePerMassUnit: idi.GetTotalIntakesPerRouteSubstance()
                            .Where(c => c.Route == route)
                            .Sum(c => c.Intake(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.Individual.BodyWeight)
                    ))
                    .ToList();

                    var weights = exposuresPerRoute.Where(c => c.IntakePerMassUnit > 0)
                        .Select(idi => idi.SamplingWeight).ToList();
                    var percentiles = exposuresPerRoute.Where(c => c.IntakePerMassUnit > 0)
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, percentages);

                    var weightsAll = exposuresPerRoute.Select(idi => idi.SamplingWeight).ToList();
                    var percentilesAll = exposuresPerRoute
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weightsAll, percentages);

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
                        Contributions = new List<double>()
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
            ICollection<ExposureRouteType> nonDietaryExposureRoutes,
            bool isPerPerson
        ) {
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes
                .GroupBy(r => r.SimulatedIndividualId)
                .Sum(c => c.Sum(r => r.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().IndividualSamplingWeight / c.Count());
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var result = nonDietaryExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = nonDietaryIndividualDayIntakes
                        .GroupBy(gr => gr.SimulatedIndividualId)
                        .Select(idi => (
                            SamplingWeight: idi.First().IndividualSamplingWeight,
                            IntakePerMassUnit: idi.First().GetTotalIntakesPerRouteSubstance()
                                .Where(c => c.Route == route)
                                .Sum(c => c.Intake(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.First().Individual.BodyWeight)
                        ))
                        .ToList();

                    var weights = exposuresPerRoute.Where(c => c.IntakePerMassUnit > 0)
                        .Select(idi => idi.SamplingWeight).ToList();
                    var percentiles = exposuresPerRoute.Where(c => c.IntakePerMassUnit > 0)
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, percentages);

                    var weightsAll = exposuresPerRoute.Select(idi => idi.SamplingWeight).ToList();
                    var percentilesAll = exposuresPerRoute
                        .Select(c => c.IntakePerMassUnit)
                        .PercentilesWithSamplingWeights(weightsAll, percentages);

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
                        Contributions = new List<double>()
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
               ICollection<ExposureRouteType> nonDietaryExposureRoutes,
               bool isPerPerson
           ) {
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes.Sum(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var result = nonDietaryExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = nonDietaryIndividualDayIntakes
                    .Select(idi => (
                        SamplingWeight: idi.IndividualSamplingWeight,
                        IntakePerMassUnit: idi.GetTotalIntakesPerRouteSubstance()
                            .Where(c => c.Route == route)
                            .Sum(c => c.Intake(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.Individual.BodyWeight)
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
            ICollection<ExposureRouteType> nonDietaryExposureRoutes,
            bool isPerPerson
        ) {
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var totalNonDietaryIntake = nonDietaryIndividualDayIntakes
                .GroupBy(gr => gr.SimulatedIndividualId)
                .Sum(c => c.Sum(r => r.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().IndividualSamplingWeight / c.Count());
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var result = nonDietaryExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                var exposuresPerRoute = nonDietaryIndividualDayIntakes
                    .GroupBy(gr => gr.SimulatedIndividualId)
                    .Select(idi => (
                        SamplingWeight: idi.First().IndividualSamplingWeight,
                        IntakePerMassUnit: idi.First().GetTotalIntakesPerRouteSubstance()
                            .Where(c => c.Route == route)
                            .Sum(c => c.Intake(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.First().Individual.BodyWeight)
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
