using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {
    public class ExternalExposureDistributionRouteSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        protected double[] Percentages;

        public List<HistogramBin> IntakeDistributionBins { get; set; }
        public List<HistogramBin> IntakeDistributionBinsCoExposure { get; set; }
        public UncertainDataPointCollection<double> Percentiles { get; set; }
        public int TotalNumberOfExposures { get; set; }
        public double PercentageZeroIntake { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }

        protected readonly double _upperWhisker = 95;
        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public List<ExternalExposureBySourceRouteRecord> ExposureRecords { get; set; }
        public List<ExternalContributionBySourceRouteRecord> ContributionRecords { get; set; }

        public double? RestrictedUpperPercentile { get; set; }
        public List<ExternalExposureBySourceRoutePercentileRecord> ExposureBoxPlotRecords { get; set; } = [];

        public TargetUnit TargetUnit { get; set; }



        public List<ExternalExposureDistributionRouteRecord> SummarizeAcute(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureRoutes,
            bool isPerPerson
        ) {
            var totalExternalExposure = externalIndividualDayExposures
                .Sum(c => c.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)
                    * c.SimulatedIndividual.SamplingWeight);
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = externalExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = externalIndividualDayExposures
                    .Select(idi => (
                        SamplingWeight: idi.SimulatedIndividual.SamplingWeight,
                        ExposurePerMassUnit: idi.GetExposuresBySubstance(route)
                            .Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.SimulatedIndividual.BodyWeight)
                    ))
                    .ToList();

                    var weights = exposuresPerRoute
                        .Where(c => c.ExposurePerMassUnit > 0)
                        .Select(idi => idi.SamplingWeight)
                        .ToList();
                    var percentiles = exposuresPerRoute
                        .Where(c => c.ExposurePerMassUnit > 0)
                        .Select(c => c.ExposurePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, Percentages);

                    var weightsAll = exposuresPerRoute.Select(idi => idi.SamplingWeight).ToList();
                    var percentilesAll = exposuresPerRoute
                        .Select(c => c.ExposurePerMassUnit)
                        .PercentilesWithSamplingWeights(weightsAll, Percentages);

                    return new ExternalExposureDistributionRouteRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = exposuresPerRoute.Sum(c => c.ExposurePerMassUnit * c.SamplingWeight) / totalExternalExposure,
                        Percentage = weights.Count / (double)exposuresPerRoute.Count * 100,
                        Mean = exposuresPerRoute.Sum(c => c.ExposurePerMassUnit * c.SamplingWeight) / weights.Sum(),
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

        public List<ExternalExposureDistributionRouteRecord> SummarizeChronic(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureRoutes,
            bool isPerPerson
        ) {
            var totalExternalExposure = externalIndividualDayExposures
                .GroupBy(r => r.SimulatedIndividual.Id)
                .Sum(c => c.Sum(r => r.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)) * c.First().SimulatedIndividual.SamplingWeight / c.Count());
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = externalExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = externalIndividualDayExposures
                        .GroupBy(d => d.SimulatedIndividual)
                        .Select(g => (
                            SamplingWeight: g.Key.SamplingWeight,
                            ExposurePerMassUnit: g.First().GetExposuresBySubstance(route)
                                .Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : g.Key.BodyWeight)
                        ))
                        .ToList();

                    var weights = exposuresPerRoute.Where(c => c.ExposurePerMassUnit > 0)
                        .Select(idi => idi.SamplingWeight).ToList();
                    var percentiles = exposuresPerRoute.Where(c => c.ExposurePerMassUnit > 0)
                        .Select(c => c.ExposurePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, Percentages);

                    var weightsAll = exposuresPerRoute.Select(idi => idi.SamplingWeight).ToList();
                    var percentilesAll = exposuresPerRoute
                        .Select(c => c.ExposurePerMassUnit)
                        .PercentilesWithSamplingWeights(weightsAll, Percentages);

                    return new ExternalExposureDistributionRouteRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = exposuresPerRoute.Sum(c => c.ExposurePerMassUnit * c.SamplingWeight) / totalExternalExposure,
                        Percentage = weights.Count / (double)exposuresPerRoute.Count * 100,
                        Mean = exposuresPerRoute.Sum(c => c.ExposurePerMassUnit * c.SamplingWeight) / weights.Sum(),
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

        public List<ExternalExposureDistributionRouteRecord> SummarizeAcuteUncertainty(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureRoutes,
            bool isPerPerson
        ) {
            var totalNonDietaryIntake = externalIndividualDayExposures
                .Sum(c => c.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson) 
                    * c.SimulatedIndividual.SamplingWeight);
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = externalExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = externalIndividualDayExposures
                        .Select(idi => (
                            SamplingWeight: idi.SimulatedIndividual.SamplingWeight,
                            IntakePerMassUnit: idi.GetExposuresBySubstance(route)
                                .Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : idi.SimulatedIndividual.BodyWeight)
                        ))
                        .ToList();
                    return new ExternalExposureDistributionRouteRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = exposuresPerRoute.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
                    };
                })
                .ToList();
            return result;
        }

        public List<ExternalExposureDistributionRouteRecord> SummarizeChronicUncertainty(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureRoutes,
            bool isPerPerson
        ) {
            var totalNonDietaryIntake = externalIndividualDayExposures
                .GroupBy(gr => gr.SimulatedIndividual.Id)
                .Sum(c => c.Sum(r => r.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                    * c.First().SimulatedIndividual.SamplingWeight / c.Count());
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = externalExposureRoutes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(route => {
                    var exposuresPerRoute = externalIndividualDayExposures
                        .GroupBy(d => d.SimulatedIndividual)
                        .Select(g => (
                            SamplingWeight: g.Key.SamplingWeight,
                            IntakePerMassUnit: g.First().GetExposuresBySubstance(route)
                                .Sum(c => c.EquivalentSubstanceAmount(relativePotencyFactors[c.Compound], membershipProbabilities[c.Compound])) / (isPerPerson ? 1 : g.Key.BodyWeight)
                        ))
                        .ToList();
                    return new ExternalExposureDistributionRouteRecord {
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = exposuresPerRoute.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) / totalNonDietaryIntake,
                    };
                })
                .ToList();
            return result;
        }
    }
}
