using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureByRouteSubstanceSection : SummarySection {
        public override bool SaveTemporaryData => true;

        private static readonly double[] _boxPlotPercentages = [5, 10, 25, 50, 75, 90, 95];
        private static readonly double _upperWhisker = 95;

        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureByRouteSubstanceRecord> ExposureRecords { get; set; }
        public List<ExposureByRouteSubstancePercentileRecord> ExposureBoxPlotRecords { get; set; }
        public TargetUnit TargetUnit { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            ExposureUnitTriple externalExposureUnit,
            bool skipPrivacySensitiveOutputs
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();

            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();

            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(aggregateExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }

            ShowOutliers = !skipPrivacySensitiveOutputs;
            TargetUnit = targetUnit;

            // Exposures of route and substance are calculated using the absorption
            // factors and the external exposures.
            ExposureRecords = summarizeExposureRecords(
                aggregateExposures,
                substances,
                routes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit,
                percentages
            );

            ExposureBoxPlotRecords = summarizeBoxPlotRecords(
                aggregateExposures,
                substances,
                routes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit,
                targetUnit
            );
        }

        private static List<ExposureByRouteSubstanceRecord> summarizeExposureRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            double[] percentages
        ) {
            var result = new List<ExposureByRouteSubstanceRecord>();
            foreach (var route in routes) {
                foreach (var substance in substances) {
                    var exposures = aggregateExposures
                        .OrderBy(r => r.SimulatedIndividual.Id)
                        .Select(c => (
                            SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                            Exposure: c
                                .GetTotalRouteExposureForSubstance(
                                    route,
                                    substance,
                                    externalExposureUnit.IsPerUnit()
                                ) * kineticConversionFactors[(route, substance)]
                        ))
                        .ToList();

                    var weightsAll = exposures
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var percentilesAll = exposures
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weightsAll, percentages);

                    var weightsPositives = exposures
                        .Where(c => c.Exposure > 0)
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var percentilesPositives = exposures
                        .Where(c => c.Exposure > 0)
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weightsPositives, percentages);

                    var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);

                    var record = new ExposureByRouteSubstanceRecord {
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        ExposureRoute = route.GetShortDisplayName(),
                        PercentagePositives = weightsPositives.Count / (double)aggregateExposures.Count * 100,
                        MeanAll = total / weightsAll.Sum(),
                        MeanPositives = total / weightsPositives.Sum(),
                        Percentile25Positives = percentilesPositives[0],
                        MedianPositives = percentilesPositives[1],
                        Percentile75Positives = percentilesPositives[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                        AssessmentGroupMembership = membershipProbabilities?[substance] ?? double.NaN,
                        NumberOfIndividuals = weightsPositives.Count,
                    };
                    result.Add(record);
                }
            }

            return result;
        }

        private List<ExposureByRouteSubstancePercentileRecord> summarizeBoxPlotRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            var result = new List<ExposureByRouteSubstancePercentileRecord>();
            var cancelToken = ProgressState?.CancellationToken ?? new();
            foreach (var route in routes) {
                var boxPlotRecords = new List<ExposureByRouteSubstancePercentileRecord>();
                foreach (var substance in substances) {
                    var exposures = aggregateExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(idi => (
                            SamplingWeight: idi.SimulatedIndividual.SamplingWeight,
                            Exposure: idi.GetTotalRouteExposureForSubstance(
                                route,
                                substance,
                                externalExposureUnit.IsPerUnit()
                            ) * kineticConversionFactors[(route, substance)]
                        ))
                        .ToList();
                    if (exposures.Any(c => c.Exposure > 0)) {
                        var boxPlotRecord = getBoxPlotRecord(
                            substance,
                            route,
                            exposures,
                            targetUnit
                        );
                        result.Add(boxPlotRecord);
                    }
                }
            }
            return result;
        }

        private ExposureByRouteSubstancePercentileRecord getBoxPlotRecord(
            Compound substance,
            ExposureRoute route,
            List<(double samplingWeight, double exposure)> exposures,
            TargetUnit targetUnit
        ) {
            var weights = exposures
                .Select(c => c.samplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.exposure)
                .ToList();
            var percentiles = allExposures
                .PercentilesWithSamplingWeights(weights, _boxPlotPercentages)
                .ToList();
            var positives = allExposures
                .Where(r => r > 0)
                .ToList();
            var outliers = allExposures
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c)
                .ToList();

            var record = new ExposureByRouteSubstancePercentileRecord() {
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                ExposureRoute = route != ExposureRoute.Undefined
                    ? route.GetDisplayName() : null,
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return record;
        }
    }
}
