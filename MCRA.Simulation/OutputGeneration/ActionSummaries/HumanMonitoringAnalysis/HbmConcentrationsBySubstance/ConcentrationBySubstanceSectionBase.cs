using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries {
    public class ConcentrationBySubstanceSectionBase : SummarySection {
        protected readonly double _upperWhisker = 95;
        public override bool SaveTemporaryData => true;

        protected static double[] _percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
        public List<HbmIndividualDayDistributionBySubstanceRecord> IndividualDayRecords { get; set; } = [];
        public List<HbmIndividualDistributionBySubstanceRecord> IndividualRecords { get; set; } = [];
        public SerializableDictionary<ExposureTarget, List<HbmConcentrationsPercentilesRecord>> HbmBoxPlotRecords { get; set; } = [];
        public double? RestrictedUpperPercentile { get; set; }
        public bool ShowOutliers { get; set; }
        /// <summary>
        /// Get boxplot record
        /// </summary>
        /// <param name="result"></param>
        /// <param name="percentages"></param>
        /// <param name="multipleSamplingMethods"></param>
        /// <param name="substance"></param>
        /// <param name="hbmIndividualDayConcentrations"></param>
        protected static void getBoxPlotRecord(
            List<HbmConcentrationsPercentilesRecord> result,
            Compound substance,
            List<(double samplingWeight, double totalEndpointExposures, List<HumanMonitoringSamplingMethod> sourceSamplingMethods)> hbmIndividualDayConcentrations,
            TargetUnit targetUnit
        ) {
            if (hbmIndividualDayConcentrations.Any(c => c.totalEndpointExposures > 0)) {
                var sourceSamplingMethods = hbmIndividualDayConcentrations
                    .SelectMany(c => c.sourceSamplingMethods)
                    .GroupBy(c => c)
                    .Select(c => c.Key.Name)
                    .ToList();
                var weights = hbmIndividualDayConcentrations
                    .Select(c => c.samplingWeight)
                    .ToList();
                var allExposures = hbmIndividualDayConcentrations
                    .Select(c => c.totalEndpointExposures)
                    .ToList();
                var percentiles = allExposures
                    .PercentilesWithSamplingWeights(weights, _percentages)
                    .ToList();
                var positives = allExposures.Where(r => r > 0).ToList();

                var p95Idx = _percentages.Length - 1;
                var substanceName = percentiles[p95Idx] > 0 ? substance.Name : $"{substance.Name} *";
                var outliers = allExposures
                        .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                            || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                        .Select(c => c)
                        .ToList();

                var record = new HbmConcentrationsPercentilesRecord() {
                    MinPositives = positives.Any() ? positives.Min() : 0,
                    MaxPositives = positives.Any() ? positives.Max() : 0,
                    SubstanceCode = substance.Code,
                    SubstanceName = substanceName,
                    Description = substanceName,
                    Percentiles = percentiles.ToList(),
                    NumberOfPositives = positives.Count,
                    Percentage = positives.Count * 100d / hbmIndividualDayConcentrations.Count,
                    Unit = targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType),
                    Outliers = outliers,
                    NumberOfOutLiers = outliers.Count(),
                };
                result.Add(record);
            }
        }
    }
}
