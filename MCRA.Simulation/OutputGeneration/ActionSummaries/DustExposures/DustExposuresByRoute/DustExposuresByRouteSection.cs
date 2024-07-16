using MathNet.Numerics.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries;
using MCRA.Simulation.OutputGeneration.ActionSummaries.DustExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class DustExposuresByRouteSection : DustExposuresByRouteSectionBase {

        public void Summarize(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> dustExposureRoutes,
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> dustExposuresBySurveys,
            double lowerPercentage,
            double upperPercentage
        ) {
            ShowOutliers = true;

            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            foreach (var dustExposureRoute in dustExposureRoutes) {
                foreach (var substance in substances) {
                    var record = GetSummaryRecord(percentages, dustExposureRoute, dustExposuresBySurveys, substance);
                    DustExposuresByRouteRecords.Add(record);
                }
            }

            summarizeBoxPlotsPerRoute(
                dustExposureRoutes,
                dustExposuresBySurveys,
                substances
            );
        }

        /// <summary>
        /// Acute summarizer
        /// </summary>
        protected static DustExposuresByRouteRecord GetSummaryRecord(
            double[] percentages,
            ExposureRoute dustExposureRoute,
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> dustExposuresBySurveys,
            Compound substance
        ) {
            var allExposures = dustExposuresBySurveys
                .SelectMany(surveyExposures => surveyExposures.Value
                    .Where(r => string.IsNullOrEmpty(r.Code))
                    .SelectMany(c => c.NonDietaryExposures)
                    .Where(c => c.Compound == substance)
                    .Select(c => dustExposureRoute == ExposureRoute.Inhalation ? c.Inhalation : c.Dermal)
                    .ToList()
                );
            var exposureUnit = dustExposuresBySurveys.Keys.FirstOrDefault().ExposureUnit;
            var positives = allExposures.Where(r => r > 0).ToList();
            var percentiles = allExposures
                .Where(c => c > 0)
                .Percentiles(percentages);
            var percentilesAll = allExposures
                .Percentiles(percentages);
            var record = new DustExposuresByRouteRecord {
                ExposureRoute = dustExposureRoute.GetDisplayName(),
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Unit = exposureUnit.GetShortDisplayName(),
                MeanAll = allExposures.Mean(),
                PercentagePositives = positives.Count * 100d / allExposures.Count(),
                MeanPositives = positives.Mean(),
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

        /// <summary>
        /// Boxplot summarizer
        /// </summary>
        protected List<DustExposuresPercentilesRecord> SummarizeBoxPlot(
            ExposureRoute dustExposureRoute,
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> dustExposuresBySurveys,
            ICollection<Compound> substances
        ) {
            var result = new List<DustExposuresPercentilesRecord>();
            getBoxPlotRecord(
                result,
                substances,
                dustExposureRoute,
                dustExposuresBySurveys
            );
            return result;
        }

        private void summarizeBoxPlotsPerRoute(
            ICollection<ExposureRoute> dustExposureRoutes,
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> dustExposuresBySurveys,
            ICollection<Compound> substances
        ) {
            foreach (var dustExposureRoute in dustExposureRoutes) {
                var dustExposureRoutesPercentilesRecords =
                    SummarizeBoxPlot(dustExposureRoute, dustExposuresBySurveys, substances);
                if (dustExposureRoutesPercentilesRecords.Count > 0) {
                    DustExposuresBoxPlotRecords[dustExposureRoute] = dustExposureRoutesPercentilesRecords;
                }
            }
        }
    }
}
