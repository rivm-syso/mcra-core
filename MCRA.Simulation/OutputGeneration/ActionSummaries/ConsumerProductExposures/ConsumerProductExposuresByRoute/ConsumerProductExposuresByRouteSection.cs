using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.ConsumerProductExposures;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ConsumerProductExposuresByRouteSection : SummarySection {
        protected readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public List<ConsumerProductExposuresByRouteRecord> ConsumerProductExposuresByRouteRecords { get; set; } = [];
        public SerializableDictionary<ExposureRoute, List<ConsumerProductExposuresPercentilesRecord>> ConsumerProductExposuresBoxPlotRecords { get; set; } = [];

        public bool ShowOutliers { get; set; } = false;

        public void Summarize(
            ICollection<Compound> substances,
            ICollection<ConsumerProductIndividualDayExposure> cpIndividualDayExposures,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var cpExposuresByRouteRecords = new List<ConsumerProductExposuresByRouteRecord>();
            foreach (var exposureRoute in routes) {
                foreach (var substance in substances) {
                    var record = getSummaryRecord(
                        percentages,
                        exposureRoute,
                        cpIndividualDayExposures,
                        substance,
                        exposureUnit
                    );
                    cpExposuresByRouteRecords.Add(record);
                }
            }
            ConsumerProductExposuresByRouteRecords = cpExposuresByRouteRecords
                .OrderBy(r => r.ExposureRoute)
                .ThenBy(r => r.SubstanceName)
                .ToList();

            foreach (var route in routes) {
                var dustExposureRoutesPercentilesRecords =
                    getBoxPlotRecords(
                        route,
                        cpIndividualDayExposures,
                        substances,
                        exposureUnit
                    );
                if (dustExposureRoutesPercentilesRecords.Count > 0) {
                    ConsumerProductExposuresBoxPlotRecords[route] = dustExposureRoutesPercentilesRecords;
                }
            }
        }

        private static ConsumerProductExposuresByRouteRecord getSummaryRecord(
            double[] percentages,
            ExposureRoute route,
            ICollection<ConsumerProductIndividualDayExposure> cpIndividualDayExposures,
            Compound substance,
            ExposureUnitTriple exposureUnit
        ) {
            var exposures = cpIndividualDayExposures
                .Select(
                    r => (
                        Exposure: r.GetExposure(
                            route,
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
            var record = new ConsumerProductExposuresByRouteRecord {
                ExposureRoute = route.GetDisplayName(),
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

        private static List<ConsumerProductExposuresPercentilesRecord> getBoxPlotRecords(
            ExposureRoute dustExposureRoute,
            ICollection<ConsumerProductIndividualDayExposure> cpIndividualDayExposures,
            ICollection<Compound> substances,
            ExposureUnitTriple exposureUnit
        ) {
            var result = new List<ConsumerProductExposuresPercentilesRecord>();

            foreach (var substance in substances) {
                var exposures = cpIndividualDayExposures
                    .Select(
                        r => (
                            Exposure: r.GetExposure(
                                dustExposureRoute,
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

                var record = new ConsumerProductExposuresPercentilesRecord() {
                    ExposureRoute = dustExposureRoute.ToString(),
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
