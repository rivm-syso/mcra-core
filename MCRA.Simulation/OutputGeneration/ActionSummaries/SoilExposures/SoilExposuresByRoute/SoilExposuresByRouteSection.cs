using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SoilExposures;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class SoilExposuresByRouteSection : SummarySection {
        protected readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public List<SoilExposuresByRouteRecord> SoilExposuresByRouteRecords { get; set; } = [];
        public SerializableDictionary<ExposureRoute, List<SoilExposuresPercentilesRecord>> SoilExposuresBoxPlotRecords { get; set; } = [];

        public bool ShowOutliers { get; set; } = false;

        public void Summarize(
            ICollection<Compound> substances,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var soilExposuresByRouteRecords = new List<SoilExposuresByRouteRecord>();
            foreach (var exposureRoute in routes) {
                foreach (var substance in substances) {
                    var record = getSummaryRecord(
                        percentages,
                        exposureRoute,
                        soilIndividualDayExposures,
                        substance,
                        exposureUnit
                    );
                    soilExposuresByRouteRecords.Add(record);
                }
            }
            SoilExposuresByRouteRecords = soilExposuresByRouteRecords
                .OrderBy(r => r.ExposureRoute)
                .ThenBy(r => r.SubstanceName)
                .ToList();

            foreach (var soilExposureRoute in routes) {
                var soilExposureRoutesPercentilesRecords =
                    getBoxPlotRecords(
                        soilExposureRoute,
                        soilIndividualDayExposures,
                        substances,
                        exposureUnit
                    );
                if (soilExposureRoutesPercentilesRecords.Count > 0) {
                    SoilExposuresBoxPlotRecords[soilExposureRoute] = soilExposureRoutesPercentilesRecords;
                }
            }
        }

        private static SoilExposuresByRouteRecord getSummaryRecord(
            double[] percentages,
            ExposureRoute soilExposureRoute,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            Compound substance,
            ExposureUnitTriple exposureUnit
        ) {
            var exposures = soilIndividualDayExposures
                .Select(
                    r => (
                        Exposure: r.GetSubstanceExposureForRoute(
                            soilExposureRoute,
                            substance,
                            exposureUnit.IsPerUnit()
                        ),
                        SamplingWeight: r.IndividualSamplingWeight
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
            var record = new SoilExposuresByRouteRecord {
                ExposureRoute = soilExposureRoute.GetDisplayName(),
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

        private static List<SoilExposuresPercentilesRecord> getBoxPlotRecords(
            ExposureRoute soilExposureRoute,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            ExposureUnitTriple exposureUnit
        ) {
            var result = new List<SoilExposuresPercentilesRecord>();

            foreach (var substance in substances) {
                var exposures = soilIndividualDayExposures
                    .Select(
                        r => (
                            Exposure: r.GetSubstanceExposureForRoute(
                                soilExposureRoute,
                                substance,
                                exposureUnit.IsPerUnit()
                            ),
                            SamplingWeight: r.IndividualSamplingWeight
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

                var record = new SoilExposuresPercentilesRecord() {
                    ExposureRoute = soilExposureRoute.ToString(),
                    MinPositives = positives.Any() ? positives.Min() : double.NaN,
                    MaxPositives = positives.Any() ? positives.Max() : double.NaN,
                    SubstanceCode = substance.Code,
                    SubstanceName = substanceName,
                    Description = substanceName,
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
