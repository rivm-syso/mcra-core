using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Constants;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureByRouteSection : ExposureByRouteSectionBase {
        public override bool SaveTemporaryData => true;

        private static readonly double _upperWhisker = 95;

        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureByRouteRecord> Records { get; set; }
        public List<ExposureByRoutePercentileRecord> BoxPlotRecords { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute route, Compound substance), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            TargetUnit targetUnit,
            bool isPerPerson,
            bool skipPrivacySensitiveOutputs
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var routes = kineticConversionFactors.Select(c => c.Key.route).Distinct().ToList();

            relativePotencyFactors = substances.Count > 1
                ? relativePotencyFactors : substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = substances.Count > 1
                ? membershipProbabilities : substances.ToDictionary(r => r, r => 1D);

            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalIndividualExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            TargetUnit = targetUnit;

            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            Records = summarizeExposureRecords(
                exposureCollection,
                percentages
            );

            BoxPlotRecords = summarizeBoxPlotsByRoute(
                exposureCollection,
                targetUnit
            );
        }

        private static List<ExposureByRouteRecord> summarizeExposureRecords(
            List<(ExposureRoute ExposureRoute, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureRouteCollection,
            double[] percentages
        ) {
            var records = new List<ExposureByRouteRecord>();
            foreach (var item in exposureRouteCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var record = getExposureRouteRecord(
                        item.ExposureRoute,
                        item.Exposures,
                        percentages
                    );
                    records.Add(record);
                }
            }
            return records;
        }

        private List<ExposureByRoutePercentileRecord> summarizeBoxPlotsByRoute(
            List<(ExposureRoute ExposureRoute, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureRouteCollection,
            TargetUnit targetUnit
        ) {
            var records = new List<ExposureByRoutePercentileRecord>();
            foreach (var item in exposureRouteCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var boxPlotRecord = getBoxPlotRecord(
                        item.ExposureRoute,
                        item.Exposures,
                        targetUnit
                    );
                    records.Add(boxPlotRecord);
                }
            }
            return records;
        }
        private static ExposureByRouteRecord getExposureRouteRecord(
            ExposureRoute route,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double[] percentages
        ) {
            var weights = exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentiles = exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);
            var weightsAll = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var total = exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new ExposureByRouteRecord {
                ExposureRoute = route.GetShortDisplayName(),
                Percentage = weights.Count / (double)exposures.Count * 100,
                MeanAll = total / weightsAll.Sum(),
                Mean = total / weights.Sum(),
                Percentile25 = percentiles[0],
                Median = percentiles[1],
                Percentile75 = percentiles[2],
                Percentile25All = percentilesAll[0],
                MedianAll = percentilesAll[1],
                Percentile75All = percentilesAll[2],
                NumberOfDays = weights.Count,
            };
            return record;
        }

        private static ExposureByRoutePercentileRecord getBoxPlotRecord(
            ExposureRoute route,
            List<(SimulatedIndividual SimulatedIndividual, double exposure)> exposures,
            TargetUnit targetUnit
        ) {
            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.exposure)
                .ToList();
            var percentiles = allExposures
                .PercentilesWithSamplingWeights(weights, _percentages)
                .ToList();
            var positives = allExposures
                .Where(r => r > 0)
                .ToList();
            var outliers = allExposures
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c)
                .ToList();

            var record = new ExposureByRoutePercentileRecord() {
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
