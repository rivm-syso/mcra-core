using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmContributionsSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public ExposureTarget Target { get; set; }
        public List<HbmContributionPercentilesRecord> HbmBoxPlotRecords { get; set; } = new();
        public List<IndividualContributionsRecord> IndividualContributionRecords { get; set; } = new();

        public static (HbmContributionPercentilesRecord, IndividualContributionsRecord) getBoxPlotRecord(
            ExposureTarget target, 
            List<double> samplingWeights, 
            Compound substance, 
            List<double> individualContributions
        ) {
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var meanContribution = individualContributions
                .Zip(samplingWeights, (i, w) => i * w).Sum() / samplingWeights.Sum();
            var percentiles = individualContributions
                .PercentilesWithSamplingWeights(samplingWeights, percentages)
                .ToList();
            var positives = individualContributions
                .Where(r => r > 0)
                .ToList();
            var outliers = positives
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c).ToList();
            var boxPlotRecord = new HbmContributionPercentilesRecord() {
                TargetUnit = target,
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                ExpressionType = target != null && target.ExpressionType != ExpressionType.None
                    ? target.ExpressionType.GetDisplayName() : null,
                ExposureRoute = target != null && target.ExposureRoute != ExposureRouteType.Undefined
                    ? target.ExpressionType.GetDisplayName() : null,
                Percentiles = percentiles.ToList(),
                Percentage = samplingWeights.Count * 100d / individualContributions.Count,
                Outliers = outliers.ToList(),
                NumberOfOutLiers = outliers.Count
            };
            var contributionRecord = new IndividualContributionsRecord() {
                TargetUnit = target,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                ExpressionType = target != null && target.ExpressionType != ExpressionType.None
                    ? target.ExpressionType.GetDisplayName() : null,
                ExposureRoute = target != null && target.ExposureRoute != ExposureRouteType.Undefined
                    ? target.ExpressionType.GetDisplayName() : null,
                Contribution = meanContribution,
                Contributions = new List<double>()
            };
            return (boxPlotRecord, contributionRecord);
        }
    }
}
