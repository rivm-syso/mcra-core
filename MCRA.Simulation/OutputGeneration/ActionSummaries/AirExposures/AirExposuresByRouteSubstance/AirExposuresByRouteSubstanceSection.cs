using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class AirExposuresByRouteSubstanceSection : SummarySection {
        protected readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public List<AirExposuresByRouteSubstanceRecord> Records { get; set; } = [];
        public SerializableDictionary<ExposureRoute, List<AirExposuresPercentilesRecord>> BoxPlotRecords { get; set; } = [];

        public bool ShowOutliers { get; set; } = false;

        public void Summarize(
            ICollection<Compound> substances,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var exposuresByRouteSubstanceRecords = new List<AirExposuresByRouteSubstanceRecord>();
            foreach (var route in routes) {
                foreach (var substance in substances) {
                    var record = getSummaryRecord(
                        percentages,
                        route,
                        airIndividualDayExposures,
                        substance,
                        exposureUnit
                    );
                    exposuresByRouteSubstanceRecords.Add(record);
                }
            }
            Records = exposuresByRouteSubstanceRecords
                .OrderBy(r => r.ExposureRoute)
                .ThenBy(r => r.SubstanceName)
                .ToList();

            foreach (var route in routes) {
                var percentilesRecords =
                    getBoxPlotRecords(
                        route,
                        airIndividualDayExposures,
                        substances,
                        exposureUnit
                    );
                if (percentilesRecords.Count > 0) {
                    BoxPlotRecords[route] = percentilesRecords;
                }
            }
        }

        private static AirExposuresByRouteSubstanceRecord getSummaryRecord(
            double[] percentages,
            ExposureRoute airExposureRoute,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            Compound substance,
            ExposureUnitTriple exposureUnit
        ) {
            var exposures = airIndividualDayExposures
                .Select(
                    r => (
                        Exposure: r.GetExposure(
                            airExposureRoute,
                            substance,
                            exposureUnit.IsPerUnit
                        ),
                        SamplingWeight: r.SimulatedIndividual.SamplingWeight
                ))
                .ToList();

            var allExposures = exposures
                .Select(r => r.Exposure)
                .ToList();

            var weightsAll = exposures
                .Select(r => r.SamplingWeight)
                .ToList();

            var positives = allExposures.Where(r => r > 0);

            var weightsPositives = exposures
                .Where(r => r.Exposure > 0)
                .Select(r => r.SamplingWeight)
                .ToList();

            var percentiles = positives
                .PercentilesWithSamplingWeights(
                    weightsPositives,
                    percentages
                );
            var percentilesAll = allExposures
                .PercentilesWithSamplingWeights(
                    weightsAll,
                    percentages
                );
            var record = new AirExposuresByRouteSubstanceRecord {
                ExposureRoute = airExposureRoute.GetDisplayName(),
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Unit = exposureUnit.GetShortDisplayName(),
                NumberOfIndividuals = positives.Count(),
                MeanAll = exposures.Sum(c => c.Exposure * c.SamplingWeight) / weightsAll.Sum(),
                PercentagePositives = positives.Count() * 100d / allExposures.Count,
                MeanPositives = exposures.Sum(c => c.Exposure * c.SamplingWeight) / weightsPositives.Sum(),
                LowerPercentilePositives = percentiles[0],
                Median = percentiles[1],
                UpperPercentilePositives = percentiles[2],
                LowerPercentileAll = percentilesAll[0],
                MedianAll = percentilesAll[1],
                UpperPercentileAll = percentilesAll[2],
                MedianAllUncertaintyValues = []
            };
            return record;
        }

        private static List<AirExposuresPercentilesRecord> getBoxPlotRecords(
            ExposureRoute airExposureRoute,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            ExposureUnitTriple exposureUnit
        ) {
            var result = new List<AirExposuresPercentilesRecord>();
            foreach (var substance in substances) {
                var exposures = airIndividualDayExposures
                    .Select(
                        r => (
                            Exposure: r.GetExposure(
                                airExposureRoute,
                                substance,
                                exposureUnit.IsPerUnit
                            ),
                            SamplingWeight: r.SimulatedIndividual.SamplingWeight
                    ))
                    .ToList();

                var allExposures = exposures
                    .Select(r => r.Exposure)
                    .ToList();

                var weightsAll = exposures
                    .Select(r => r.SamplingWeight)
                    .ToList();

                var percentiles = allExposures
                    .PercentilesWithSamplingWeights(weightsAll, _percentages)
                    .ToList();
                var positives = allExposures.Where(r => r > 0).ToList();
                var p95Idx = _percentages.Length - 1;
                var substanceName = percentiles[p95Idx] > 0 ? substance.Name : $"{substance.Name} *";
                var outliers = allExposures
                    .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                        || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                    .Select(c => c)
                    .ToList();

                var record = new AirExposuresPercentilesRecord() {
                    ExposureRoute = airExposureRoute.ToString(),
                    MinPositives = positives.Any() ? positives.Min() : double.NaN,
                    MaxPositives = positives.Any() ? positives.Max() : double.NaN,
                    SubstanceCode = substance.Code,
                    SubstanceName = substanceName,
                    Percentiles = [.. percentiles],
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / allExposures.Count,
                    Unit = exposureUnit.GetShortDisplayName(),
                    Outliers = outliers,
                    NumberOfOutLiers = outliers.Count,
                };
                result.Add(record);
            }
            return result;
        }
    }
}
