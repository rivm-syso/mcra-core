using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.OpexProductDefinitions.Dto;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries;
using MCRA.Simulation.OutputGeneration.ActionSummaries.DustExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class DustExposuresByRouteSection : DustExposuresByRouteSectionBase {

        public void Summarize(
            ICollection<Compound> substances,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            double lowerPercentage,
            double upperPercentage,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> exposureRoutes,
            ExposureType exposureType
        ) {
            ShowOutliers = true;

            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            foreach (var exposureRoute in exposureRoutes) {
                foreach (var substance in substances) {
                    var record = GetSummaryRecord(
                        percentages,
                        exposureType,
                        exposureRoute,
                        dustIndividualDayExposures,
                        substance,
                        exposureUnit
                    );
                    DustExposuresByRouteRecords.Add(record);
                }
            }

            summarizeBoxPlotsPerRoute(
                exposureType,
                exposureRoutes,
                dustIndividualDayExposures,
                substances,
                exposureUnit
            );
        }

        /// <summary>
        /// Acute summarizer
        /// </summary>
        protected static DustExposuresByRouteRecord GetSummaryRecord(
            double[] percentages,
            ExposureType exposureType,
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
                );

            if (exposureType == ExposureType.Chronic) {
                exposures = exposures
                    .GroupBy(r => r.SimulatedIndividualId)
                    .Select(g => (
                        SimulatedIndividualId: g.Key,
                        g.First().IndividualSamplingWeight,
                        Amount: g.Sum(s => s.Amount) / g.Count()
                    ));
            }

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

        /// <summary>
        /// Boxplot summarizer
        /// </summary>
        protected static List<DustExposuresPercentilesRecord> SummarizeBoxPlot(
            ExposureType exposureType,
            ExposureRoute dustExposureRoute,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            ExposureUnitTriple exposureUnit
        ) {
            var result = new List<DustExposuresPercentilesRecord>();
            getBoxPlotRecord(
                result,
                substances,
                exposureType,
                dustExposureRoute,
                dustIndividualDayExposures,
                exposureUnit
            );
            return result;
        }

        private void summarizeBoxPlotsPerRoute(
            ExposureType exposureType,
            ICollection<ExposureRoute> dustExposureRoutes,
            ICollection<DustIndividualDayExposure> individualDustExposures,
            ICollection<Compound> substances,
            ExposureUnitTriple exposureUnit
        ) {
            foreach (var dustExposureRoute in dustExposureRoutes) {
                var dustExposureRoutesPercentilesRecords =
                    SummarizeBoxPlot(
                        exposureType,
                        dustExposureRoute,
                        individualDustExposures,
                        substances,
                        exposureUnit);
                if (dustExposureRoutesPercentilesRecords.Count > 0) {
                    DustExposuresBoxPlotRecords[dustExposureRoute] = dustExposureRoutesPercentilesRecords;
                }
            }
        }
    }
}
