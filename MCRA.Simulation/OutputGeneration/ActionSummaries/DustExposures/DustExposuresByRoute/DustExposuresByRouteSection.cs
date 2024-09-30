using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries;
using MCRA.Simulation.OutputGeneration.ActionSummaries.DustExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class DustExposuresByRouteSection : DustExposuresByRouteSectionBase {

        public void Summarize(
            ICollection<Compound> substances,            
            ICollection<DustIndividualDayExposure> individualDustExposures,
            double lowerPercentage,
            double upperPercentage
        ) {
            ShowOutliers = true;

            var dustExposureRoutes = individualDustExposures                
                .SelectMany(r => r.ExposurePerSubstanceRoute.Keys)                
                .Distinct()
                .ToList();

            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            foreach (var dustExposureRoute in dustExposureRoutes) {
                foreach (var substance in substances) {
                    var record = GetSummaryRecord(percentages, dustExposureRoute, individualDustExposures, substance);
                    DustExposuresByRouteRecords.Add(record);
                }
            }

            summarizeBoxPlotsPerRoute(
                dustExposureRoutes,
                individualDustExposures,
                substances
            );
        }

        /// <summary>
        /// Acute summarizer
        /// </summary>
        protected static DustExposuresByRouteRecord GetSummaryRecord(
            double[] percentages,
            ExposureRoute dustExposureRoute,
            ICollection<DustIndividualDayExposure> individualDustExposures,
            Compound substance
        ) {
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

            var positives = allExposures.Where(r => r > 0);

            var weightsPositives = exposures
                .Where(r => r.Amount > 0)
                .Select(r => r.IndividualSamplingWeight)
                .ToList();
                          
            var exposureUnit = individualDustExposures.FirstOrDefault().ExposureUnit;
            
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
                PercentagePositives = positives.Count() * 100d / allExposures.Count(),
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

        /// <summary>
        /// Boxplot summarizer
        /// </summary>
        protected static List<DustExposuresPercentilesRecord> SummarizeBoxPlot(
            ExposureRoute dustExposureRoute,
            ICollection<DustIndividualDayExposure> individualDustExposures,
            ICollection<Compound> substances
        ) {
            var result = new List<DustExposuresPercentilesRecord>();
            getBoxPlotRecord(
                result,
                substances,
                dustExposureRoute,
                individualDustExposures
            );
            return result;
        }

        private void summarizeBoxPlotsPerRoute(
            ICollection<ExposureRoute> dustExposureRoutes,
            ICollection<DustIndividualDayExposure> individualDustExposures,
            ICollection<Compound> substances
        ) {
            foreach (var dustExposureRoute in dustExposureRoutes) {
                var dustExposureRoutesPercentilesRecords =
                    SummarizeBoxPlot(dustExposureRoute, individualDustExposures, substances);
                if (dustExposureRoutesPercentilesRecords.Count > 0) {
                    DustExposuresBoxPlotRecords[dustExposureRoute] = dustExposureRoutesPercentilesRecords;
                }
            }
        }
    }
}
