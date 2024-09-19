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

        protected static double[] _percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
        public List<DustExposuresByRouteRecord> DustExposuresByRouteRecords { get; set; } = new();
        public SerializableDictionary<ExposureRoute, List<DustExposuresPercentilesRecord>> DustExposuresBoxPlotRecords { get; set; } = new();

        public bool ShowOutliers { get; set; } = false;

        /// <summary>
        /// Get boxplot record
        /// </summary>
        /// <param name="result"></param>
        /// <param name="dustExposureRoute"></param>
        /// <param name="individualDustExposures"></param>
        /// <param name="substances"></param>
        
        protected static void getBoxPlotRecord(
            List<DustExposuresPercentilesRecord> result,
            ICollection<Compound> substances,
            ExposureRoute dustExposureRoute,
            ICollection<IndividualDustExposureRecord> individualDustExposures
        ) {
            foreach (var substance in substances) {

                var allExposures = individualDustExposures
                    .Where(c => c.Substance == substance & c.ExposureRoute == dustExposureRoute)
                    .Select(c => c.Exposure)
                    .ToList();                    
                var exposureUnit = individualDustExposures.FirstOrDefault().ExposureUnit;

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
