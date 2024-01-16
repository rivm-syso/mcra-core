using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionRouteCompoundSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public double _lowerPercentage;
        public double _upperPercentage;

        public List<DistributionRouteCompoundRecord> Summarize(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType RouteType, Compound Substance), double> absorptionFactors,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalIntakes = relativePotencyFactors != null ?
                aggregateIndividualDayExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var distributionRouteCompoundRecords = new List<DistributionRouteCompoundRecord>();
            var exposureRoutes = absorptionFactors.Select(c => c.Key.RouteType).Distinct().ToList();
            //writeToCsv(aggregateIndividualDayExposures, selectedCompounds, exposureRoutes);
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            foreach (var route in exposureRoutes) {
                foreach (var substance in selectedSubstances) {
                    var exposures = aggregateIndividualDayExposures
                       .AsParallel()
                       .WithCancellation(cancelToken)
                       .Select(idi => (
                           SamplingWeight: idi.IndividualSamplingWeight,
                           IntakePerMassUnit: idi.ExposuresPerRouteSubstance[route]
                                .Where(r => r.Compound == substance)
                                .Sum(r => r.Intake(1d, membershipProbabilities[r.Compound]) * absorptionFactors[(route, r.Compound)]) / (isPerPerson ? 1 : idi.CompartmentWeight)
                       ))
                       .ToList();

                    var allWeights = exposures.Select(a => a.SamplingWeight).ToList();
                    var percentilesAll = exposures.Select(a => a.IntakePerMassUnit).PercentilesWithSamplingWeights(allWeights, percentages);
                    var weights = exposures.Where(a => a.IntakePerMassUnit > 0).Select(a => a.SamplingWeight).ToList();
                    var percentiles = exposures.Where(a => a.IntakePerMassUnit > 0).Select(a => a.IntakePerMassUnit).PercentilesWithSamplingWeights(weights, percentages);
                    var total = exposures.Sum(a => a.IntakePerMassUnit * a.SamplingWeight);
                    var record = new DistributionRouteCompoundRecord {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = total * (relativePotencyFactors?[substance] ?? double.NaN) / totalIntakes,
                        Percentage = weights.Count / (double)aggregateIndividualDayExposures.Count * 100,
                        Mean = total / weights.Sum(),
                        Percentile25 = percentiles[0],
                        Median = percentiles[1],
                        Percentile75 = percentiles[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                        AssessmentGroupMembership = membershipProbabilities?[substance] ?? double.NaN,
                        AbsorptionFactor = absorptionFactors.TryGetValue((route, substance), out var factor) ? factor : double.NaN,
                        N = weights.Count,
                        Contributions = new List<double>(),
                    };
                    distributionRouteCompoundRecords.Add(record);
                }
            }
            distributionRouteCompoundRecords = distributionRouteCompoundRecords.OrderByDescending(r => r.Contribution).ToList();
            var rescale = distributionRouteCompoundRecords.Sum(c => c.Contribution);
            distributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            distributionRouteCompoundRecords.TrimExcess();
            return distributionRouteCompoundRecords;
        }

        public List<DistributionRouteCompoundRecord> Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType RouteType, Compound), double> absorptionFactors,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var distributionRouteCompoundRecords = new List<DistributionRouteCompoundRecord>();
            var totalIntakes = relativePotencyFactors != null ?
                aggregateIndividualExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var exposureRoutes = absorptionFactors.Select(c => c.Key.RouteType).Distinct().ToList();
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            foreach (var route in exposureRoutes) {
                foreach (var substance in selectedSubstances) {
                    var exposures = aggregateIndividualExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            IntakePerMassUnit: c.ExposuresPerRouteSubstance[route]
                                .Where(r => r.Compound == substance)
                                .Sum(r => r.Intake(1d, membershipProbabilities[r.Compound]) * absorptionFactors[(route, r.Compound)]) / (isPerPerson ? 1 : c.CompartmentWeight)
                        ))
                        .ToList();

                    var allWeights = exposures.Select(c => c.SamplingWeight).ToList();
                    var percentilesAll = exposures.Select(c => c.IntakePerMassUnit).PercentilesWithSamplingWeights(allWeights, percentages);
                    var weights = exposures.Where(c => c.IntakePerMassUnit > 0).Select(c => c.SamplingWeight).ToList();
                    var percentiles = exposures.Where(c => c.IntakePerMassUnit > 0).Select(c => c.IntakePerMassUnit).PercentilesWithSamplingWeights(weights, percentages);
                    var total = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight);
                    var record = new DistributionRouteCompoundRecord {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = total * (relativePotencyFactors?[substance] ?? double.NaN) / totalIntakes,
                        Percentage = weights.Count / (double)aggregateIndividualExposures.Count * 100,
                        Mean = total / weights.Sum(),
                        Percentile25 = percentiles[0],
                        Median = percentiles[1],
                        Percentile75 = percentiles[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                        AssessmentGroupMembership = membershipProbabilities?[substance] ?? double.NaN,
                        AbsorptionFactor = absorptionFactors.TryGetValue((route, substance), out var factor) ? factor : double.NaN,
                        N = weights.Count,
                        Contributions = new List<double>(),
                    };
                    distributionRouteCompoundRecords.Add(record);
                }
            }
            distributionRouteCompoundRecords = distributionRouteCompoundRecords.OrderByDescending(r => r.Contribution).ToList();
            var rescale = distributionRouteCompoundRecords.Sum(c => c.Contribution);
            distributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            distributionRouteCompoundRecords.TrimExcess();
            return distributionRouteCompoundRecords;
        }


        public List<DistributionRouteCompoundRecord> SummarizeUncertainty(
              ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
              ICollection<Compound> selectedSubstances,
              IDictionary<Compound, double> relativePotencyFactors,
              IDictionary<Compound, double> membershipProbabilities,
              IDictionary<(ExposurePathType RouteType, Compound), double> absorptionFactors,
              bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalIntakes = relativePotencyFactors != null ?
                aggregateIndividualDayExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var distributionRouteCompoundRecords = new List<DistributionRouteCompoundRecord>();
            var exposureRoutes = absorptionFactors.Select(c => c.Key.RouteType).Distinct().ToList();
            //writeToCsv(aggregateIndividualDayExposures, selectedCompounds, exposureRoutes);
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            foreach (var route in exposureRoutes) {
                foreach (var substance in selectedSubstances) {
                    var exposures = aggregateIndividualDayExposures
                       .AsParallel()
                       .WithCancellation(cancelToken)
                       .Select(idi => (
                           SamplingWeight: idi.IndividualSamplingWeight,
                           IntakePerMassUnit: idi.ExposuresPerRouteSubstance[route]
                                .Where(r => r.Compound == substance)
                                .Sum(r => r.Intake(1d, membershipProbabilities[r.Compound]) * absorptionFactors[(route, r.Compound)]) / (isPerPerson ? 1 : idi.CompartmentWeight)
                       ))
                       .ToList();

                    var record = new DistributionRouteCompoundRecord {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = exposures.Sum(a => a.IntakePerMassUnit * a.SamplingWeight) * (relativePotencyFactors?[substance] ?? double.NaN) / totalIntakes,
                    };
                    distributionRouteCompoundRecords.Add(record);
                }
            }
            distributionRouteCompoundRecords = distributionRouteCompoundRecords.OrderByDescending(r => r.Contribution).ToList();
            var rescale = distributionRouteCompoundRecords.Sum(c => c.Contribution);
            distributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            distributionRouteCompoundRecords.TrimExcess();
            return distributionRouteCompoundRecords;
        }

        public List<DistributionRouteCompoundRecord> SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType RouteType, Compound), double> absorptionFactors,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var distributionRouteCompoundRecords = new List<DistributionRouteCompoundRecord>();
            var totalIntakes = relativePotencyFactors != null ?
                aggregateIndividualExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var exposureRoutes = absorptionFactors.Select(c => c.Key.RouteType).Distinct().ToList();
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            foreach (var route in exposureRoutes) {
                foreach (var substance in selectedSubstances) {
                    var exposures = aggregateIndividualExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            IntakePerMassUnit: c.ExposuresPerRouteSubstance[route]
                                .Where(r => r.Compound == substance)
                                .Sum(r => r.Intake(1d, membershipProbabilities[r.Compound]) * absorptionFactors[(route, r.Compound)]) / (isPerPerson ? 1 : c.CompartmentWeight)
                        ))
                        .ToList();

                    var record = new DistributionRouteCompoundRecord {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = exposures.Sum(c => c.IntakePerMassUnit * c.SamplingWeight) * (relativePotencyFactors?[substance] ?? double.NaN) / totalIntakes,
                    };
                    distributionRouteCompoundRecords.Add(record);
                }
            }
            distributionRouteCompoundRecords = distributionRouteCompoundRecords.OrderByDescending(r => r.Contribution).ToList();
            var rescale = distributionRouteCompoundRecords.Sum(c => c.Contribution);
            distributionRouteCompoundRecords.ForEach(c => c.Contribution = c.Contribution / rescale);
            distributionRouteCompoundRecords.TrimExcess();
            return distributionRouteCompoundRecords;
        }
        protected void setPercentages(double lowerPercentage, double upperPercentage) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
        }
    }
}
