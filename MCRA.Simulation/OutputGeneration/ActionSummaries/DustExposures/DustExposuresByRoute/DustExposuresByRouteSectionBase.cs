using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.DustExposures;
using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries {
    public class DustExposuresByRouteSectionBase : SummarySection {
        protected readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = [5, 10, 25, 50, 75, 90, 95];
        public List<DustExposuresByRouteRecord> DustExposuresByRouteRecords { get; set; } = [];
        public SerializableDictionary<ExposureRoute, List<DustExposuresPercentilesRecord>> DustExposuresBoxPlotRecords { get; set; } = [];

        public bool ShowOutliers { get; set; } = false;

        /// <summary>
        /// Get boxplot record
        /// </summary>
        protected static void getBoxPlotRecord(
            List<DustExposuresPercentilesRecord> result,
            ICollection<Compound> substances,
            ExposureRoute dustExposureRoute,
            ICollection<DustIndividualDayExposure> individualDustExposures,
            ExposureUnitTriple exposureUnit
        ) {
            foreach (var substance in substances) {

                var exposures = individualDustExposures
                    .SelectMany(r => r.ExposurePerSubstanceRoute[dustExposureRoute]
                    .Where(s => s.Compound == substance)
                    .Select(s => (r.IndividualSamplingWeight, s.Amount)));

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
                    Percentiles = percentiles.ToList(),
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / allExposures.Count(),
                    Unit = exposureUnit.GetShortDisplayName(),
                    Outliers = outliers,
                    NumberOfOutLiers = outliers.Count(),
                };
                result.Add(record);
            }
        }
    }
}
