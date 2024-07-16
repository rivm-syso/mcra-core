using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.DustExposures;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries {
    public class DustExposuresByRouteSectionBase : SummarySection {
        protected readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
        public List<DustExposuresByRouteRecord> DustExposuresByRouteRecords { get; set; } = new();
        public SerializableDictionary<ExposureRoute, List<DustExposuresPercentilesRecord>> DustExposuresBoxPlotRecords { get; set; } = new();

        public bool ShowOutliers { get; set; } = false;

        /// <summary>
        /// Get boxplot record
        /// </summary>
        /// <param name="result"></param>
        /// <param name="dustExposureRoute"></param>
        /// <param name="dustExposuresBySurveys"></param>
        /// <param name="substances"></param>
        /// <param name="concentrationUnit"></param>
        protected static void getBoxPlotRecord(
            List<DustExposuresPercentilesRecord> result,
            ICollection<Compound> substances,
            ExposureRoute dustExposureRoute,
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> dustExposuresBySurveys
        ) {
            foreach (var substance in substances) {

                var allExposures = dustExposuresBySurveys
                    .SelectMany(surveyExposures => surveyExposures.Value
                    .Where(r => string.IsNullOrEmpty(r.Code))
                    .SelectMany(c => c.NonDietaryExposures)
                    .Where(c => c.Compound == substance)
                    .Select(c => dustExposureRoute == ExposureRoute.Inhalation ? c.Inhalation : c.Dermal)
                    .ToList()
                    );
                var exposureUnit = dustExposuresBySurveys.Keys.FirstOrDefault().ExposureUnit;

                var percentiles = allExposures
                    .Percentiles(_percentages)
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
                    MinPositives = positives.Any() ? positives.Min() : 0,
                    MaxPositives = positives.Any() ? positives.Max() : 0,
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
