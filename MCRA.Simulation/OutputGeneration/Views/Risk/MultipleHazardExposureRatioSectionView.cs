using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.Charting;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MultipleHazardExposureRatioSectionView : SectionView<MultipleHazardExposureRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var pLower = $"p{(100 - Model.ConfidenceInterval) / 2:F1}";
            var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";

            var riskRecords = Model.RiskRecords.SelectMany(c => c.Records).ToList();
            var isUncertainty = riskRecords.Any(c => !double.IsNaN(c.PLowerRiskUncLower) && c.PLowerRiskUncLower > 0);

            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var targetUnit in Model.TargetUnits) {
                var target = targetUnit?.Target;
                var targetCode = target?.Code;
                var targetName = target?.GetDisplayName() ?? "Overall";
                var plotRecords = Model.RiskRecords.SingleOrDefault(c => c.Target == target).Records
                    .OrderBy(c => c.BiologicalMatrix)
                    .ThenBy(c => c.ExpressionType)
                    .ThenByDescending(c => c.ProbabilityOfCriticalEffect)
                    .ToList(); ;

                var numberOfSubstancesZero = plotRecords.Count(c => c.PercentagePositives == 0);
                if (numberOfSubstancesZero > 0) {
                    if (Model.TargetUnits.Count > 1) {
                        sb.AppendWarning($"For {numberOfSubstancesZero} substances no positive exposure is found for {targetUnit.Target.GetDisplayName()}.");
                    } else {
                        sb.AppendWarning($"For {numberOfSubstancesZero} substances no positive exposure is found.");
                    }
                }
                var numberOfRecords = plotRecords.Count;
                var percentileDataSection = DataSectionHelper
                    .CreateCsvDataSection(
                        $"MarginOfExposureBySubstanceChart{targetCode}",
                        Model,
                        plotRecords,
                        ViewBag
                    );
                var caption = $"Safety chart: each bar shows the variability ({pLower} - {pUpper}) of the risk ratio ({Model.RiskMetricType.GetDisplayName()}) in the population.";
                if (isUncertainty) {
                    caption = caption
                        + $" The whiskers indicate a composed confidence interval, the left whisker is the"
                        + $" lower {Model.UncertaintyLowerLimit:F1}% limit of {pLower}, the right whisker is the"
                        + $" upper {Model.UncertaintyUpperLimit:F1}% limit of {pUpper}.";
                }
                IChartCreator chartCreator = new MultipleHazardExposureRatioHeatMapCreator(Model, targetUnit, isUncertainty);
                var chartName = Model.TargetUnits.Count == 1 ? "MarginOfExposureBySubstanceChart" : $"MarginOfExposureBySubstance{targetCode}Chart";
                panelBuilder.AddPanel(
                    id: $"Panel_{targetCode}",
                    title: targetName,
                    hoverText: targetName,
                    content: ChartHelpers.Chart(
                        name: chartName,
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: caption,
                        chartData: percentileDataSection
                    )
                );
            }
            panelBuilder.RenderPanel(sb);

            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("PLowerRiskUncLower");
                hiddenProperties.Add("PUpperRiskUncUpper");
                hiddenProperties.Add("PLowerRiskUncP50");
                hiddenProperties.Add("RiskP50UncP50");
                hiddenProperties.Add("PUpperRiskUncP50");
                hiddenProperties.Add("MedianProbabilityOfCriticalEffect");
                hiddenProperties.Add("LowerProbabilityOfCriticalEffect");
                hiddenProperties.Add("UpperProbabilityOfCriticalEffect");
            } else {
                hiddenProperties.Add("PLowerRiskNom");
                hiddenProperties.Add("RiskP50Nom");
                hiddenProperties.Add("PUpperRiskNom");
                hiddenProperties.Add("ProbabilityOfCriticalEffect");
            }
            if (riskRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
                hiddenProperties.Add("ExpressionType");
            } else if (riskRecords.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                hiddenProperties.Add("ExpressionType");
            }


            riskRecords = riskRecords
                .OrderByDescending(c => c.IsCumulativeRecord)
                .ThenBy(c => isUncertainty ? c.PUpperRiskUncUpper : c.PUpperRiskNom)
                .ToList();

            sb.AppendTable(
                Model,
                riskRecords.Where(c => c.PercentagePositives > 0).ToList(),
                "MOEBySubstanceTable",
                ViewBag,
                caption: $"Risk statistics by substance.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
