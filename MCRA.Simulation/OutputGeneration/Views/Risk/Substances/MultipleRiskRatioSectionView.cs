using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MultipleRiskRatioSectionView : SectionView<MultipleRiskRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.RestrictedUpperPercentile.HasValue) {
                var upper = Model.RestrictedUpperPercentile.Value;
                var lower = 100 - Model.RestrictedUpperPercentile.Value;
                sb.AppendWarning("This section cannot be rendered because the sample size is insufficient for reporting the selected percentiles in accordance with the privacy guidelines." +
                    $" For the given sample size, only percentile values between p{lower:#0.##} and p{upper:#0.##} can be reported.");
            } else {
                var pLower = $"p{(100 - Model.ConfidenceInterval) / 2:F1}";
                var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";
                var riskRecords = Model.MultipleRiskRecords.SelectMany(c => c.Records).ToList();
                var isUncertainty = riskRecords.Any(c => !double.IsNaN(c.PLowerRiskUncLower));

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

                var unstratifiedHiddenProperties = new List<string>(hiddenProperties) { "Stratification" };
                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var targetUnit in Model.TargetUnits) {
                    var target = targetUnit?.Target;
                    var targetCode = target?.Code;
                    var targetName = target?.GetDisplayName() ?? "Overall";
                    var plotRecords = Model.MultipleRiskRecords
                        .SingleOrDefault(c => c.Target == target).Records
                        .OrderBy(c => c.BiologicalMatrix)
                        .ThenBy(c => c.ExpressionType)
                        .ThenByDescending(c => c.ProbabilityOfCriticalEffect)
                        .ThenBy(c => c.GetLabel())
                        .ToList();

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
                            $"RiskBySubstanceChart{targetCode}",
                            Model,
                            plotRecords,
                            ViewBag
                        );
                    var caption = $"Safety chart: each bar shows the variability ({pLower} - {pUpper}) of the risk characterisation ratio ({Model.RiskMetric.GetDisplayName()}) in the population.";
                    if (isUncertainty) {
                        caption = caption
                            + $" The whiskers indicate a composed confidence interval, the left whisker is the"
                            + $" lower {Model.UncertaintyLowerLimit:F1}% limit of {pLower}, the right whisker is the"
                            + $" upper {Model.UncertaintyUpperLimit:F1}% limit of {pUpper}.";
                    }
                    var records = plotRecords
                        .Where(c => string.IsNullOrEmpty(c.Stratification))
                        .OrderByDescending(c => c.IsCumulativeRecord)
                        .ThenByDescending(c => isUncertainty ? c.PUpperRiskUncUpper : c.PUpperRiskNom)
                        .ToList();

                    var panelSb = new StringBuilder();

                    var chartCreator = new MultipleRiskRatioHeatMapCreator(Model, targetUnit, isUncertainty);
                    var chartName = Model.TargetUnits.Count == 1 ? "MarginOfExposureBySubstanceChart" : $"MarginOfExposureBySubstance{targetCode}Chart";
                    panelSb.AppendChart(
                        name: chartName,
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: caption,
                        chartData: percentileDataSection
                    );

                    panelSb.AppendTable(
                        Model,
                        records.Where(c => c.PercentagePositives > 0).ToList(),
                        $"RiskBySubstanceTable{targetCode}",
                        ViewBag,
                        caption: $"Risk statistics by substance.",
                        saveCsv: true,
                        sortable: true,
                        hiddenProperties: unstratifiedHiddenProperties
                    );

                    panelBuilder.AddPanel(
                        id: $"PanelRiskBySubstance{targetCode}",
                        title: targetName,
                        hoverText: targetName,
                        content: new HtmlString(panelSb.ToString())
                    );

                    if (Model.StratifiedMultipleRiskRecords.Count > 0) {
                        var stratifiedRecords = Model.StratifiedMultipleRiskRecords.SingleOrDefault(c => c.Target == target).Records
                            .Where(c => !string.IsNullOrEmpty(c.Stratification))
                            .OrderByDescending(c => c.IsCumulativeRecord)
                            .ThenByDescending(c => isUncertainty ? c.PUpperRiskUncUpper : c.PUpperRiskNom)
                            .ToList();
                        var stratifiedChartCreator = new MultipleStratifiedRiskRatioHeatMapCreator(
                            Model, targetUnit, isUncertainty
                        );
                        panelSb = new StringBuilder();
                        panelSb.AppendChart(
                            name: $"RiskBySubstanceStratified{targetCode}",
                            section: Model,
                            viewBag: ViewBag,
                            chartCreator: stratifiedChartCreator,
                            fileType: ChartFileType.Svg,
                            saveChartFile: true,
                            caption: caption
                        );
                        panelSb.AppendTable(
                            Model,
                            stratifiedRecords.Where(c => c.PercentagePositives > 0).ToList(),
                            $"RiskBySubstanceStratifiedTable{targetCode}",
                            ViewBag,
                            caption: $"Stratified risk statistics by substance.",
                            saveCsv: true,
                            sortable: true,
                            rotate: false,
                            hiddenProperties: hiddenProperties
                        );
                        panelBuilder.AddPanel(
                            id: $"PanelRiskBySubstanceStratified{targetCode}",
                            title: $"{targetName} stratified ",
                            hoverText: $"{targetName} stratified",
                            content: new HtmlString(panelSb.ToString())
                        );
                    }
                }

                // Render unstratified table for all targets
                // Kept for backward compatibility, but not used in the current report
                var allRecords = Model.MultipleRiskRecords
                    .SelectMany(c => c.Records)
                    .OrderByDescending(c => c.IsCumulativeRecord)
                    .ThenByDescending(c => isUncertainty ? c.PUpperRiskUncUpper : c.PUpperRiskNom)
                    .Where(c => string.IsNullOrEmpty(c.Stratification)
                        && c.PercentagePositives > 0)
                    .ToList();
                var combinedRiskBySubstanceDataSection = DataSectionHelper
                .CreateCsvDataSection(
                    Model.RiskMetric == RiskMetricType.ExposureHazardRatio
                        ? "HazardIndexBySubstanceTable" : "MOEBySubstanceTable",
                    Model,
                    allRecords,
                    ViewBag,
                    hiddenProperties: unstratifiedHiddenProperties
                );
                panelBuilder.RenderPanel(sb, collapseSingleTab: true);
            }
        }
    }
}
