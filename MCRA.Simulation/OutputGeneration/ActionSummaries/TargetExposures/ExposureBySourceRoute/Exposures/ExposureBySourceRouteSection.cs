using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceRouteSection : SummarySection {
        public override bool SaveTemporaryData => true;

        private static readonly double _upperWhisker = 95;

        private static readonly double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public bool ShowOutliers { get; set; }
        public double? RestrictedUpperPercentile { get; set; }
        public List<ExposureBySourceRouteRecord> ExposureRecords { get; set; }
        public List<ExposureBySourceRoutePercentileRecord> ExposureBoxPlotRecords { get; set; } = [];
        public ExposureUnitTriple ExposureUnit { get; set; }

        public void Summarize(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson,
            bool skipPrivacySensitiveOutputs
        ) {
            var result = new List<ExternalExposureBySourceRecord>();
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            if (skipPrivacySensitiveOutputs) {
                var maxUpperPercentile = SimulationConstants.MaxUpperPercentage(externalExposureCollections.First().ExternalIndividualDayExposures.Count);
                if (_upperWhisker > maxUpperPercentile) {
                    RestrictedUpperPercentile = maxUpperPercentile;
                }
            }
            ShowOutliers = !skipPrivacySensitiveOutputs;
            ExposureUnit = externalExposureUnit;
            var observedIndividualMeans = dietaryIndividualDayIntakes
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
                    Exposure: c.Sum(r => r.Exposure) / c.First().NumberOfDays
                ))
                .ToList();

            ExposureRecords = summarizeExposureRecords(
                externalExposureCollections,
                observedIndividualMeans,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                routes,
                kineticConversionFactors,
                percentages,
                isPerPerson
            );

            ExposureBoxPlotRecords = summarizeBoxPlotsRecords(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                externalExposureUnit,
                routes,
                kineticConversionFactors,
                isPerPerson
            );
        }

        public List<ExposureBySourceRouteRecord> summarizeExposureRecords(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            List<(double SamplingWeight, double Exposure)> observedIndividualMeans,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double[] percentages,
            bool isPerPerson
        ) {
            var results = new List<ExposureBySourceRouteRecord>();
            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposures = collection.ExternalIndividualDayExposures
                        .Select(id => (
                            SamplingWeight: id.IndividualSamplingWeight,
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
                        var record = getExposureSourceRouteRecord(
                            route,
                            collection.ExposureSource,
                            exposures,
                            percentages
                        );
                        results.Add(record);
                    }
                }
            }

            if (observedIndividualMeans != null) {
                results.Add(getExposureSourceRouteRecord(
                    ExposureRoute.Oral,
                    ExposureSource.Diet,
                    observedIndividualMeans,
                    percentages
                 ));
            }
            return results;
        }


        private ExposureBySourceRouteRecord getExposureSourceRouteRecord(
            ExposureRoute route,
            ExposureSource source,
            List<(double SamplingWeight, double Exposure)> exposures,
            double[] percentages
        ) {
            var weights = exposures.Where(c => c.Exposure > 0)
                .Select(idi => idi.SamplingWeight)
                .ToList();
            var percentiles = exposures.Where(c => c.Exposure > 0)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);
            var weightsAll = exposures
                .Select(idi => idi.SamplingWeight)
                .ToList();
            var percentilesAll = exposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weightsAll, percentages);
            var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
            var record = new ExposureBySourceRouteRecord {
                ExposureSource = source.GetShortDisplayName(),
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

        private List<ExposureBySourceRoutePercentileRecord> summarizeBoxPlotsRecords(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            List<(double SamplingWeight, double Exposure)> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureUnitTriple externalExposureUnit,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = new List<ExposureBySourceRoutePercentileRecord>();
            foreach (var collection in externalExposureCollections) {
                foreach (var route in routes) {
                    var exposures = collection.ExternalIndividualDayExposures
                        .Select(id => (
                            SamplingWeight: id.IndividualSamplingWeight,
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
                        var boxPlotRecord = getBoxPlotRecord(
                            collection.ExposureSource,
                            route,
                            exposures,
                            externalExposureUnit
                        );
                        result.Add(boxPlotRecord);
                    }
                }
            }
            if (observedIndividualMeans != null) {
                var dietaryBoxPlotRecord = getBoxPlotRecord(
                    ExposureSource.Diet,
                    ExposureRoute.Oral,
                    observedIndividualMeans,
                    externalExposureUnit
                );
                result.Add(dietaryBoxPlotRecord);
            }
            return result;
        }

        private static ExposureBySourceRoutePercentileRecord getBoxPlotRecord(
            ExposureSource source,
            ExposureRoute route,
            List<(double SamplingWeight, double Exposure)> exposures,
            ExposureUnitTriple unit
        ) {
            var weights = exposures
                .Select(c => c.SamplingWeight)
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
            var record = new ExposureBySourceRoutePercentileRecord() {
                ExposureSource = source.GetDisplayName(),
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
