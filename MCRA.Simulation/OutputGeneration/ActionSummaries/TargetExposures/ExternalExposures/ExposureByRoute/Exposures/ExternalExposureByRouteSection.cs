﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureByRouteSection : ExternalExposureByRouteSectionBase {
        public override bool SaveTemporaryData => true;

        private static readonly double _upperWhisker = 95;

        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExternalExposureByRouteRecord> Records { get; set; }
        public List<ExternalExposureByRoutePercentileRecord> BoxPlotRecords { get; set; } = [];
        public ExposureUnitTriple ExposureUnit { get; set; }

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson,
            bool skipPrivacySensitiveOutputs
        ) {
            //var result = new List<ExternalExposureByRouteRecord>();
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalIndividualExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            ExposureUnit = externalExposureUnit;

            var exposureRouteCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );

            Records = summarizeExposureRecords(
                exposureRouteCollection,
                percentages
            );

            BoxPlotRecords = summarizeBoxPlotsRecords(
                exposureRouteCollection,
                externalExposureUnit
            );
        }

        public List<ExternalExposureByRouteRecord> summarizeExposureRecords(
            List<(ExposureRoute ExposureRoute, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureRouteCollection,
            double[] percentages
        ) {
            var results = new List<ExternalExposureByRouteRecord>();
            foreach (var item in exposureRouteCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var record = getExposureRouteRecord(
                        item.ExposureRoute,
                        item.Exposures,
                        percentages
                    );
                    results.Add(record);
                }
            }
            return results;
        }

        private ExternalExposureByRouteRecord getExposureRouteRecord(
            ExposureRoute route,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double[] percentages
        ) {
            var weights = exposures.Where(c => c.Exposure > 0)
                .Select(idi => idi.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentiles = exposures.Where(c => c.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);

            var weightsAll = exposures.Select(idi => idi.SimulatedIndividual.SamplingWeight).ToList();
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var total = exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var record = new ExternalExposureByRouteRecord {
                ExposureRoute = route.GetShortDisplayName(),
                Percentage = weights.Count / (double)exposures.Count * 100,
                MeanAll = total / weightsAll.Sum(),
                Mean = total/weights.Sum(),
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

        private List<ExternalExposureByRoutePercentileRecord> summarizeBoxPlotsRecords(
            List<(ExposureRoute ExposureRoute, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureRouteCollection,
            ExposureUnitTriple externalExposureUnit
        ) {
            var result = new List<ExternalExposureByRoutePercentileRecord>();
            foreach (var item in exposureRouteCollection) {
                if (item.Exposures.Any(c => c.Exposure > 0)) {
                    var boxPlotRecord = getBoxPlotRecord(
                        item.ExposureRoute,
                        item.Exposures,
                        externalExposureUnit
                    );
                    result.Add(boxPlotRecord);
                }
            }
            return result;
        }

        private static ExternalExposureByRoutePercentileRecord getBoxPlotRecord(
            ExposureRoute route,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            ExposureUnitTriple unit
        ) {
            var weights = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var allExposures = exposures
                .Select(c => c.Exposure)
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
            var record = new ExternalExposureByRoutePercentileRecord() {
                ExposureRoute = route.GetDisplayName(),
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentiles,
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count,
                Unit = unit.GetShortDisplayName(),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return record;
        }
    }
}
