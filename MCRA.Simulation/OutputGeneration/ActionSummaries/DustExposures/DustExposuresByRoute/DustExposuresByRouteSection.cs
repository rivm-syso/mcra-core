using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.DustExposures;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class DustExposuresByRouteSection : SummarySection {
        protected readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public List<DustExposuresByRouteRecord> DustExposuresByRouteRecords { get; set; } = [];
        public SerializableDictionary<ExposureRoute, List<DustExposuresPercentilesRecord>> DustExposuresBoxPlotRecords { get; set; } = [];

        public bool ShowOutliers { get; set; } = false;

        public void Summarize(
            ICollection<Compound> substances,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> exposureRoutes
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            foreach (var exposureRoute in exposureRoutes) {
                foreach (var substance in substances) {
                    var record = getSummaryRecord(
                        percentages,
                        exposureRoute,
                        dustIndividualDayExposures,
                        substance,
                        exposureUnit
                    );
                    DustExposuresByRouteRecords.Add(record);
                }
            }

            summarizeBoxPlotsPerRoute(
                exposureRoutes,
                dustIndividualDayExposures,
                substances,
                exposureUnit
            );
        }

        private static DustExposuresByRouteRecord getSummaryRecord(
            double[] percentages,
            ExposureRoute dustExposureRoute,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            Compound substance,
            ExposureUnitTriple exposureUnit
        ) {
            var exposures = dustIndividualDayExposures
                .SelectMany(r => r.ExposurePerSubstanceRoute[dustExposureRoute]
                .Where(s => s.Compound == substance)
                .Select(s => (
                    r.SimulatedIndividualId,
                    r.IndividualSamplingWeight,
                    s.Amount))
                )
                .GroupBy(r => r.SimulatedIndividualId)
                .Select(g => (
                    SimulatedIndividualId: g.Key,
                    g.First().IndividualSamplingWeight,
                    Amount: g.Sum(s => s.Amount) / g.Count()
                ));

            var allExposures = exposures
                .Select(r => r.Amount)
                .ToList();

            var weightsAll = exposures
                .Select(r => r.IndividualSamplingWeight)
                .ToList();

            var positives = allExposures.Where(r => r > 0);

            var weightsPositives = exposures
                .Where(r => r.Amount > 0)
                .Select(r => r.IndividualSamplingWeight)
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
            var record = new DustExposuresByRouteRecord {
                ExposureRoute = dustExposureRoute.GetDisplayName(),
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Unit = exposureUnit.GetShortDisplayName(),
                MeanAll = exposures.Sum(c => c.Amount * c.IndividualSamplingWeight) / weightsAll.Sum(),
                PercentagePositives = positives.Count() * 100d / allExposures.Count,
                MeanPositives = exposures.Sum(c => c.Amount * c.IndividualSamplingWeight) / weightsPositives.Sum(),
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

        private void summarizeBoxPlotsPerRoute(
            ICollection<ExposureRoute> dustExposureRoutes,
            ICollection<DustIndividualDayExposure> individualDustExposures,
            ICollection<Compound> substances,
            ExposureUnitTriple exposureUnit
        ) {
            foreach (var dustExposureRoute in dustExposureRoutes) {
                var dustExposureRoutesPercentilesRecords =
                    summarizeBoxPlot(
                        dustExposureRoute,
                        individualDustExposures,
                        substances,
                        exposureUnit
                    );
                if (dustExposureRoutesPercentilesRecords.Count > 0) {
                    DustExposuresBoxPlotRecords[dustExposureRoute] = dustExposureRoutesPercentilesRecords;
                }
            }
        }

        /// <summary>
        /// Boxplot summarizer
        /// </summary>
        private static List<DustExposuresPercentilesRecord> summarizeBoxPlot(
            ExposureRoute dustExposureRoute,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            ExposureUnitTriple exposureUnit
        ) {
            var result = new List<DustExposuresPercentilesRecord>();

            foreach (var substance in substances) {
                var exposures = dustIndividualDayExposures
                    .SelectMany(r => r.ExposurePerSubstanceRoute[dustExposureRoute]
                    .Where(s => s.Compound == substance)
                    .Select(s => (
                        r.SimulatedIndividualId,
                        r.IndividualSamplingWeight,
                        s.Amount))
                    )
                    .GroupBy(r => r.SimulatedIndividualId)
                    .Select(g => (
                        SimulatedIndividualId: g.Key,
                        g.First().IndividualSamplingWeight,
                        Amount: g.Sum(s => s.Amount) / g.Count()
                    ));

                var allExposures = exposures
                    .Select(r => r.Amount)
                    .ToList();

                var weightsAll = exposures
                    .Select(r => r.IndividualSamplingWeight)
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

                var record = new DustExposuresPercentilesRecord() {
                    ExposureRoute = dustExposureRoute.ToString(),
                    MinPositives = positives.Any() ? positives.Min() : null,
                    MaxPositives = positives.Any() ? positives.Max() : null,
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
