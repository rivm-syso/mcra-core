using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CumulativeExposureHazardRatioSectionView : SectionView<CumulativeExposureHazardRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.RestrictedUpperPercentile.HasValue) {
                var upper = Model.RestrictedUpperPercentile.Value;
                var lower = 100 - Model.RestrictedUpperPercentile.Value;
                sb.AppendWarning("This section cannot be rendered because the sample size is insufficient for reporting the selected percentiles in accordance with the privacy guidelines." +
                    $" For the given sample size, only percentile values between p{lower:#0.##} and p{upper:#0.##} can be reported.");
            } else {
                var riskRecords = Model.RiskRecords.SelectMany(c => c.Records).Distinct().ToList();
                var cumulativeRecord = riskRecords.FirstOrDefault(c => c.IsCumulativeRecord);
                cumulativeRecord.BiologicalMatrix = null;
                cumulativeRecord.ExpressionType = null;
                //riskRecords may contain > 1 cumulative records, remove them
                riskRecords = riskRecords.Where(c => !c.IsCumulativeRecord).ToList();
                riskRecords.Add(cumulativeRecord);
                riskRecords = riskRecords
                    .OrderByDescending(c => c.IsCumulativeRecord)
                    .ThenByDescending(c => !double.IsNaN(c.PUpperRiskUncUpper) ? c.PUpperRiskUncUpper : c.PUpperRiskNom)
                    .ToList();

                var isUncertainty = riskRecords
                    .Any(c => c.RiskPercentiles[0].UncertainValues?.Any() ?? false);
                var hiddenProperties = new List<string>();
                if (isUncertainty) {
                    hiddenProperties.Add("PLowerRiskNom");
                    hiddenProperties.Add("RiskP50Nom");
                    hiddenProperties.Add("PUpperRiskNom");
                    hiddenProperties.Add("ProbabilityOfCriticalEffect");
                } else {
                    hiddenProperties.Add("PLowerRiskUncP50");
                    hiddenProperties.Add("RiskP50UncP50");
                    hiddenProperties.Add("PUpperRiskUncP50");
                    hiddenProperties.Add("PLowerRiskUncLower");
                    hiddenProperties.Add("PUpperRiskUncUpper");
                    hiddenProperties.Add("MedianProbabilityOfCriticalEffect");
                    hiddenProperties.Add("LowerProbabilityOfCriticalEffect");
                    hiddenProperties.Add("UpperProbabilityOfCriticalEffect");
                }
                if (riskRecords.All(c => string.IsNullOrEmpty(c.ExpressionType))) {
                    hiddenProperties.Add("BiologicalMatrix");
                    hiddenProperties.Add("ExpressionType");
                }

                var targets = Model.RiskRecords.Select(c => c.Target).ToList();
                sb.Append("<div class=\"figure-container\">");
                sb.AppendChart(
                    name: "CumulativeHazardIndexBySubstanceMedianChart",
                    chartCreator: new CumulativeExposureHazardRatioMedianChartCreator(Model, isUncertainty),
                    fileType: ChartFileType.Svg,
                    section: Model,
                    viewBag: ViewBag,
                    caption: "Cumulative risk (median) in the population.",
                    saveChartFile: true
                );

                sb.AppendChart(
                    name: "CumulativeHazardIndexBySubstanceUpperChart",
                    chartCreator: new CumulativeExposureHazardRatioUpperChartCreator(Model, isUncertainty),
                    fileType: ChartFileType.Svg,
                    section: Model,
                    viewBag: ViewBag,
                    caption: $"Cumulative risk (upper p{Model.UpperBoundConficenceInterval} percentile) in the population.",
                    saveChartFile: true
                );
                sb.Append("</div>");

                sb.AppendTable(
                    Model,
                    riskRecords,
                    "CumulativeHazardIndexBySubstanceTable",
                    ViewBag,
                    caption: $"Percentage at risk by substance.",
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
