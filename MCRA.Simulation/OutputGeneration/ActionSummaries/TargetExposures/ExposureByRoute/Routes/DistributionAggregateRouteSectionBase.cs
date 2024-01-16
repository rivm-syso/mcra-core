using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionAggregateRouteSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public double _lowerPercentage;
        public double _upperPercentage;
        protected List<AggregateDistributionExposureRouteTotalRecord> Summarize(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalIntake = aggregateIndividualDayExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var distributionExposureRouteRecords = new List<AggregateDistributionExposureRouteTotalRecord>();
            foreach (var route in exposureRoutes) {
                var exposures = aggregateIndividualDayExposures
                       .AsParallel()
                       .WithCancellation(cancelToken)
                       .Select(idi => (
                           SamplingWeight: idi.IndividualSamplingWeight,
                           IntakePerMassUnit: idi.ExposuresPerRouteSubstance[route].Sum(r => r.Intake(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]) * absorptionFactors[(route, r.Compound)]) / (isPerPerson ? 1 : idi.CompartmentWeight)
                       ))
                       .ToList();

                var weights = exposures
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentiles = exposures.Where(c => c.IntakePerMassUnit > 0)
                    .Select(ndidi => ndidi.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, percentages);
                var total = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                var weightsAll = exposures.Select(idi => idi.SamplingWeight).ToList();
                var percentilesAll = exposures
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weightsAll, percentages);
                var record = new AggregateDistributionExposureRouteTotalRecord {
                    ExposureRoute = route.GetShortDisplayName(),
                    Contribution = total / totalIntake,
                    Percentage = weights.Count / (double)aggregateIndividualDayExposures.Count * 100,
                    Mean = total / weights.Sum(),
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    NumberOfDays = weights.Count,
                    Contributions = new List<double>(),
                };
                distributionExposureRouteRecords.Add(record);
            };
            var rescale = distributionExposureRouteRecords.Sum(c => c.Contribution);
            distributionExposureRouteRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            distributionExposureRouteRecords.TrimExcess();
            return distributionExposureRouteRecords.OrderByDescending(c => c.Contribution).ToList();
        }

        protected List<AggregateDistributionExposureRouteTotalRecord> Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<ExposurePathType> exposureRoutes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
            bool isPerPerson
        ) {
            var totalIntake = aggregateIndividualExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var distributionExposureRouteRecords = new List<AggregateDistributionExposureRouteTotalRecord>();
            foreach (var exposureRoute in exposureRoutes) {
                var exposures = aggregateIndividualExposures
                    .Select(idi => (
                        SamplingWeight: idi.IndividualSamplingWeight,
                        SimulatedIndividualId: idi.SimulatedIndividualId,
                        IntakePerMassUnit: idi.ExposuresPerRouteSubstance[exposureRoute]
                            .Sum(r => r.Intake(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]) * absorptionFactors[(exposureRoute, r.Compound)]) / (isPerPerson ? 1 : idi.CompartmentWeight)
                    ))
                    .ToList();

                var weights = exposures
                    .Where(c => c.IntakePerMassUnit > 0)
                    .Select(idi => idi.SamplingWeight)
                    .ToList();
                var percentiles = exposures.Where(c => c.IntakePerMassUnit > 0)
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, percentages);
                var total = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                var weightsAll = exposures
                    .Select(c => c.SamplingWeight).ToList();
                var percentilesAll = exposures
                    .Select(c => c.IntakePerMassUnit)
                    .PercentilesWithSamplingWeights(weightsAll, percentages);
                var record = new AggregateDistributionExposureRouteTotalRecord {
                    ExposureRoute = exposureRoute.GetShortDisplayName(),
                    Contribution = total / totalIntake,
                    Mean = total / weights.Sum(),
                    Percentage = weights.Count / (double)aggregateIndividualExposures.Count * 100,
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    NumberOfDays = weights.Count,
                    Contributions = new List<double>(),
                };
                distributionExposureRouteRecords.Add(record);
            };
            var rescale = distributionExposureRouteRecords.Sum(c => c.Contribution);
            distributionExposureRouteRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            distributionExposureRouteRecords.TrimExcess();
            return distributionExposureRouteRecords.OrderByDescending(c => c.Contribution).ToList();
        }

        protected List<AggregateDistributionExposureRouteTotalRecord> SummarizeUncertainty(
           ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
           ICollection<ExposurePathType> exposureRoutes,
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
           bool isPerPerson
       ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalIntake = aggregateIndividualDayExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var distributionExposureRouteRecords = new List<AggregateDistributionExposureRouteTotalRecord>();
            foreach (var route in exposureRoutes) {
                var exposures = aggregateIndividualDayExposures
                       .AsParallel()
                       .WithCancellation(cancelToken)
                       .Select(idi => (
                           SamplingWeight: idi.IndividualSamplingWeight,
                           IntakePerMassUnit: idi.ExposuresPerRouteSubstance[route].Sum(r => r.Intake(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]) * absorptionFactors[(route, r.Compound)]) / (isPerPerson ? 1 : idi.CompartmentWeight)
                       ))
                       .ToList();

                var total = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                var record = new AggregateDistributionExposureRouteTotalRecord {
                    ExposureRoute = route.GetShortDisplayName(),
                    Contribution = total / totalIntake,
                };
                distributionExposureRouteRecords.Add(record);
            };
            var rescale = distributionExposureRouteRecords.Sum(c => c.Contribution);
            distributionExposureRouteRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            distributionExposureRouteRecords.TrimExcess();
            return distributionExposureRouteRecords.OrderByDescending(c => c.Contribution).ToList();
        }

        protected List<AggregateDistributionExposureRouteTotalRecord> SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<ExposurePathType> exposureRoutes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors,
            bool isPerPerson
        ) {
            var totalIntake = aggregateIndividualExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var distributionExposureRouteRecords = new List<AggregateDistributionExposureRouteTotalRecord>();
            foreach (var exposureRoute in exposureRoutes) {
                var exposures = aggregateIndividualExposures
                    .Select(idi => (
                        SamplingWeight: idi.IndividualSamplingWeight,
                        SimulatedIndividualId: idi.SimulatedIndividualId,
                        IntakePerMassUnit: idi.ExposuresPerRouteSubstance[exposureRoute].Sum(r => r.Intake(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]) * absorptionFactors[(exposureRoute, r.Compound)]) / (isPerPerson ? 1 : idi.CompartmentWeight)
                    ))
                    .ToList();

                var total = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                var record = new AggregateDistributionExposureRouteTotalRecord {
                    ExposureRoute = exposureRoute.GetShortDisplayName(),
                    Contribution = total / totalIntake,
                };
                distributionExposureRouteRecords.Add(record);
            };
            var rescale = distributionExposureRouteRecords.Sum(c => c.Contribution);
            distributionExposureRouteRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            distributionExposureRouteRecords.TrimExcess();
            return distributionExposureRouteRecords.OrderByDescending(c => c.Contribution).ToList();
        }
    }
}
